using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hermes.API.Editor
{
    /// <summary>
    /// テンプレートAPI生成
    /// </summary>
    public class GenerateTemplateAPI : EditorWindow
    {
        /// <summary>アクティブインスタンスのパス</summary>
        string currentPath = "Assets/Project";
        /// <summary>アクティブインスタンスの名前（フォルダ名）</summary>
        string currentName = "Template";
        /// <summary>APIの名前</summary>
        string apiName = "hoge/hoge";
        /// <summary>新しいアクティブインスタンスのパス</summary>
        string newCurrentPath = string.Empty;

        /// <summary>指定したフォルダ</summary>
        DefaultAsset targetFolder = null;
        /// <summary>新しい指定したフォルダ</summary>
        DefaultAsset newTargetFolder = null;
        /// <summary>指定したフォルダのパス</summary>
        static string targetFolderPath = string.Empty;

        [MenuItem("Hermes/GenerateTemplate/GenerateTemplateAPI")]
        static void OpenWindow()
        {
            GetWindow<GenerateTemplateAPI>("GenerateTemplateAPI");
        }

        void OnGUI()
        {
            GUILayout.Label("テンプレートAPIの作成", EditorStyles.boldLabel);

            GUILayout.Space(10f);
            var canGenerate = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                currentPath = EditorUserSettings.GetConfigValue("GenerateTemplateAPI_currentPath");
                targetFolderPath = EditorUserSettings.GetConfigValue("GenerateTemplateAPI_targetFolderPath");

                if (AssetDatabase.LoadAssetAtPath<DefaultAsset>(currentPath))
                {
                    var tex = EditorGUIUtility.IconContent("lightMeter/greenLight");
                    EditorGUILayout.LabelField(new GUIContent(tex), GUILayout.MaxWidth(10));
                    canGenerate = true;
                }
                else
                {
                    var tex = EditorGUIUtility.IconContent("lightMeter/redLight");
                    EditorGUILayout.LabelField(new GUIContent(tex), GUILayout.MaxWidth(10));
                }
                GUILayout.Label("Current Path", GUILayout.MaxWidth(100f));
                newCurrentPath = GUILayout.TextField(currentPath);
                if (newCurrentPath != currentPath)
                {
                    currentPath = newCurrentPath;
                    // 値を設定
                    EditorUserSettings.SetConfigValue("GenerateTemplateAPI_currentPath", currentPath);
                    AssetDatabase.SaveAssets();
                }
                newTargetFolder = (DefaultAsset)EditorGUILayout.ObjectField(targetFolder, typeof(DefaultAsset), false, GUILayout.Width(100f));
                if (newTargetFolder == null && targetFolder == null)
                {
                    if (targetFolderPath != string.Empty)
                    {
                        newTargetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(targetFolderPath);
                        // ディレクトリでなければ、指定を解除する
                        bool isDirectory = File.GetAttributes(targetFolderPath).HasFlag(FileAttributes.Directory);
                        if (isDirectory)
                        {
                            targetFolder = newTargetFolder;
                        }
                        else
                        {
                            newTargetFolder = null;
                            targetFolderPath = string.Empty;
                            EditorUserSettings.SetConfigValue("GenerateTemplateAPI_targetFolderPath", targetFolderPath);
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
                if (newTargetFolder != targetFolder)
                {
                    targetFolder = newTargetFolder;
                    targetFolderPath = AssetDatabase.GetAssetOrScenePath(targetFolder);
                    currentPath = targetFolderPath;
                    // ディレクトリでなければ、指定を解除する
                    bool isDirectory = File.GetAttributes(targetFolderPath).HasFlag(FileAttributes.Directory);
                    if (!isDirectory)
                    {
                        targetFolder = null;
                        targetFolderPath = string.Empty;
                        currentPath = newCurrentPath;
                    }
                    // 値を設定
                    EditorUserSettings.SetConfigValue("GenerateTemplateAPI_targetFolderPath", targetFolderPath);
                    EditorUserSettings.SetConfigValue("GenerateTemplateAPI_currentPath", currentPath);
                    AssetDatabase.SaveAssets();
                }
            }

            GUILayout.Space(10f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(17f);
                GUILayout.Label("Current Name", GUILayout.MaxWidth(100f));
                currentName = GUILayout.TextField(currentName);
            }

            GUILayout.Space(10f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(17f);
                GUILayout.Label("API Name", GUILayout.MaxWidth(100f));
                apiName = GUILayout.TextField(apiName);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Generate"))
            {
                if (canGenerate)
                    // スクリプトを作成
                    GenerateScripts(currentName, apiName, Template);
                else
                    Debug.LogError("CurrentPath is not set correctly.");
            }
        }

        /// <summary>
        /// スクリプト作成
        /// </summary>
        /// <param name="scriptName">作成するスクリプトファイルの名前</param>
        /// <param name="apiName">APIの名前</param>
        /// <param name="scriptSource">作成するスクリプトのソース</param>
        void GenerateScripts(string scriptName, string apiName, string scriptSource)
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
        static readonly string Template =
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