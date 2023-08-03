using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Hermes.Save
{
    /// <summary>
    /// Save Manager
    /// </summary>
    public class SaveManager : SingletonMonoBehaviour<SaveManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>ファイルストリーム</summary>
        FileStream fileStream;
        /// <summary>バイナリフォーマッター</summary>
        BinaryFormatter bf;

        /// <summary>暗号化の共有キー(非公開)</summary>
        static readonly string key = "RARdRxzemkkewJQm";
        /// <summary>初期化ベクトル(公開)</summary>
        static readonly string iv = "SInKORyzckHTuMfI";

        /// <summary>ディレクトリパス</summary>
        string dirPass { get { return $"{FileUtils.Pass}/SaveData"; } }

        /// <summary>
        /// ファイルパス取得
        /// </summary>
        string GetFilePass(string name) => $"{dirPass}/{name}.dat";

        protected override void Awake()
        {
            base.Awake();

            if (!Directory.Exists(dirPass))
                Directory.CreateDirectory(dirPass);
        }

        /// <summary>
        /// ファイルが存在しているか
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <returns>true = 存在している : false = 存在していない</returns>
        public bool HasFile(string name)
        {
            return File.Exists(GetFilePass(name));
        }

        /// <summary>
        /// 保存
        /// <para>データがclassなら、Json形式にして圧縮と暗号化を行う</para>
        /// <para>データが非classなら、圧縮と暗号化を行う</para>
        /// <para>データはclass以外で使えるのは、int,long,float,double,string,bool</para>
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="name">ファイル名</param>
        /// <param name="data">データ</param>
        public void Save<T>(string name, T data)
        {
            bf = new BinaryFormatter();
            fileStream = null;

            try
            {
                // SaveDataフォルダにdatファイルを作成
                fileStream = File.Open(GetFilePass(name), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                // ファイルに保存
                bf.Serialize(fileStream, Encode<T>(data));
            }
            catch (IOException e1)
            {
                Debug.LogError($"ファイルオープンエラー : {e1.Message}");
            }
            finally
            {
                fileStream?.Close();
            }
        }

        /// <summary>
        /// 読み込み
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="name">ファイル名</param>
        /// <returns>データ</returns>
        public T Load<T>(string name)
        {
            bf = new BinaryFormatter();
            fileStream = null;
            T data = default;

            try
            {
                // ファイルを読み込む
                fileStream = File.Open(GetFilePass(name), FileMode.Open);
                // 読み込んだデータをデシリアライズ
                data = Decode<T>((string)bf.Deserialize(fileStream));
            }
            catch (FileNotFoundException e1)
            {
                Debug.LogError($"ファイルがありません : {e1.Message}");
            }
            catch (IOException e2)
            {
                Debug.LogError($"ファイルオープンエラー : {e2.Message}");
            }
            finally
            {
                fileStream?.Close();
            }
            return data;
        }

        /// <summary>
        /// 暗号化
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="data">データ</param>
        /// <returns>暗号データ</returns>
        string Encode<T>(object data)
        {
            var _data = string.Empty;
            // 1. Class Instance -> Json
            if (IsClass(typeof(T)))
                _data = JsonUtility.ToJson(data);
            else
                _data = Convert.ToString(data);
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            Debug.Log($"SaveData = {_data}");
#endif
            // 2. Json -> Gzip
            var gzip = GZipBase64.Compress(_data);

            // 3. GZip -> AES
            return AES128Base64.Encode(gzip, key, iv);
        }

        /// <summary>
        /// 復号
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="data">データ</param>
        /// <param name="iv">初期化ベクトル(公開)</param>
        /// <returns>復号データ</returns>
        T Decode<T>(string data)
        {
            // 1. AES -> GZip
            var aes = AES128Base64.Decode(data, key, iv);
            // 2. GZip -> Json
            var gzip = GZipBase64.Decompress(aes);
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            Debug.Log($"LoadData = {gzip}");
#endif
            // 3. Json -> Class Instance
            if (IsClass(typeof(T)))
                return JsonUtility.FromJson<T>(gzip);
            else
                return (T)ConvertType(gzip, typeof(T));
        }

        /// <summary>
        /// クラスか判定
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns>クラスならtrue</returns>
        bool IsClass(Type type)
        {
            return type.IsClass && type != typeof(int) && type != typeof(long) && type != typeof(float) && type != typeof(double) && type != typeof(string) && type != typeof(bool) && type != typeof(object);
        }

        /// <summary>
        /// オブジェクトの型変換
        /// <para>オブジェクトを指定された型に解釈する</para>
        /// </summary>
        object ConvertType(object data, Type type)
        {
            if (type == typeof(int) || type.IsEnum)
            {
                // int/Enum型に変換.Enumはintと等価であるので、intにコンバート可能
                if (data == null)
                {
                    return 0;
                }
                else if (int.TryParse(Convert.ToString(data), out int tmp))
                {
                    return tmp;
                }
                else
                {
                    var tmpStr = Convert.ToString(data);
                    if (tmpStr == "False")
                    {
                        return 0;
                    }
                    else if (tmpStr == "True")
                    {
                        return 1;
                    }
                    else
                    {
                        Debug.LogError($"illegal type = {type}, data = {data}, tmpStr = {tmpStr}");
                    }
                }
                return 0;
            }
            else if (type == typeof(long))
            {
                // long型に変換
                if (data == null)
                {
                    return 0;
                }
                else if (long.TryParse(Convert.ToString(data), out long tmp))
                {
                    return tmp;
                }
                else
                {
                    Debug.LogError($"illegal type = {type}, data = {data}");
                }
                return 0;
            }
            else if (type == typeof(string))
            {
                // string型に変換
                if (data == null)
                    return string.Empty;
                return Convert.ToString(data);
            }
            else if (type == typeof(float))
            {
                // float型に変換
                if (data == null)
                {
                    return 0.0f;
                }
                else if (float.TryParse(Convert.ToString(data), out float tmp))
                {
                    return tmp;
                }
                else
                {
                    Debug.LogError($"illegal type = {type}, data = {data}");
                }
                return 0.0f;
            }
            else if (type == typeof(double))
            {
                // double型に変換
                if (data == null)
                {
                    return 0.0d;
                }
                else if (double.TryParse(Convert.ToString(data), out double tmp))
                {
                    return tmp;
                }
                else
                {
                    Debug.LogError($"illegal type = {type}, data = {data}");
                }
                return 0.0d;
            }
            else if (type == typeof(bool))
            {
                // bool型に変換
                if (data == null)
                {
                    return false;
                }
                else if (int.TryParse(Convert.ToString(data), out _))
                {
                    return Convert.ToInt32(data) > 0;
                }
                else
                {
                    var tmpStr = Convert.ToString(data);
                    if (tmpStr == "False")
                    {
                        return false;
                    }
                    else if (tmpStr == "True")
                    {
                        return true;
                    }
                    else
                    {
                        Debug.LogError($"illegal type = {type}, data = {data}, tmpStr = {tmpStr}");
                    }
                }
                return false;
            }
            else
            {
                // その他の型(object型)
                return data;
            }
        }
    }
}