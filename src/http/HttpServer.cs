using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace http_server.http
{
	public class HttpServer
	{
		private const int PORT = 5000;
		private const int RESPONSE_BUFFER_SIZE = 512;
		private const string HOST = "localhost";
		private Socket _listener;

		public static async Task Start()
		{
			await new HttpServer()._runServer();
		}

		private HttpServer()
		{
			this._listener = _initListener();
		}

		private Socket _initListener()
		{
			var ipHostInfo = Dns.GetHostEntry(HOST);
			IPAddress? ipAddress = ipHostInfo.AddressList.First(
				address => address.AddressFamily == AddressFamily.InterNetwork);

			if (ipAddress == null)
			{
				throw new Exception("No IPv4 Address found");
			}

			var localEndPoint = new IPEndPoint(ipAddress, PORT);

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind(localEndPoint);
			socket.Listen(15);

			return socket;
		}

		private async Task _runServer()
		{
			Console.WriteLine($"Running server on {HOST}:{PORT}");
			while (true)
			{
				var handler = await _listener.AcceptAsync();
				var task = Task.Run(() => _handleRequest(handler));
			}
		}

		private async Task _handleRequest(Socket socket)
		{
			Console.WriteLine("Handling request");
			var requestMessage = await _readRequest(socket);

			var request = HttpRequestMessage.FromMessage(requestMessage);

			Console.WriteLine(request.AsString());
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
