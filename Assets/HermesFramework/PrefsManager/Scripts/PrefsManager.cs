using System;
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
        /// <para>valueのtypeは値型だとint,bool,enum,float,stringが使えます</para>
        /// <para>それ以外ではJsonUtilityを用いるため、classを使用して下さい</para>
        /// <para>classの変数はシリアライズ可能な形にして下さい(public、SerializeFieldアトリビュート等)</para>
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Set(string key, object value)
        {
            if (value == null)
                return;
            var type = value.GetType();
            if (type == typeof(int) || type.IsEnum)
                PlayerPrefs.SetInt(key, (int)value);
            else if (type == typeof(bool))
                PlayerPrefs.SetInt(key, Convert.ToInt32((bool)value));
            else if (type == typeof(float))
                PlayerPrefs.SetFloat(key, (float)value);
            else if (type == typeof(string))
                PlayerPrefs.SetString(key, (string)value);
            else
                PlayerPrefs.SetString(key, JsonUtility.ToJson(value));
        }

        /// <summary>
        /// 存在していたら取得、無かったらnull
        /// <para>Tには値型だとint,bool,enum,float,stringが使えます</para>
        /// <para>それ以外ではJsonUtilityを用いるため、classを使用して下さい</para>
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="key">Key</param>
        /// <returns>Value</returns>
        public T Load<T>(string key)
        {
            var type = typeof(T);
            if (type == typeof(int) || type.IsEnum)
                return (T)(object)PlayerPrefs.GetInt(key, 0);
            else if (type == typeof(bool))
                return (T)(object)Convert.ToBoolean(PlayerPrefs.GetInt(key, 0));
            else if (type == typeof(float))
                return (T)(object)PlayerPrefs.GetFloat(key, 0f);
            else if (type == typeof(string))
                return (T)(object)PlayerPrefs.GetString(key, string.Empty);
            return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key, string.Empty));
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 存在していたら削除
        /// </summary>
        /// <param name="key">Key</param>
        public void DeleteKey(string key)
        {
            if (!PlayerPrefs.HasKey(key))
                return;
            PlayerPrefs.DeleteKey(key);
        }

        /// <summary>
        /// 全削除
        /// </summary>
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}