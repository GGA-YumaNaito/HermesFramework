using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Hermes.Save.Editor
{
    /// <summary>
    /// SaveDataDelete
    /// </summary>
    public class SaveDataDelete : EditorWindow
    {
        /// <summary>アクティブインスタンスのパス</summary>
        string currentPath = "Assets/Project";
        /// <summary>アクティブインスタンスの名前（フォルダ名）</summary>
        string currentName = "Template";
        /// <summary>新しいアクティブインスタンスのパス</summary>
        string newCurrentPath = string.Empty;

        /// <summary>ディレクトリパス</summary>
        string dirPass { get { return $"{FileUtils.Pass}/SaveData"; } }

        /// <summary>リストパス</summary>
        string listPass { get { return $"{dirPass}/list.txt"; } }

        /// <summary>
        /// ファイルパス取得
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <returns>パス</returns>
        string GetFilePass(string name) => $"{dirPass}/{name}.dat";

        /// <summary>
        /// ファイルパス取得
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <param name="designationPass">指定パス</param>
        /// <returns>パス</returns>
        string GetFilePass(string name, string designationPass)
        {
            if (designationPass.IsNullOrEmpty())
                return GetFilePass(name);
            else
                return $"{designationPass}/{name}.dat";
        }

        /// <summary>
        /// ファイルが存在しているか
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <param name="designationPass">指定パス</param>
        /// <returns>true = 存在している : false = 存在していない</returns>
        bool HasFile(string name, string designationPass = null)
        {
            return File.Exists(GetFilePass(name, designationPass));
        }

        /// <summary>
        /// 削除
        /// </summary>
        /// <param name="name">ファイル名</param>
        /// <param name="designationPass">指定パス</param>
        void Delete(string name, string designationPass = null)
        {
            if (HasFile(name, designationPass))
                File.Delete(GetFilePass(name, designationPass));
        }

        [MenuItem("Hermes/SaveDataDelete")]
        static void OpenWindow()
        {
            GetWindow<SaveDataDelete>("SaveDataDelete");
        }

        void OnGUI()
        {
            GUILayout.Label("SaveData", EditorStyles.boldLabel);

            GUILayout.Space(10f);

            var hasDirectory = false;
            using (new EditorGUILayout.HorizontalScope())
            {
                currentPath = EditorUserSettings.GetConfigValue("SaveDataDelete_currentPath");

                if (Directory.Exists(currentPath))
                {
                    var tex = EditorGUIUtility.IconContent("lightMeter/greenLight");
                    EditorGUILayout.LabelField(new GUIContent(tex), GUILayout.MaxWidth(10));
                    hasDirectory = true;
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
                    EditorUserSettings.SetConfigValue("SaveDataDelete_currentPath", currentPath);
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
            if (GUILayout.Button("Delete"))
            {
                if (hasDirectory)
                {
                    Delete(currentName, currentPath);

                    var filePass = GetFilePass(currentName, currentPath);
                    var lines = File
                        .ReadLines(listPass)
                        .Where((line, num) => line != filePass)
                        .ToList();
                    File.WriteAllLines(listPass, lines);

                    // CurrentPathのフォルダにあるファイル一覧のログ出力
                    var directoryInfo = new DirectoryInfo(currentPath);
                    var fileInfos = directoryInfo.GetFiles();
                    foreach (var info in fileInfos)
                    {
                        Debug.Log(info.FullName);
                    }
                }
                else
                    Debug.LogError("CurrentPath is not set correctly.");
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("All Delete"))
            {
                var lines = File.ReadLines(listPass);

                foreach (var line in lines)
                {
                    Debug.Log(line);
                    if (File.Exists(line))
                        File.Delete(line);
                    else
                        Debug.LogError($"ファイルが存在していません : {line}");
                }

                File.WriteAllText(listPass, string.Empty);
            }
        }
    }
}