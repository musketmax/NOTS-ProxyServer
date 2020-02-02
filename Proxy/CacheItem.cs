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
        public DateTime CachedDate { get; }
        public int MaxAge;

        public CacheItem(HttpRequest request, HttpResponse response)
        {
            Response = response;
            ResponseInBytes = response.MessageInBytes;
            CachedDate = DateTime.UtcNow;
            MaxAge = 0;

            // If request has MaxAge header, set MaxAge to this value.
            if (request.HasHeader("Cache-Control") && request.GetHeader("Cache-Control").Value.Contains("max-age="))
            {
                int.TryParse(request.GetHeader("Cache-Control").Value.Split('=')[1], out MaxAge);
            }
        }

        public bool IsValid(int cacheTimeOut)
        {
            if (MaxAge > 0)
            {
                return ((DateTime.UtcNow - CachedDate).TotalSeconds <= MaxAge && (DateTime.UtcNow - CachedDate).TotalSeconds <= cacheTimeOut);
            }

            return (DateTime.UtcNow - CachedDate).TotalSeconds <= cacheTimeOut;
        }

        public double RemainingTime(int cacheTimeOut)
        {
            return cacheTimeOut - (DateTime.UtcNow - CachedDate).TotalSeconds;
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
