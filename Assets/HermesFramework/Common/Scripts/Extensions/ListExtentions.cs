namespace System.Collections.Generic
{
    /// <summary>
    /// List extentions.
    /// </summary>
    public static partial class ListExtentions
    {
        public static void AddUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }

        public static void AddRangeUnique<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
                list.AddUnique(item);
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// 末尾にあるオブジェクトを削除せずに返します.
        /// </summary>
        public static T Peek<T>(this IList<T> self)
        {
            return self[self.Count - 1];
        }

        /// <summary>
        /// 末尾にあるオブジェクトを削除し、返します.
        /// </summary>
        public static T Pop<T>(this IList<T> self)
        {
            int index = self.Count - 1;
            var result = self[index];
            self.RemoveAt(index);
            return result;
        }

        /// <summary>
        /// 末尾にオブジェクトを追加します.
        /// </summary>
        public static void Push<T>(this IList<T> self, T item)
        {
            self.Add(item);
        }
    }
}