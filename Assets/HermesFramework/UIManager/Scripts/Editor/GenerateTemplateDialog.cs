using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// テンプレートダイアログ生成
    /// </summary>
    public class GenerateTemplateDialog : EditorWindow
    {
        /// <summary>アクティブインスタンスのパス</summary>
        string currentPath = "Assets/Project";
        /// <summary>アクティブインスタンスの名前（フォルダ名）</summary>
        string currentName = "Template";
        /// <summary>namespace</summary>
        string nameSpace = "namespace";
        /// <summary>新しいアクティブインスタンスのパス</summary>
        string newCurrentPath = string.Empty;

        /// <summary>指定したフォルダ</summary>
        DefaultAsset targetFolder = null;
        /// <summary>新しい指定したフォルダ</summary>
        DefaultAsset newTargetFolder = null;
        /// <summary>指定したフォルダのパス</summary>
        static string targetFolderPath = string.Empty;

        [MenuItem("Hermes/GenerateTemplate/GenerateTemplateDialog")]
        static void OpenWindow()
        {
            GetWindow<GenerateTemplateDialog>("GenerateTemplateDialog");
        }

        void OnGUI()
        {
            GUILayout.Label("テンプレートダイアログの作成", EditorStyles.boldLabel);

            GUILayout.Space(10f);
            var canGenerate = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                currentPath = EditorUserSettings.GetConfigValue("GenerateTemplateDialog_currentPath");
                targetFolderPath = EditorUserSettings.GetConfigValue("GenerateTemplateDialog_targetFolderPath");

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
                    EditorUserSettings.SetConfigValue("GenerateTemplateDialog_currentPath", currentPath);
                    AssetDatabase.SaveAssets();
                }
                newTargetFolder = (DefaultAsset)EditorGUILayout.ObjectField(targetFolder, typeof(DefaultAsset), false, GUILayout.Width(100f));
                if (newTargetFolder == null && targetFolder == null)
                {
                    if (targetFolderPath != string.Empty)
                    {
                        newTargetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(targetFolderPath);
                        // ディレクトリでなければ、指定を解除する
                        if (newTargetFolder)
                        {
                            bool isDirectory = File.GetAttributes(targetFolderPath).HasFlag(FileAttributes.Directory);
                            if (isDirectory)
                            {
                                targetFolder = newTargetFolder;
                            }
                            else
                            {
                                newTargetFolder = null;
                                targetFolderPath = string.Empty;
                                EditorUserSettings.SetConfigValue("GenerateTemplateDialog_targetFolderPath", targetFolderPath);
                                AssetDatabase.SaveAssets();
                            }
                        }
                        else
                        {
                            targetFolderPath = string.Empty;
                            EditorUserSettings.SetConfigValue("GenerateTemplateDialog_targetFolderPath", targetFolderPath);
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
                    EditorUserSettings.SetConfigValue("GenerateTemplateDialog_targetFolderPath", targetFolderPath);
                    EditorUserSettings.SetConfigValue("GenerateTemplateDialog_currentPath", currentPath);
                    AssetDatabase.SaveAssets();
                }
                if (targetFolderPath != AssetDatabase.GetAssetOrScenePath(targetFolder))
                {
                    if (targetFolderPath == currentPath)
                    {
                        currentPath = AssetDatabase.GetAssetOrScenePath(targetFolder);
                        EditorUserSettings.SetConfigValue("GenerateTemplateDialog_currentPath", currentPath);
                    }
                    targetFolderPath = AssetDatabase.GetAssetOrScenePath(targetFolder);
                    // 値を設定
                    EditorUserSettings.SetConfigValue("GenerateTemplateDialog_targetFolderPath", targetFolderPath);
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
                GUILayout.Label("namespace", GUILayout.MaxWidth(100f));
                nameSpace = GUILayout.TextField(nameSpace);
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("Generate"))
            {
                if (canGenerate)
                    CreateTemplatePackage();
                else
                    Debug.LogError("CurrentPath is not set correctly.");
            }
        }
        
        /// <summary>
        /// 作成
        /// </summary>
        void CreateTemplatePackage()
        {
            // フォルダを作成
            AssetDatabase.CreateFolder(currentPath, "Dialog");
            IEnumerable<string> folders = new List<string>{"Scripts", "Prefabs" };   // 作成したいフォルダ名群
            foreach (var name in folders)
            {
                AssetDatabase.CreateFolder(currentPath + "/Dialog", name);
            }

            // スクリプトを作成
            GenerateScripts(currentName, nameSpace, Template);

            // 機能実行を記憶（クリップボードを利用）
            GUIUtility.systemCopyBuffer = "GenerateTemplateDialog@" + currentPath + "@" + currentName + "@" + nameSpace;
        }

        /// <summary>
        /// スクリプト作成
        /// </summary>
        /// <param name="scriptName">作成するスクリプトファイルの名前</param>
        /// <param name="nameSpace">namespace</param>
        /// <param name="scriptSource">作成するスクリプトのソース</param>
        void GenerateScripts(string scriptName, string nameSpace, string scriptSource)
        {
            var filePath = currentPath + "/Dialog/Scripts/" + scriptName + "Dialog.cs";
            var scriptPath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            var script = scriptSource.Replace(@"#TEMPLATENAME#", scriptName);
            script = script.Replace(@"#NAMESPACE#", nameSpace);

            // スクリプトファイルに書き込み
            File.WriteAllText(scriptPath, script, System.Text.Encoding.UTF8);

            // エディタ更新
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// シーン改装
        /// </summary>
        [DidReloadScripts]
        [Obsolete]
        static void SceneRenovation()
        {
            // 本スクリプトでのシーン作成を行った直後かどうかをチェック（クリップボード利用）
            if (GUIUtility.systemCopyBuffer.Split('@')[0] != "GenerateTemplateDialog") return;

            // クリップボードから情報を取得
            var currentPath = GUIUtility.systemCopyBuffer.Split('@')[1];
            var currentName = GUIUtility.systemCopyBuffer.Split('@')[2];
            var nameSpace = GUIUtility.systemCopyBuffer.Split('@')[3];

            // クリップボードクリア
            GUIUtility.systemCopyBuffer = "";


            // ダイアログを作成
            var baseName = currentName + "Dialog";

            // GameObjectを作成
            var obj = new GameObject(baseName);
            // Scriptをアタッチ
            var dialog = obj.AddComponentExt(nameSpace, baseName);
            // componentをいじる
            PrefabUtility.SaveAsPrefabAsset(obj, currentPath + "/Dialog/Prefabs/" + baseName + ".prefab");
            // 削除
            DestroyImmediate(obj);

            //開いているシーンを全て保存(前もってUndoに記録していない部分は保存されない)
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"{currentName} Dialog Created!");
        }










        // 作成するスクリプトファイル：Template
        static readonly string Template =
@"using Cysharp.Threading.Tasks;

namespace #NAMESPACE#
{
    /// <summary>
    /// #TEMPLATENAME# Dialog
    /// </summary>
    public class #TEMPLATENAME#Dialog : Hermes.UI.Dialog
    {
        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name=""options"">オプション</param>
        public override UniTask OnLoad(object options)
        {
            var op = options as Options;
            return base.OnLoad(options);
        }
    }
}";
    }
}