using System.Collections;
using System.Collections.Generic;

namespace MN.L10n.SystemWeb
{
    public class WebLanguageScopeContainer : IDictionary<object, object>
    {
        private readonly Hashtable _httpContextItems;
        public WebLanguageScopeContainer(Hashtable httpContextItems)
        {
            _httpContextItems = httpContextItems;
        }

        public object this[object key] { get => _httpContextItems[key]; set => _httpContextItems[key] = value; }

        public ICollection<object> Keys {
            get {
                var keys = new List<object>();
                foreach (var key in _httpContextItems.Keys)
                {
                    keys.Add(key);
                }

                return keys;
            }
        }

        public ICollection<object> Values {
            get {
                var values = new List<object>();
                foreach (var value in _httpContextItems.Values)
                {
                    values.Add(value);
                }

                return values;
            }
        }

        public int Count => _httpContextItems.Count;

        public bool IsReadOnly => _httpContextItems.IsReadOnly;

        public void Add(object key, object value)
        {
            _httpContextItems.Add(key, value);
        }

        public void Add(KeyValuePair<object, object> item)
        {
            _httpContextItems.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _httpContextItems.Clear();
        }

        public bool Contains(KeyValuePair<object, object> item)
        {
            return TryGetValue(item.Key, out var value) && value == item.Value;
        }

        public bool ContainsKey(object key)
        {
            return _httpContextItems.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex)
        {
            _httpContextItems.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
        {
            var keyValueDic = new List<KeyValuePair<object, object>>();
            foreach (DictionaryEntry item in _httpContextItems)
            {
                keyValueDic.Add(new KeyValuePair<object, object>(item.Key, item.Value));
            }

            return keyValueDic.GetEnumerator();
        }

        public bool Remove(object key)
        {
            if (_httpContextItems.ContainsKey(key))
            {
                _httpContextItems.Remove(key);
                return true;
            }

            return false;
        }

        public bool Remove(KeyValuePair<object, object> item)
        {
            if (TryGetValue(item.Key, out var value) && value == item.Value)
            {
                _httpContextItems.Remove(item.Key);
                return true;
            }

            return false;
        }

        public bool TryGetValue(object key, out object value)
        {
            if (_httpContextItems.ContainsKey(key))
            {
                value = _httpContextItems[key];
                return true;
            }

            value = null;
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _httpContextItems.GetEnumerator();
        }
    }
}
