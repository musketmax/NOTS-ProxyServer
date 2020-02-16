using ProxyServer_NOTS.Http;
using System;

namespace ProxyServer_NOTS.Proxy.Caches
{
    public class CacheItem
    {
        public HttpResponse response { get; }
        public DateTime created { get; }
        public byte[] responseInBytes { get; }
        public int timeToLive;


        public CacheItem(HttpRequest request, HttpResponse response)
        {
            this.response = response;
            responseInBytes = response.messageInBytes;
            created = DateTime.UtcNow;
            timeToLive = 0;

            // If request has a Cache-Control header, set our max age to it
            //if (request.hasHeader("Cache-Control") && request.getHeader("Cache-Control").value.Contains("max-age="))
            //{
            //    int.TryParse(request.getHeader("Cache-Control").value.Split('=')[1], out timeToLive);
            //}
        }

        public double getRemainingTime(int cacheTimeOut)
        {
            return cacheTimeOut - (DateTime.UtcNow - created).TotalSeconds;
        }

        public bool isValid(int cacheTimeOut)
        {
            if (timeToLive > 0)
            {
                return ((DateTime.UtcNow - created).TotalSeconds <= timeToLive && (DateTime.UtcNow - created).TotalSeconds <= cacheTimeOut);
            }

            return (DateTime.UtcNow - created).TotalSeconds <= cacheTimeOut;
        }

        public HttpResponse toHttpResponse
        {
            get
            {
                string firstLine = "HTTP/1.1 200 OK";
                HttpResponse httpResponse = new HttpResponse(firstLine, response.headers, response.body, responseInBytes);

                string date = DateTime.Now.ToUniversalTime().ToString("r");

                if (httpResponse.hasHeader("Date"))
                {
                    httpResponse.updateHeader("Date", date);
                }
                else
                {
                    httpResponse.addHeader("Date", date);
                }

                return httpResponse;
            }
        }
    }
}
