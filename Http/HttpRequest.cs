using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Http
{
    public class HttpRequest : HttpMessage
    {
        public HttpRequest(string firstLine, List<HttpHeader> headers, byte[] body, byte[] requestInBytes) : base(firstLine, headers, body, requestInBytes) { }

        public static HttpRequest TryParse(byte[] requestBytes)
        {
            string requestString = Encoding.UTF8.GetString(requestBytes);
            List<string> requestLines = ToLines(requestString);

            string firstLine = requestLines[0];
            List<HttpHeader> headers = ReadHeaders(requestLines);
            byte[] body = ReadBody(requestString);

            if (headers.Count() > 0) return new HttpRequest(firstLine, headers, body, requestBytes);

            return null;
        }

        public void HideUserAgent()
        {
            if (HasHeader("User-Agent"))
                RemoveHeader("User-Agent");
        }

        public bool HasContentToFilter()
        {
            string[] fileTypesToFilter = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".ai", ".eps" };
            return fileTypesToFilter.Any(fileType => FirstLine.Split(' ')[1].ToLower().Contains(fileType));
        }

        public bool IsCacheable()
        {
            // If request isn't a GET request, request is not cacheable.
            if (!FirstLine.Split(' ')[0].ToLower().Contains("get")) return false;
            // If request has a 'Cache-Control: private' header, request is not cacheable.
            if (HasHeader("Cache-Control") && GetHeader("Cache-Control").Value == "private") return false;

            return true;
        }

        public string GetHost()
        {
            if (HasHeader("Host")) return GetHeader("Host").Value;

            return FirstLine.Split(' ')[1];
        }
    }
}
