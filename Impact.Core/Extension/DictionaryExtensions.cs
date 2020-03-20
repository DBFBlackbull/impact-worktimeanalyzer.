using System.Collections.Generic;

namespace Impact.Core.Extension
{
    public static class DictionaryExtensions
    {
        public static TValue GetValueSafely<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key)
        {
            return dic.GetValueSafely(key, default(TValue));
        }

        public static TValue GetValueSafely<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue defaultValue)
        {
            if (dic == null || key == null)
                return defaultValue;

            return dic.TryGetValue(key, out var ret) 
                ? ret 
                : defaultValue;
        }
    }
}