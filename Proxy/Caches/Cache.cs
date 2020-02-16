using ProxyServer_NOTS.Http;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ProxyServer_NOTS.Proxy.Caches
{
    public class Cache
    {
        private ConcurrentDictionary<string, CacheItem> items;

        public Cache()
        {
            items = new ConcurrentDictionary<string, CacheItem>();
        }

        public void clear()
        {
            items.Clear();
        }

        public CacheItem getItem(string key)
        {
            CacheItem cacheItem;
            if (items.TryGetValue(key, out cacheItem)) return cacheItem;

            return null;
        }
        
        public ObservableCollection<ListBoxItem> getItemsForListbox()
        {
            ObservableCollection<ListBoxItem> result = new ObservableCollection<ListBoxItem>();

            foreach (var item in items)
            {
                string key = item.Key;
                result.Add(new ListBoxItem { Content = key });
            }

            return result;
        }

        public void removeItem(string key)
        {
            CacheItem cacheItem;
            items.TryRemove(key, out cacheItem);
        }

        public bool isStored(string key)
        {
            CacheItem cacheItem = getItem(key);
            return cacheItem != null;
        }

        public void store(HttpRequest request, HttpResponse response)
        {
            // Only store in cache when there is no cacheItem already for this requestLine.
            if (getItem(request.firstLine) == null)
            {
                CacheItem cacheItem = new CacheItem(request, response);
                items.TryAdd(request.firstLine, cacheItem);
            }
        }
    }
}
