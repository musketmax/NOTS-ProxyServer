using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyServer_NOTS.Http
{
    public class HttpRequest : HttpMessage
    {
        public HttpRequest(string firstLine, List<HttpHeader> headers, byte[] body, byte[] requestInBytes) : base(firstLine, headers, body, requestInBytes) { }

        public static HttpRequest parseToHttpRequest(byte[] requestInBytes)
        {
            string messageInString = Encoding.UTF8.GetString(requestInBytes);
            List<string> lines = toLines(messageInString);

            string firstLine = lines[0];

            // Return null if the request or method is from Firefox telemetry
            if (firstLine.Contains("CONNECT") || firstLine.Contains("firefox.com")) return null;

            List<HttpHeader> headers = getHeaders(lines);
            byte[] body = getBody(messageInString);

            if (headers.Count() > 0) return new HttpRequest(firstLine, headers, body, requestInBytes);

            return null;
        }

        public void removeUserAgent()
        {
            if (hasHeader("User-Agent")) removeHeader("User-Agent");
        }

        public bool hasContentToFilter()
        {
            string[] fileTypesToFilter = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".ai", ".eps" };
            return fileTypesToFilter.Any(fileType => firstLine.Split(' ')[1].ToLower().Contains(fileType));
        }

        public bool canCache()
        {
            // Only cache if request is GET
            if (!firstLine.Split(' ')[0].ToLower().Contains("get")) return false;

            // Only cache if request header 'Cache-Control' is not set to private
            if (hasHeader("Cache-Control") && getHeader("Cache-Control").value == "private") return false;

            return true;
        }

        public string getHost()
        {
            if (hasHeader("Host")) return getHeader("Host").value;
            return firstLine.Split(' ')[1];
        }
    }
}
