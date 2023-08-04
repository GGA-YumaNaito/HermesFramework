using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// テンプレートシーン生成
    /// </summary>
    public class GenerateTemplateScene : EditorWindow
    {
        /// <summary>アクティブインスタンスのパス</summary>
        static string currentPath { get; set; } = "Assets/Project";
        /// <summary>アクティブインスタンスの名前（フォルダ名）</summary>
        static string currentName { get; set; } = "Template";

        /// <summary>新しいアクティブインスタンスのパス</summary>
        string newCurrentPath = string.Empty;

        [MenuItem("Hermes/GenerateTemplate/GenerateTemplateScene")]
        private static void OpenWindow()
        {
            EditorWindow.GetWindow<GenerateTemplateScene>("GenerateTemplateScene");
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
            if (GUILayout.Button("Generate"))
            {
                CreateTemplatePackage();
            }
        }
        
        /// <summary>
        /// 作成
        /// </summary>
        private static void CreateTemplatePackage()
        {
            // フォルダを作成
            AssetDatabase.CreateFolder(currentPath, currentName);
            IEnumerable<string> folders = new List<string>{"Scripts", "Scenes" };   // 作成したいフォルダ名群
            foreach (var name in folders)
            {
                AssetDatabase.CreateFolder(currentPath + "/" + currentName, name);
            }
            // シーンを作成
            CreateScene(currentName + "Scene");

            // スクリプトを作成
            GenerateScripts(currentName, Template);
        
            // 機能実行を記憶（クリップボードを利用）
            GUIUtility.systemCopyBuffer = "GenerateTemplateScene@" + currentName;
        }
        
        /// <summary>
        /// 新しくシーンを作成する
        /// </summary>
        /// <param name="baseName">作成するシーンの名前</param>
        private static void CreateScene(string baseName = "Template")
        {
            var sceneName = baseName + ".unity";
            var scenePath = currentPath + "/" + currentName + "/Scenes/" + sceneName;
            var path = AssetDatabase.GenerateUniqueAssetPath(scenePath);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            EditorSceneManager.SaveScene(scene, path);
        }
        
        /// <summary>
        /// スクリプト作成
        /// </summary>
        /// <param name="scriptName">作成するスクリプトファイルの名前</param>
        /// <param name="scriptSource">作成するスクリプトのソース</param>
        private static void GenerateScripts(string scriptName, string scriptSource)
        {
            var filePath = currentPath + "/" + scriptName + "/Scripts/" + scriptName + "Scene.cs";
            var scriptPath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            var script = scriptSource.Replace(@"#TEMPLATENAME#", scriptName);

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
        private static void SceneRenovation()
        {
            // 本スクリプトでのシーン作成を行った直後かどうかをチェック（クリップボード利用）
            if (GUIUtility.systemCopyBuffer.Split('@')[0] != "GenerateTemplateScene") return;

            // クリップボードから情報を取得
            currentName = GUIUtility.systemCopyBuffer.Split('@')[1];

            // クリップボードクリア
            GUIUtility.systemCopyBuffer = "";

            // 規定値
            var cullingMaskUI = 32;
            var ui = 5;
            var referenceResolution = new Vector2(1170, 2532);

            // カメラオブジェクト取得
            var cameraObject = GameObject.FindWithTag("MainCamera");
            // AudioListener削除
            //EditorApplication.delayCall += () => DestroyImmediate(cameraObject.GetComponent<AudioListener>());
            // AudioListenerを削除し、Undoに記録
            Undo.DestroyObjectImmediate(cameraObject.GetComponent<AudioListener>());
            // Camera取得
            var camera = cameraObject.GetComponent<Camera>();
            // Cameraの状態をUndoに記録
            Undo.RecordObject(camera, "Change Camera Settings");
            // Camera設定
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.cullingMask = cullingMaskUI;
            camera.depth = 0;

            // GameObjectを作成
            var obj = new GameObject(currentName + "Scene");
            // Layer変更
            obj.layer = ui;
            // RectTransform
            obj.AddComponent<RectTransform>();
            // Scriptをアタッチ
            var screen = obj.AddComponentExt(currentName);
            // カメラを設定
            ((Screen)screen).SetCamera(camera);

            // Canvas
            var canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.sortingLayerName = "UI";
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            // CanvasScaler
            var scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            // GraphicRaycaster
            obj.AddComponent<GraphicRaycaster>();

            // GameObjectをUndoに記録
            Undo.RegisterCreatedObjectUndo(obj, "Create New GameObject");

            //開いているシーンを全て保存(前もってUndoに記録していない部分は保存されない)
            EditorSceneManager.SaveOpenScenes();

            Debug.Log($"{currentName} Scene Created!");
        }

        // 作成するスクリプトファイル：Template
        private static readonly string Template =
@"using Cysharp.Threading.Tasks;

namespace #TEMPLATENAME#
{
    /// <summary>
    /// #TEMPLATENAME# Scene
    /// </summary>
    public class #TEMPLATENAME#Scene : Hermes.UI.Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name=""options""></param>
        public override UniTask OnLoad(object options)
        {
            return UniTask.CompletedTask;
        }
    }
}";
    }

    /// <summary>
    /// AddComponentExtension
    /// </summary>
    public static class AddComponentExtension
    {
        /// <summary>
        /// AddComponentExt
        /// </summary>
        /// <param name="obj">this GameObject</param>
        /// <param name="scriptName">Script name</param>
        /// <returns>Component</returns>
        public static Component AddComponentExt(this GameObject obj, string scriptName)
        {
            Assembly asm = Assembly.Load("Assembly-CSharp");
            var type = asm?.GetType(scriptName + "." + scriptName + "Scene") ?? null;
            if (type == null)
            {
                Debug.LogError($"Failed to ComponentType:{scriptName + "Scene"}");
                return null;
            }
            return obj.AddComponent(type);
        }
    }
}