using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Hermes
{
    /// <summary>
    /// gzipとBase64を使った圧縮、解凍クラス
    /// </summary>
    public class GZipBase64
    {
        /// <summary>
        /// 圧縮
        /// </summary>
        /// <param name="rawtext">生テキスト</param>
        /// <returns>圧縮テキスト</returns>
        public static string Compress(string rawtext)
        {
            byte[] uncompressed = Encoding.UTF8.GetBytes(rawtext);
            using (var ms = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(ms, CompressionLevel.Fastest))
                {
                    gzipStream.Write(uncompressed, 0, uncompressed.Length);
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// 解凍
        /// </summary>
        /// <param name="base64gzip">圧縮テキスト</param>
        /// <returns>生テキスト</returns>
        public static string Decompress(string base64gzip)
        {
            byte[] compressed = Convert.FromBase64String(base64gzip);
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress))
                {
                    while (true)
                    {
                        var readSize = gzipStream.Read(buffer, 0, buffer.Length);
                        if (readSize == 0)
                        {
                            break;
                        }
                        ms.Write(buffer, 0, readSize);
                    }
                }
                byte[] base64raw = ms.ToArray();
                return Encoding.UTF8.GetString(base64raw, 0, base64raw.Length);
            }
        }
    }
}