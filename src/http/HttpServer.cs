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
	public class HttpServer
	{
		private const int RESPONSE_BUFFER_SIZE = 512;
		private const string HOST = "localhost";
		private Socket _listener;
		private int _port;

		public static async Task Start(int port)
		{
			await new HttpServer(port)._runServer();
		}

		private HttpServer(int port)
		{
			this._port = port;
			this._listener = _initListener();
		}

		private Socket _initListener()
		{
			var ipHostInfo = Dns.GetHostEntry(HOST);
			IPAddress? ipAddress = ipHostInfo.AddressList.FirstOrDefault(
				address => address.AddressFamily == AddressFamily.InterNetwork);

			if (ipAddress == null)
			{
				throw new Exception("No IPv4 Address found");
			}

			var localEndPoint = new IPEndPoint(ipAddress, _port);

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(localEndPoint);
			socket.Listen(15);

			return socket;
		}

		private async Task _runServer()
		{
			var cancellationToken = _initCancellationToken();
			Console.WriteLine($"Running server on {HOST}:{_port} Press Ctrl+c to stop");
			try
			{
				using (cancellationToken.Register(() => _listener.Dispose()))
				{
					while (!cancellationToken.IsCancellationRequested)
					{
						var handler = await _listener.AcceptAsync();
						if (handler != null)
						{
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

		private CancellationToken _initCancellationToken()
		{
			var cancellationToken = new CancellationTokenSource();
			Console.CancelKeyPress += (sender, args) =>
			{
				args.Cancel = true;
				cancellationToken.Cancel();
			};

			return cancellationToken.Token;
		}

		private async Task _handleRequest(Socket socket)
		{
			Console.WriteLine("Handling request");
			var requestMessage = await _readRequest(socket);

			var request = HttpRequestMessage.FromMessage(requestMessage);

			HttpResponseMessage response;

			response = await HttpController.Run(request);

			var encodedRequest = Encoding.ASCII.GetBytes(response.ToString());

			await socket.SendAsync(encodedRequest, SocketFlags.None);

			socket.Dispose();
		}

		private async Task<string> _readRequest(Socket socket)
		{
			var bytesReceived = new List<byte>();
			int bytes = 0;
			var responseBuffer = new byte[RESPONSE_BUFFER_SIZE];

			do
			{
				bytes = await socket.ReceiveAsync(responseBuffer, SocketFlags.None);
				bytesReceived.AddRange(responseBuffer.Take(bytes));
				await Task.Delay(2);
			}
			while (socket.Available > 0 && bytes > 0);

			return Encoding.ASCII.GetString(bytesReceived.ToArray());
		}
	}
}
