using UnityEngine;

namespace Hermes.Prefs
{
    /// <summary>
    /// 設定マネージャー
    /// <para>PlayerPrefsとJsonUtilityを使ってデータを格納しています</para>
    /// <para>PlayerPrefsのSaveはOnApplicationQuitにて自動で呼び出されますが、アプリがクラッシュした時は保存されない可能性があります</para>
    /// <para>クラッシュ対策をする場合、SetやDeleteをした後にSaveを呼び出して下さい</para>
    /// <para>(Set or Delete)とSaveが分けている理由は、PlayerPrefsの保存先は一つのファイルなので頻繁に書き換えを行いたくないためです</para>
    /// </summary>
    public class PrefsManager : SingletonMonoBehaviour<PrefsManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>
        /// 値があったら一時保存
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public static void Set(string key, object value)
        {
            if (value == null)
                return;
            var json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, json);
        }

        /// <summary>
        /// 存在していたら取得、無かったらnull
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public static T Load<T>(string key)
        {
            var json = PlayerPrefs.GetString(key, string.Empty);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 存在していたら削除
        /// </summary>
        /// <param name="key">Key</param>
        public static void DeleteKey(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return;
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// 全削除
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}