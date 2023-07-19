using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// ファイル操作に関するクラス
    /// </summary>
    public class FileUtils
    {
        static string pass;
        /// <summary>
        /// 書き込み可能なディレクトリのパス
        /// <para>ファイルの保存はこのディレクトリの直下ではなく、サブディレクトリを作成して保存する事を推奨します</para>
        /// </summary>
        public static string Pass {
            get {
                if (pass.IsNullOrEmpty())
                    pass = GetWritableDirectoryPath();
                return pass;
            }
        }

        /// <summary>
        /// 書き込み可能なディレクトリのパスを返す
        /// <para>ファイルの保存はこのディレクトリの直下ではなく、サブディレクトリを作成して保存する事を推奨します</para>
        /// </summary>
        /// <returns>プラットフォームごとの書き込み可能なディレクトリのパス</returns>
        static string GetWritableDirectoryPath()
        {
            // Androidの場合、Application.persistentDataPathでは外部から読み出せる場所に保存されてしまうため
            // アプリをアンインストールしてもファイルが残ってしまう
            // ここではアプリ専用領域に保存するようにする
#if !UNITY_EDITOR && UNITY_ANDROID
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using var getFilesDir = currentActivity.Call<AndroidJavaObject>("getFilesDir");
            return getFilesDir.Call<string>("getCanonicalPath");
#else
            return Application.persistentDataPath;
#endif
        }
    }
}