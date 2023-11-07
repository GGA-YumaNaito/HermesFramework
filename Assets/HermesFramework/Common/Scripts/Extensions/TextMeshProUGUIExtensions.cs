using Hermes.Localize;

namespace TMPro
{
    /// <summary>
    /// TextMeshProUGUIの拡張メソッドを管理するクラス
    /// </summary>
    public static partial class TextMeshProUGUIExtensions
    {
        /// <summary>
        /// 値があったらテキストを設定します
        /// <para>keyにはLocalizeのKeyを入れて下さい</para>
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="key">key</param>
        public static void SetTextLocalize(this TextMeshProUGUI text, string key)
        {
            if (!key.IsNullOrEmpty())
                text.text = LocalizeManager.Instance.GetValue(key);
        }

        /// <summary>
        /// 値があったらテキストを設定します(Format)
        /// <para>keyにはLocalizeのKeyを入れて下さい</para>
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="key">key</param>
        /// <param name="args">args</param>
        public static void SetTextLocalize(this TextMeshProUGUI text, string key, params object[] args)
        {
            if (!key.IsNullOrEmpty())
                text.text = LocalizeManager.Instance.GetValue(key, args);
        }

        /// <summary>
        /// 値があったらテキストを設定し、なかったら非活性にする
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="value">value</param>
        public static void SetTextOrInactive(this TextMeshProUGUI text, string value)
        {
            if (value.IsNullOrEmpty())
                text.gameObject.SetActive(false);
            else
                text.text = value;
        }

        /// <summary>
        /// 値があったらテキストを設定し、なかったら非活性にする(Format)
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="value">value</param>
        /// <param name="args">args</param>
        public static void SetTextOrInactive(this TextMeshProUGUI text, string value, params object[] args)
        {
            if (value.IsNullOrEmpty())
                text.gameObject.SetActive(false);
            else
                text.text = string.Format(value, args);
        }

        /// <summary>
        /// 値があったらテキストを設定し、なかったら非活性にする
        /// <para>keyにはLocalizeのKeyを入れて下さい</para>
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="key">key</param>
        public static void SetTextLocalizeOrInactive(this TextMeshProUGUI text, string key)
        {
            if (key.IsNullOrEmpty())
                text.gameObject.SetActive(false);
            else
                text.text = LocalizeManager.Instance.GetValue(key);
        }

        /// <summary>
        /// 値があったらテキストを設定し、なかったら非活性にする(Format)
        /// <para>keyにはLocalizeのKeyを入れて下さい</para>
        /// </summary>
        /// <param name="text">TextMeshProUGUI</param>
        /// <param name="key">key</param>
        /// <param name="args">args</param>
        public static void SetTextLocalizeOrInactive(this TextMeshProUGUI text, string key, params object[] args)
        {
            if (key.IsNullOrEmpty())
                text.gameObject.SetActive(false);
            else
                text.text = LocalizeManager.Instance.GetValue(key, args);
        }
    }
}