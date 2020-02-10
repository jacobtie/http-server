using System;
using System.Threading.Tasks;
using http_server.http;

namespace http_server
{
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length != 1)
			{
				Console.WriteLine("Missing port number");
			}
			else
			{
				int port;
				if (Int32.TryParse(args[0], out port))
				{
					await HttpServer.Start(port);
				}
				else
				{
					Console.WriteLine("Invalid port number");
				}
			}
		}
	}
}
