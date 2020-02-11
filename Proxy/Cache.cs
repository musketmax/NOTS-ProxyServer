using ProxyServer.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProxyServer.Proxy
{
    public class Cache
    {
        private ConcurrentDictionary<string, CacheItem> storedItems;

        public Cache()
        {
            storedItems = new ConcurrentDictionary<string, CacheItem>();
        }

        public CacheItem GetCacheItem(string requestLine)
        {
            CacheItem cacheItem;
            if (storedItems.TryGetValue(requestLine, out cacheItem)) return cacheItem;

            return null;
        }

        public void RemoveCacheItem(string requestLine)
        {
            CacheItem cacheItem;
            storedItems.TryRemove(requestLine, out cacheItem);
        }

        public bool IsStored(string requestLine)
        {
            CacheItem cacheItem = GetCacheItem(requestLine);
            return cacheItem != null;
        }

        public void Store(HttpRequest request, HttpResponse response)
        {
            // Only store in cache when there is no cacheItem already for this requestLine.
            if (GetCacheItem(request.FirstLine) == null)
            {
                CacheItem cacheItem = new CacheItem(request, response);
                storedItems.TryAdd(request.FirstLine, cacheItem);
            }
        }
    }
}
