using System;
using System.IO;
using System.Threading.Tasks;

namespace http_server.http
{
	public class HttpController
	{
		private HttpRequestMessage _message;

		public static async Task<HttpResponseMessage> Run(HttpRequestMessage message)
		{
			var controller = new HttpController(message);
			return await controller._route();
		}

		private HttpController(HttpRequestMessage message)
		{
			this._message = message;
		}

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

		public async Task<HttpResponseMessage> Get()
		{
			HttpResponseMessage response;
			if (!File.Exists($"files{_message.Route}"))
			{
				response = new HttpResponseMessage(HttpResponseCode.NOT_FOUND, null);
			}
			else
			{
				var fileContents = await File.ReadAllTextAsync($"files{_message.Route}");

				response = new HttpResponseMessage(HttpResponseCode.OK, fileContents);
			}

			return response;
		}

		private async Task<HttpResponseMessage> Put()
		{
			HttpResponseMessage response;
			var filePath = $"files{_message.Route}";

			await File.WriteAllTextAsync(filePath, _message.Body);

			response = new HttpResponseMessage(HttpResponseCode.OK, null);

			return response;
		}
	}
}
