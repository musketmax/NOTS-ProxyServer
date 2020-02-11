using ProxyServer.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Proxy
{
    public class CacheItem
    {
        public HttpResponse Response { get; }
        public byte[] ResponseInBytes { get; }
        public DateTime DateCached { get; }
        public int MaximumAge;

        public CacheItem(HttpRequest request, HttpResponse response)
        {
            Response = response;
            ResponseInBytes = response.MessageInBytes;
            DateCached = DateTime.UtcNow;
            MaximumAge = 0;

            // If request has a Cache-Control header, set our max age to it
            if (request.HasHeader("Cache-Control") && request.GetHeader("Cache-Control").Value.Contains("max-age="))
            {
                int.TryParse(request.GetHeader("Cache-Control").Value.Split('=')[1], out MaximumAge);
            }
        }

        public double RemainingTime(int cacheTimeOut)
        {
            return cacheTimeOut - (DateTime.UtcNow - DateCached).TotalSeconds;
        }

        public bool IsValid(int cacheTimeOut)
        {
            if (MaximumAge > 0)
            {
                return ((DateTime.UtcNow - DateCached).TotalSeconds <= MaximumAge && (DateTime.UtcNow - DateCached).TotalSeconds <= cacheTimeOut);
            }

            return (DateTime.UtcNow - DateCached).TotalSeconds <= cacheTimeOut;
        }

        public HttpResponse ToResponse
        {
            get
            {
                string firstLine = "HTTP/1.1 200 OK (By Proxy Cache)";
                HttpResponse httpResponse = new HttpResponse(firstLine, Response.Headers, Response.Body, ResponseInBytes);

                string date = DateTime.Now.ToUniversalTime().ToString("r");
                if (httpResponse.HasHeader("Date"))
                {
                    httpResponse.UpdateHeader("Date", date);
                }
                else
                {
                    httpResponse.AddHeader("Date", date);
                }

                return httpResponse;
            }
        }
    }
}
