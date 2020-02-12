using System;
using System.Threading.Tasks;
using http_server.http;

namespace http_server
{
	class Program
	{
		// Method to parse the arguments and run the server
		static async Task Main(string[] args)
		{
			// If the number of arguments is not one
			if (args.Length != 1)
			{
				Console.WriteLine("Missing port number");
			}
			else
			{
				// Try to parse an integer for the port
				int port;
				if (Int32.TryParse(args[0], out port))
				{
					// Task waits until the HTTP server has finished
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
