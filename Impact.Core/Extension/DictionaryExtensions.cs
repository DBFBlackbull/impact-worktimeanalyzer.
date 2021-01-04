using System.Collections.Generic;

namespace Impact.Core.Extension
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueSafely<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue)
        {
            if (key == null)
                return defaultValue;

            return dic.TryGetValue(key, out var ret) 
                ? ret 
                : defaultValue;
        }
    }
}