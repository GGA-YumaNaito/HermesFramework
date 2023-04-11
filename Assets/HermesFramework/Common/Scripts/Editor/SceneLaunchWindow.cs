using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hermes.Common.Editor
{
    /// <summary>
    /// Scene Launcher
    /// </summary>
    public class SceneLaunchWindow : EditorWindow
    {
        /// <summary>buildScenePaths</summary>
        string[] buildScenePaths = null;
        /// <summary>othersScenePath</summary>
        string[] othersScenePath = null;
        /// <summary>scrollPosition</summary>
        Vector2 scrollPosition = Vector2.zero;

        /// <summary>
        /// ShowWindow
        /// </summary>
        [MenuItem("Hermes/Scene Launcher")]
        static void ShowWindow()
        {
            // ウィンドウを表示！
            GetWindow<SceneLaunchWindow>("Scene Launcher");
        }

        /// <summary>
        /// OnFocus
        /// </summary>
        void OnFocus()
        {
            Reload();
        }

        /// <summary>
        /// Reload
        /// </summary>
        void Reload()
        {
            buildScenePaths = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();

            // コードを分かりやすくするため、一度 ToArray() を使ってローカル変数化してるけど、
            // 別にまとめちゃってもいいよ(……というか処理的にはそっちのほうがいいはず)
            string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets" });
            string[] paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            othersScenePath = paths.Where(path => !buildScenePaths.Any(buildPath => buildPath == path)).ToArray();
        }

        /// <summary>
        /// OnGUI
        /// </summary>
        void OnGUI()
        {
            // OnFocus() より前に呼ばれる対策(あるのかな？)
            if (buildScenePaths == null && othersScenePath == null)
                Reload();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    EditorGUILayout.LabelField("Scenes in build");
                    GenerateButtons(buildScenePaths);

                    GUILayout.Space(10.0f);

                    EditorGUILayout.LabelField("Others");
                    GenerateButtons(othersScenePath);
                }
                EditorGUILayout.EndScrollView();

            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// ボタン生成
        /// </summary>
        /// <param name="scenePaths">Scene Paths</param>
        void GenerateButtons(string[] scenePaths)
        {
            if (scenePaths != null && scenePaths.Length > 0)
            {
                foreach (var path in scenePaths)
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    if (GUILayout.Button(name))
                    {
                        // 保存するかどうか
                        if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(GetDirtyScenes()))
                        {
                            EditorSceneManager.OpenScene(path);
                        }
                    }
                    GUILayout.Space(5.0f);
                }
            }
            else
                EditorGUILayout.LabelField("シーンがありません");
        }

        /// <summary>
        /// 編集中のシーン一覧取得
        /// </summary>
        /// <returns>編集中のシーン一覧</returns>
        Scene[] GetDirtyScenes()
        {
            var scenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                    scenes.Add(scene);
            }
            return scenes.ToArray();
        }
    }
}