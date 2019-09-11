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
        private ConcurrentDictionary<string, CacheItem> _storedItems;

        public Cache()
        {
            _storedItems = new ConcurrentDictionary<string, CacheItem>();
        }

        public CacheItem GetCacheItem(string requestLine)
        {
            CacheItem cacheItem;
            if (_storedItems.TryGetValue(requestLine, out cacheItem)) return cacheItem;

            return null;
        }

        public void RemoveCacheItem(string requestLine)
        {
            CacheItem cacheItem;
            _storedItems.TryRemove(requestLine, out cacheItem);
        }

        public bool IsStoredInCache(string requestLine)
        {
            CacheItem cacheItem = GetCacheItem(requestLine);

            // If no cacheItems are found, return false.
            if (cacheItem != null) return true;

            return false;
        }

        public void TryStoreInCache(HttpRequest httpRequest, HttpResponse httpResponse)
        {
            // Only store in cache when there is no cacheItem already for this requestLine.
            if (GetCacheItem(httpRequest.FirstLine) == null)
            {
                CacheItem cacheItem = new CacheItem(httpRequest, httpResponse);
                _storedItems.TryAdd(httpRequest.FirstLine, cacheItem);
            }
        }
    }
}
