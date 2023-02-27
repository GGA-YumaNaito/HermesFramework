using System;

namespace Hermes
{
    /// <summary>
    /// 配列の拡張メソッドを管理するクラス
    /// </summary>
    public static partial class ArrayExtensions
    {
        /// <summary>
        /// 指定された配列内の各要素に対して、指定された処理を実行します
        /// </summary>
        /// <typeparam name="T">配列要素の型</typeparam>
        /// <param name="array">要素に処理を適用する、インデックス番号が 0 から始まる 1 次元の Array</param>
        /// <param name="action">array の各要素に対して実行する Action<T></param>
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach<T>(array, obj => action(obj));
        }
    }
}