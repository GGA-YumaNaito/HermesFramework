using System.Linq;

namespace System.Collections.Generic
{
    /// <summary>
    /// Dictionary extentions.
    /// </summary>
    public static partial class DictionaryExtentions
    {
        public static void RemoveAll<K, V>(this IDictionary<K, V> dict, Func<K, V, bool> match)
        {
            foreach (var key in dict.Keys.ToArray().Where(key => match(key, dict[key])))
                dict.Remove(key);
        }
    }
}