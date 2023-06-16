using System;
using System.Security.Cryptography;
using System.Text;

namespace Hermes
{
    /// <summary>
    /// AESとBase64を使った暗号、復号クラス
    /// </summary>
    public class AES128Base64
    {
        /// <summary>
        /// AES128ビット(CBCモード)による暗号化
        /// <para>※このメソッドでは最大16byteの文字列(src)に対応</para>
        /// </summary>
        /// <param name="src">文字列</param>
        /// <param name="key">共有キー(非公開)</param>
        /// <param name="iv">初期化ベクトル(公開)</param>
        /// <returns>暗号化されたBase64String</returns>
        public static string Encode(string src, string key, string iv)
        {
            // 文字列をバイト型配列へ
            var input = Encoding.UTF8.GetBytes(src);

            // AES作成
            var AES = CreateAES(key, iv);

            // 暗号化
            var output = AES.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);

            // Base64
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// AES128ビット(CBCモード)による復号化
        /// <para>※このメソッドでは最大16byteの文字列(src)に対応</para>
        /// </summary>
        /// <param name="src">文字列</param>
        /// <param name="key">共有キー(非公開)</param>
        /// <param name="iv">初期化ベクトル(公開)</param>
        /// <returns>復号化されたBase64String</returns>
        public static string Decode(string src, string key, string iv)
        {
            // 文字列をバイト型配列へ
            var input = Encoding.UTF8.GetBytes(src);

            // AES作成
            var AES = CreateAES(key, iv);

            // 復号化
            var output = AES.CreateDecryptor().TransformFinalBlock(input, 0, input.Length);

            // Base64
            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// AES作成
        /// </summary>
        /// <param name="key">共有キー(非公開)</param>
        /// <param name="iv">初期化ベクトル(公開)</param>
        /// <returns>AES</returns>
        static AesManaged CreateAES(string key, string iv)
        {
            return new AesManaged
            {
                // 鍵の長さ
                KeySize = 128,
                // ブロックサイズ(srcは16byteまで)
                BlockSize = 128,
                // CBCモード
                Mode = CipherMode.CBC,
                // 初期化ベクトル(公開)
                IV = Encoding.UTF8.GetBytes(iv),
                // 共有キー(非公開)
                Key = Encoding.UTF8.GetBytes(key),
                // パディング
                Padding = PaddingMode.PKCS7
            };
        }
    }
}