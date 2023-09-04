using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PSXPrev.Common.Utils
{
    public static class CollectionExtensions
    {
        public static void AddToCounter<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int inc = 1)
        {
            dictionary.TryGetValue(key, out var count);
            dictionary[key] = count + inc;
        }

        public static string DictionaryToString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dictionary, Func<TKey, string> keyToString = null, Func<TValue, string> valueToString = null, IComparer<TKey> orderBy = null, bool padKey = true)
        {
            if (!dictionary.Any())
            {
                return string.Empty;
            }
            var str = new StringBuilder();
            var maxLength = 0;
            if (padKey)
            {
                maxLength = dictionary.Select(kvp => (keyToString?.Invoke(kvp.Key) ?? kvp.Key.ToString()).Length).Max();
            }
            var items = dictionary;
            if (orderBy != null)
            {
                items = items.OrderBy(kvp => kvp.Key, orderBy);
            }
            var first = true;
            foreach (var kvp in items)
            {
                if (!first)
                {
                    str.AppendLine();
                }
                first = false;
                var key   = (keyToString?.Invoke(kvp.Key)     ?? kvp.Key.ToString());
                var value = (valueToString?.Invoke(kvp.Value) ?? kvp.Value.ToString());
                str.Append($"{key.PadLeft(maxLength)}: {value}");
            }
            return str.ToString();
        }
    }
}
