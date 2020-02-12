using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace http_server.http
{
	// Class to run the HTTP server
	public class HttpServer
	{
		// Creation of fields for the server
		private const int RESPONSE_BUFFER_SIZE = 512;
		private const string HOST = "localhost";
		private Socket _listener;
		private int _port;

		// Asynchronous method to start the server on the given port
		public static async Task Start(int port)
		{
			await new HttpServer(port)._runServer();
		}

		// Constructor the create a HTTP server on the given port
		private HttpServer(int port)
		{
			this._port = port;
			this._listener = _initListener();
		}

		// Method to get the listener socket
		private Socket _initListener()
		{
			// Get the info about the host IP address
			var ipHostInfo = Dns.GetHostEntry(HOST);

			// Get only the IPv4 addresses
			IPAddress? ipAddress = ipHostInfo.AddressList.FirstOrDefault(
				address => address.AddressFamily == AddressFamily.InterNetwork);

			// If there are no valid IPv4 addresses
			if (ipAddress == null)
			{
				throw new Exception("No IPv4 Address found");
			}

			// Get the local endpoint of the IP address through the given port
			var localEndPoint = new IPEndPoint(ipAddress, _port);

			// Create a new socket
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Bind the socket to the local endpoint and set max pending connections
			socket.Bind(localEndPoint);
			socket.Listen(15);

			return socket;
		}

		// Asynchronous method to run the core of the server
		private async Task _runServer()
		{
			// Get the cancelation token to shut down the server
			var cancellationToken = _initCancellationToken();
			Console.WriteLine($"Running server on {HOST}:{_port} Press Ctrl+c to stop");

			// Try to listen for request until the server is cancelled
			try
			{
				// Using the cancellation token to dispose of the socket
				using (cancellationToken.Register(() => _listener.Dispose()))
				{
					// While the server is not cancel
					while (!cancellationToken.IsCancellationRequested)
					{
						// Task waits for the listener to get a request
						var handler = await _listener.AcceptAsync();

						// If the request was accepted successfully
						if (handler != null)
						{
							// Create a task to handle the request
							var task = _handleRequest(handler);
						}
					}
				}
			}
			catch
			{
				Console.WriteLine("Shutting down server");
			}
		}

		// Method to create a cancellation token to stop the server
		private CancellationToken _initCancellationToken()
		{
			// Create a new cancellation token
			var cancellationToken = new CancellationTokenSource();

			// Add function to handle ctrl+c 
			Console.CancelKeyPress += (sender, args) =>
			{
				args.Cancel = true;
				cancellationToken.Cancel();
			};

			return cancellationToken.Token;
		}

		// Method to handle a request 
		private async Task _handleRequest(Socket socket)
		{
			Console.WriteLine("Handling request");

			// Task waits to read the request from the socket
			var requestMessage = await _readRequest(socket);

			// Create the HTTP request message from the received message
			var request = HttpRequestMessage.FromMessage(requestMessage);

			// Task waits to get the response based on the request
			HttpResponseMessage response;
			response = await HttpController.Run(request);

			// Encode the response to bytes 
			var encodedResponse = Encoding.ASCII.GetBytes(response.ToString());

			// Task waits for the socket to send the response
			await socket.SendAsync(encodedResponse, SocketFlags.None);

			// Dispose of the socket
			socket.Dispose();
		}

		// Method to read the request from the socket
		private async Task<string> _readRequest(Socket socket)
		{
			// Variables to read the request from the socket
			var bytesReceived = new List<byte>();
			int bytes = 0;
			var responseBuffer = new byte[RESPONSE_BUFFER_SIZE];

			// Do while there are still bytes to be received
			do
			{
				// Get the number of bytes to be received
				bytes = await socket.ReceiveAsync(responseBuffer, SocketFlags.None);
				
				// Receive the number of bytes
				bytesReceived.AddRange(responseBuffer.Take(bytes));

				// Task waits to allow all bytes to be received
				await Task.Delay(2);
			}
			while (socket.Available > 0 && bytes > 0);

			return Encoding.ASCII.GetString(bytesReceived.ToArray());
		}
	}
}
