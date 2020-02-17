using System;
using System.Collections.Generic;
using System.Text;

namespace http_server.http
{
	// Class to create a HTTP Response Message
	public class HttpResponseMessage
	{
		// Basic getters and setters
		public string Version { get; set; }
		public HttpResponseCode Code { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public string? Body { get; set; }

		// Constructor to create a HTTP Response Message with code and body
		public HttpResponseMessage(HttpResponseCode code, string? body)
		{
			Version = "HTTP/1.1";
			Code = code;
			Headers = new Dictionary<string, string>();
			Body = body;

			// Initialize the headers
			_initHeaders();
		}

		// Method to initialize the headers of the response message
		private void _initHeaders()
		{
			Headers.Add("Date", DateTime.Now.ToString("r"));
			Headers.Add("Server", "Mason/1.0.0");
			Headers.Add("Connection", "close");
			if (Body != null)
			{
				Headers.Add("Content-Type", "text/plain");
				Headers.Add("Content-Length", Body.Length.ToString());
			}
		}

		// Method to carefully format the contents of the message as a particular string
		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.Append(Version);
			sb.Append(" ");

			switch (Code)
			{
				case HttpResponseCode.OK:
					sb.Append("200 OK\r\n");
					break;
				case HttpResponseCode.OK_FILE_CREATED:
					sb.Append("200 OK File Created\r\n");
					break;
				case HttpResponseCode.NOT_FOUND:
					sb.Append("404 Not Found\r\n");
					break;
				default:
					throw new Exception("Invalid status code");
			}

			foreach (var (header, value) in Headers)
			{
				sb.Append($"{header}: {value}\r\n");
			}

			sb.Append("\r\n");

			if (Body != null)
			{
				sb.Append(Body);
			}

			return sb.ToString();
		}
	}
}
