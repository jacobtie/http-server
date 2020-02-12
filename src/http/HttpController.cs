using System;
using System.IO;
using System.Threading.Tasks;

namespace http_server.http
{
	// Class to perform the function of different method types
	public class HttpController
	{
		// Field to store the HTTP Request Message
		private HttpRequestMessage _message;

		// Asynchonous method to run the controller with the given message
		public static async Task<HttpResponseMessage> Run(HttpRequestMessage message)
		{
			// Task waits to create a new controller and route the message
			var controller = new HttpController(message);
			return await controller._route();
		}

		// Constructor to create a new HTTP Controller with a request message
		private HttpController(HttpRequestMessage message)
		{
			this._message = message;
		}

		// Asynchronous method to route the message based on the method type
		public async Task<HttpResponseMessage> _route()
		{
			if (_message.Method == HttpMethod.GET)
			{
				return await Get();
			}
			else if (_message.Method == HttpMethod.PUT)
			{
				return await Put();
			}
			else
			{
				throw new Exception("Unrecognized method");
			}
		}

		// Asynchonous method to perform GET functionality
		public async Task<HttpResponseMessage> Get()
		{
			HttpResponseMessage response;

			// If the file does not exists for the message
			if (!File.Exists($"files{_message.Route}"))
			{
				// Respond with file not found
				response = new HttpResponseMessage(HttpResponseCode.NOT_FOUND, null);
			}
			else
			{
				// Task waits to read the file contents
				var fileContents = await File.ReadAllTextAsync($"files{_message.Route}");

				// Respond with an OK message and the contents of the file
				response = new HttpResponseMessage(HttpResponseCode.OK, fileContents);
			}

			return response;
		}

		// Asynchonous method to perform PUT functionality
		private async Task<HttpResponseMessage> Put()
		{
			HttpResponseMessage response;

			// Get the file path
			var filePath = $"files{_message.Route}";

			// Task waits until contents of the message are written to the file
			await File.WriteAllTextAsync(filePath, _message.Body);

			// Respond with an OK message
			response = new HttpResponseMessage(HttpResponseCode.OK, null);

			return response;
		}
	}
}
