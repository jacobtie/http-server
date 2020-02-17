using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace http_server.http
{
	// Singleton Class to perform the function of different method types
	public class HttpController
	{
		private static HttpController? _controller;
		// Field to store the mutex
		private SemaphoreSlim _mutex;

		// Asynchonous method to run the controller with the given message
		public static async Task<HttpResponseMessage> Run(HttpRequestMessage message)
		{
			// Lazily instantiate the controller singleton
			if (_controller is null)
			{
				_controller = new HttpController();
			}
			// Task routes the message
			return await _controller._route(message);
		}

		// Constructor to create a new HTTP Controller
		private HttpController()
		{
			_mutex = new SemaphoreSlim(1, 1);
		}

		// Asynchronous method to route the message based on the method type
		public async Task<HttpResponseMessage> _route(HttpRequestMessage message)
		{
			if (message.Method == HttpMethod.GET)
			{
				return await Get(message);
			}
			else if (message.Method == HttpMethod.PUT)
			{
				return await Put(message);
			}
			else
			{
				throw new Exception("Unrecognized method");
			}
		}

		// Asynchonous method to perform GET functionality
		public async Task<HttpResponseMessage> Get(HttpRequestMessage message)
		{
			HttpResponseMessage response;

			// If the file does not exists for the message
			if (!File.Exists($"files{message.Route}"))
			{
				// Respond with file not found
				response = new HttpResponseMessage(HttpResponseCode.NOT_FOUND, null);
			}
			else
			{
				await _mutex.WaitAsync();
				// Task waits to read the file contents
				var fileContents = await File.ReadAllTextAsync($"files{message.Route}");
				_mutex.Release();

				// Respond with an OK message and the contents of the file
				response = new HttpResponseMessage(HttpResponseCode.OK, fileContents);
			}

			return response;
		}

		// Asynchonous method to perform PUT functionality
		private async Task<HttpResponseMessage> Put(HttpRequestMessage message)
		{
			HttpResponseMessage response;

			// Get the file path
			var filePath = $"files{message.Route}";

			await _mutex.WaitAsync();
			// Task waits until contents of the message are written to the file
			await File.WriteAllTextAsync(filePath, message.Body);
			_mutex.Release();

			// Respond with an OK message
			response = new HttpResponseMessage(HttpResponseCode.OK_FILE_CREATED, null);

			return response;
		}
	}
}
