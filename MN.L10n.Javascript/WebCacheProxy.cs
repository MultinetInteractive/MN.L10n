using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace MN.L10n.Javascript
{
    internal class WebCacheProxy : IMemoryCache
    {
        private readonly Cache _cache;

        public WebCacheProxy(Cache cache)
        {
            _cache = cache;
        }

        public void Dispose()
        {
        }

        public bool TryGetValue(object key, out object value)
        {
            if (key is string s)
            {
                value = _cache[s];
                return true;
            }

            value = null;
            return false;
        }

        public ICacheEntry CreateEntry(object key)
        {
            //only used internally so this should be "fine"
            throw new NotImplementedException();
        }

        public void Remove(object key)
        {
            if (key is string s)
            {
                _cache.Remove(s);
            }
        }
    }
}
