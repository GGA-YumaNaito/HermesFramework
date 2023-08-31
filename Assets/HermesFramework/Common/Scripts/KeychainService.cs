using System.Runtime.InteropServices;

namespace Hermes.Save
{
    /// <summary>
    /// 実装は <c>Assets/Plugins/iOS/KeychainService.mm</c> に記載
    /// </summary>
    /// <remarks>
    /// <a href="https://developer.apple.com/documentation/security/keychain_services">Keychain Services</a>
    /// </remarks>
    public class KeychainService
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int addItem(string dataType, string value);

        [DllImport("__Internal")]
        private static extern string getItem(string dataType);

        [DllImport("__Internal")]
        private static extern int deleteItem(string dataType);
#endif

        /// <summary>
        /// 追加
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns>成功, 失敗</returns>
        public bool Put(string key, string value)
        {
#if UNITY_IOS
            // 返却されるステータスが 0 なら成功
            return addItem(key, value) == 0;
#else
        return false;
#endif
        }

        /// <summary>
        /// 取得
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>値</returns>
        public string Get(string key)
        {
#if UNITY_IOS
            return getItem(key);
#else
        return null;
#endif
        }

        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>成功, 失敗</returns>
        public bool Delete(string key)
        {
#if UNITY_IOS
            // 返却されるステータスが 0 なら成功
            return deleteItem(key) == 0;
#else
        return false;
#endif
        }
    }
}