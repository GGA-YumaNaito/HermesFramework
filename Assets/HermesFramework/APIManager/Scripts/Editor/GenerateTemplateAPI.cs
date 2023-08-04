using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hermes.API.Editor
{
    /// <summary>
    /// テンプレートシーン生成
    /// </summary>
    public class GenerateTemplateAPI : EditorWindow
    {
        /// <summary>アクティブインスタンスのパス</summary>
        static string currentPath { get; set; } = "Assets/Project";
        /// <summary>アクティブインスタンスの名前（フォルダ名）</summary>
        static string currentName { get; set; } = "Template";
        /// <summary>APIの名前</summary>
        static string apiName { get; set; } = "hoge/hoge";

        /// <summary>新しいアクティブインスタンスのパス</summary>
        string newCurrentPath = string.Empty;

        [MenuItem("Hermes/GenerateTemplate/GenerateTemplateAPI")]
        private static void OpenWindow()
        {
            EditorWindow.GetWindow<GenerateTemplateAPI>("GenerateTemplateAPI");
        }

        private void OnGUI()
        {
            GUILayout.Label("テンプレートシーンの作成", EditorStyles.boldLabel);

            GUILayout.Space(10f);
            newCurrentPath = EditorGUILayout.TextField("Current Path", currentPath);
            if (newCurrentPath != currentPath)
            {
                currentPath = newCurrentPath;
                // 値を設定
                EditorUserSettings.SetConfigValue("GenerateTemplateAPI_currentPath", currentPath);
                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(10f);
            currentName = EditorGUILayout.TextField("Current Name", currentName);

            GUILayout.Space(10f);
            apiName = EditorGUILayout.TextField("API Name", apiName);

            GUILayout.Space(10f);
            if (GUILayout.Button("Generate"))
            {
                // スクリプトを作成
                GenerateScripts(currentName, apiName, Template);
            }
        }

        /// <summary>
        /// スクリプト作成
        /// </summary>
        /// <param name="scriptName">作成するスクリプトファイルの名前</param>
        /// <param name="apiName">APIの名前</param>
        /// <param name="scriptSource">作成するスクリプトのソース</param>
        private static void GenerateScripts(string scriptName, string apiName, string scriptSource)
        {
            var filePath = currentPath + "/" + scriptName + ".cs";
            var scriptPath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            var script = scriptSource.Replace(@"#TEMPLATENAME#", scriptName);
            script = script.Replace(@"#APINAME#", apiName);

            // スクリプトファイルに書き込み
            File.WriteAllText(scriptPath, script, System.Text.Encoding.UTF8);
        
            // エディタ更新
            AssetDatabase.Refresh();
        }

        // 作成するスクリプトファイル：Template
        private static readonly string Template =
@"using System;
using Cysharp.Threading.Tasks;
using Hermes.API;

namespace API
{
    /// <summary>
    /// #TEMPLATENAME#
    /// </summary>
    [Serializable]
    public class #TEMPLATENAME# : APIData<#TEMPLATENAME#, #TEMPLATENAME#.Request, #TEMPLATENAME#.Response>
    {
        protected override string api => ""#APINAME#"";

        protected override bool isPost => true;

        public override Request request { get; set; }
        public override Response response { get; set; }

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase
        {
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <param name=""onSuccess"">成功コールバック</param>
        /// <param name=""onFailed"">失敗コールバック</param>
        /// <param name=""isQueue"">キューにためるか.</param>
        /// <param name=""isRetry"">リトライするか.</param>
        /// <returns>UniTask</returns>
        public override async UniTask SendWebRequest(Action<#TEMPLATENAME#> onSuccess = null, Action<#TEMPLATENAME#> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<#TEMPLATENAME#, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}";
    }
}