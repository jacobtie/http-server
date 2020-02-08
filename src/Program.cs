using System;
using System.Threading.Tasks;
using http_server.http;

namespace http_server
{
	class Program
	{
		static async Task Main(string[] args)
		{
			await HttpServer.Start();

			Console.WriteLine("Press any key to exit...");
			Console.Read();
		}
	}
}
