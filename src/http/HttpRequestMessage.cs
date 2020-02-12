using System;
using System.Text;
using System.Collections.Generic;

namespace http_server.http
{
	public class HttpRequestMessage
	{
		public HttpMethod Method { get; set; }
		public string Route { get; set; }
		public string Version { get; set; }
		public Dictionary<string, string> Headers { get; set; }
		public string? Body { get; set; }

		public HttpRequestMessage(HttpMethod method, string host, string route, string? body)
		{
			this.Method = method;
			this.Route = route;
			this.Version = "HTTP/1.1";
			this.Body = body;
			this.Headers = new Dictionary<string, string>();
			_initDefaultHeaders(host);
		}

		public HttpRequestMessage(HttpMethod method, string route, string version, Dictionary<string, string> headers, string? body)
		{
			this.Method = method;
			this.Route = route;
			this.Version = version;
			this.Headers = headers;
			this.Body = body;
		}

		private void _initDefaultHeaders(string host)
		{
			this.SetHeaderValue("User-Agent", "ReynoldsAgent/0.1.0");
			this.SetHeaderValue("Accept", "*/*");
			this.SetHeaderValue("Cache-Control", "no-cache");
			this.SetHeaderValue("Host", host);
			this.SetHeaderValue("Connection", "keep-alive");
		}

		public static HttpRequestMessage FromMessage(string message)
		{
			var lines = message.Split("\r\n");

			var firstLineSplit = lines[0].Split(' ');

			var methodAsString = firstLineSplit[0];

			HttpMethod method;
			if (methodAsString.Equals("get", StringComparison.InvariantCultureIgnoreCase))
			{
				method = HttpMethod.GET;
			}
			else if (methodAsString.Equals("put", StringComparison.InvariantCultureIgnoreCase))
			{
				method = HttpMethod.PUT;
			}
			else
			{
				throw new Exception("Method not recognized");
			}

			var route = firstLineSplit[1];
			var version = firstLineSplit[2];

			lines = lines[1..];

			var headers = new Dictionary<string, string>();
			int lastHeaderLine = -1;
			for (int i = 0; i < lines.Length; i++)
			{
				if (lines[i].Equals(""))
				{
					lastHeaderLine = i;
					break;
				}
				var headerValue = lines[i].Split(": ");
				headers.Add(headerValue[0], headerValue[1]);
			}

			string? body = null;
			if (method == HttpMethod.PUT)
			{
				lines = lines[(lastHeaderLine + 1)..];
				body = string.Join('\n', lines);
			}

			return new HttpRequestMessage(method, route, version, headers, body);
		}

		public void SetHeaderValue(string header, string value)
		{
			if (this.Headers.ContainsKey(header))
			{
				this.Headers[header] = value;
			}
			else
			{
				this.Headers.Add(header, value);
			}
		}

		public string GetHeaderValue(string header)
		{
			if (this.Headers.ContainsKey(header))
			{
				return this.Headers[header];
			}
			else
			{
				throw new KeyNotFoundException("Header not found");
			}
		}

		public string AsString()
		{
			var sb = new StringBuilder();
			sb.Append(this.Method.ToString("G"));
			sb.Append(" ");
			sb.Append(this.Route);
			sb.Append(" ");
			sb.Append(this.Version);
			sb.Append("\r\n");

			foreach (var (header, value) in this.Headers)
			{
				sb.Append($"{header}: {value}\r\n");
			}

			sb.Append("\r\n");
			if (!(this.Body is null))
			{
				sb.Append(this.Body);
			}

			return sb.ToString();
		}
	}
}
