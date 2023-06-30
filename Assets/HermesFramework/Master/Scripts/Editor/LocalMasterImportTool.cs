using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Hermes.Master
{
    /// <summary>
    /// CSVからローカルマスター(ScriptableObject)を作成するツール
    /// <para>メニューのHermes/LocalMasterImportToolを選択</para>>
    /// </summary>
    public class LocalMasterImportTool : EditorWindow
    {
        private string csvPath = "Master/Csv";
        private string assetPath = "Assets/Master";

        private string selectMaster;
        private int selectMasterNum;
        private static List<string> masterList = new List<string>();

        [MenuItem("Hermes/LocalMasterImportTool")]
        private static void OpenWindow()
        {
            GetWindow<LocalMasterImportTool>("LocalMasterImportTool");
        }

        private void OnGUI()
        {
            GUILayout.Label("作成/更新するMasterを選択", EditorStyles.boldLabel);

            if (!masterList.Any())
            {
                string[] fs = Directory.GetFiles(csvPath, "*.csv");
                fs.ForEach(x => masterList.Add(Path.GetFileNameWithoutExtension(x)));
                masterList.Add("All");
            }

            selectMasterNum = EditorGUILayout.Popup("SelectMaster", selectMasterNum, masterList.ToArray());
            selectMaster = masterList[selectMasterNum];

            GUILayout.Space(10f);
            csvPath = EditorGUILayout.TextField("CsvPath", csvPath);

            GUILayout.Space(10f);
            assetPath = EditorGUILayout.TextField("AssetPath", assetPath);

            GUILayout.Space(10f);
            if (GUILayout.Button("リフレッシュ"))
            {
                masterList.Clear();
            }

            GUILayout.Space(10f);
            if (GUILayout.Button("作成/更新"))
            {

                //全て実行.
                if (selectMaster == "All")
                {
                    foreach (string s in masterList)
                    {
                        if (s == "All")
                            break;
                        UpdateMaster(s);
                    }
                }
                else
                {
                    UpdateMaster(selectMaster);
                }
            }
        }

        /// <summary>
        /// CSVファイルパスを取得します.
        /// </summary>
        string GetCsvPath(string masterCsvPath)
        {
            return string.Format("{0}/{1}", csvPath, masterCsvPath);
        }

        /// <summary>
        /// 現在のマスターアセットパスを取得します.
        /// </summary>
        /// <returns>The asset path.</returns>
        /// <param name="masterAssetPath">Master asset path.</param>
        string GetAssetPath(string masterAssetPath)
        {
            return string.Format("{0}/{1}", assetPath, masterAssetPath);
        }

        /// <summary>
        /// CSVファイルを行ごとに分ける.
        /// </summary>
        /// <returns>The lines.</returns>
        /// <param name="csvPath">Csv path.</param>
        private string[] SplitLines(string csvPath)
        {
            StreamReader sr;
            try
            {
                sr = new StreamReader(csvPath);
            }
            catch
            {
                Debug.LogError("Not Found CSV File");
                throw;
            }
            string strStream = sr.ReadToEnd();
            //StringSplitOption設定.
            StringSplitOptions option = StringSplitOptions.None;
            //行に分ける.
            return strStream.Split(new char[] { '\r', '\n' }, option);
        }

        /// <summary>
        /// クラス名からタイプを取得.
        /// </summary>
        private Type GetTypeByClassName(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == className)
                    {
                        return type;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// マスタ更新.
        /// </summary>
        private void UpdateMaster(string masterName)
        {
            Type t = GetTypeByClassName(masterName);
            object master = CreateInstance(t);

            string path = GetCsvPath($"{masterName}.csv");
            string assetPath = GetAssetPath($"{masterName}.asset");

            string[] lines = SplitLines(path);

            List<string> keys = new();

            string tmp = "";
            char[] delimiterChars = new char[1] { ',' };
            foreach (char c in lines[0])
            {
                tmp += c;
                if (c == delimiterChars[0])
                {
                    tmp = tmp.Trim(delimiterChars);
                    if (tmp != "")
                    {
                        keys.Add(tmp);
                        tmp = "";
                    }
                }
            }
            keys.Add(tmp);

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i] == "")
                    continue;

                var keyValueList = ParseKeyValueList(keys, lines[i]);

                // 使わないデータは含めないようにする
                var isReleasePair = keyValueList.Find(x => x.Key == "is_release");
                var isRelease = false;
                if (int.TryParse(Convert.ToString(isReleasePair.Value), out _))
                {
                    isRelease = Convert.ToInt32(isReleasePair.Value) > 0;
                }
                else
                {
                    string tmpStr = Convert.ToString(isReleasePair.Value);
                    if (tmpStr == "False")
                    {
                        isRelease = false;
                    }
                    else if (tmpStr == "True")
                    {
                        isRelease = true;
                    }
                }
                if (!isRelease)
                    continue;
                keyValueList.Remove(isReleasePair);

                // データ入れ込み
                Type t2 = GetTypeByClassName(masterName + "Data");
                object data = Activator.CreateInstance(t2);
                object value = ParseValue(data, keyValueList);
                MethodInfo addMethod = t.GetMethod("Add", new Type[] { typeof(object) });
                addMethod.Invoke(master, new object[] { value });
            }

            AssetDatabase.CreateAsset((UnityEngine.Object)master, assetPath);
            EditorGUIUtility.PingObject((UnityEngine.Object)master);
            Debug.Log($"Success!!! : {masterName}");
        }

        /// <summary>
        /// KeyValuePairのListに変換する
        /// </summary>
        /// <param name="keys">Keys</param>
        /// <param name="lineStrings">Line strings</param>
        /// <returns>KeyValuePairのList</returns>
        private List<KeyValuePair<string, string>> ParseKeyValueList(List<string> keys, string lineStrings)
        {
            bool isDoubleQuate = false;
            string tmp = "";
            char[] delimiterChars = new char[2] { ',', '"' };
            int count = 0;
            char before = ',';
            List<KeyValuePair<string, string>> keyValueList = new();
            foreach (char c in lineStrings)
            {
                tmp += c;
                if (c == delimiterChars[0] && !isDoubleQuate)
                {
                    tmp = tmp.Trim(delimiterChars[0]);
                    if (tmp != "")
                    {
                        keyValueList.Add(new KeyValuePair<string, string>(keys[count], tmp));
                        count++;
                        tmp = "";
                    }
                    else if (before == delimiterChars[0])
                    {
                        count++;
                    }
                }
                else if (c == delimiterChars[1])
                {
                    if (isDoubleQuate)
                    {
                        tmp = tmp.Trim(delimiterChars[1]);
                        keyValueList.Add(new KeyValuePair<string, string>(keys[count], tmp));
                        count++;
                        tmp = "";
                        isDoubleQuate = false;
                    }
                    else
                    {
                        isDoubleQuate = true;
                    }
                }
                before = c;
            }
            if (isDoubleQuate)
            {
                tmp = tmp.Trim(delimiterChars[1]);
                keyValueList.Add(new KeyValuePair<string, string>(keys[count], tmp));
            }
            else
            {
                keyValueList.Add(new KeyValuePair<string, string>(keys[count], tmp));
            }
            return keyValueList;
        }

        private object ParseValue(object data, List<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                SetValue(data, pair.Key, pair.Value);
            }
            return data;
        }

        /// <summary>
        /// データセット
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        private void SetValue(object data, string key, string value)
        {
            Type type;
            if (key == "id")
                type = data.GetType().BaseType;
            else
                type = data.GetType();
            MemberInfo[] membersInfo = type.GetMember(key, BindingFlags.NonPublic | BindingFlags.Instance);
            if (membersInfo.Length == 0)
            {
                Debug.LogError($"Member not found!!! key = {key}");
                return;
            }
            MemberInfo memberInfo = membersInfo[0];
            Type memberType = (memberInfo.MemberType == MemberTypes.Field) ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                ((FieldInfo)memberInfo).SetValue(data, ConvertType(value, memberType, key));
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                ((PropertyInfo)memberInfo).SetValue(data, ConvertType(value, memberType, key), null);
            }
        }

        /// <summary>
        /// オブジェクトの型変換.オブジェクトを指定された型に解釈する
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="type">Type</param>
        /// <param name="key">Key</param>
        /// <returns>変換されたデータ</returns>
        private object ConvertType(string data, Type type, string key)
        {
            if (type == typeof(int) || type.IsEnum)
            {
                // int/Enum型に変換.Enumはintと等価であるので、intにコンバート可能.
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
                    string tmpStr = Convert.ToString(data);
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
                        Debug.LogError($"illegal type = {type}, key = {key}, data = {data}, tmpStr = {tmpStr}");
                    }
                }
                return 0;

            }
            else if (type == typeof(long))
            {
                // long型に変換.
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
                    Debug.LogError($"illegal type = {type}, key = {key}, data = {data}");
                }
                return 0;

            }
            else if (type == typeof(string))
            {
                // string型に変換.
                if (data == null)
                {
                    return "";
                }
                string stringData = Convert.ToString(data);
                return stringData;

            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list型に変換.

                // リストの要素の型を取得する.
                // ex. List<int>だったら、int型.
                Type elementType = type.GetGenericArguments()[0];

                // リストの型を動的に定める.
                // ex. List<int>等.
                Type listType = typeof(List<>).MakeGenericType(elementType);

                // 動的に定めたリストのインスタンスを生成.
                object listInstance = Activator.CreateInstance(listType);

                // 動的生成したリスト型に要素を加えるためのメソッドを取得.
                MethodInfo addMethod = listType.GetMethod("Add");

                string[] splitData = data.Split(new char[1] { ',' });
                for (int i = 0; i < splitData.Length; i++)
                {
                    object obj = ConvertType(splitData[i], elementType, key);
                    addMethod.Invoke(listInstance, new object[] { obj });
                }
                return listInstance;

            }
            else if (type == typeof(float))
            {
                // float型に変換.
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
                    Debug.LogError($"illegal type = {type}, key = {key}, data = {data}");
                }
                return 0.0f;

            }
            else if (type == typeof(double))
            {
                // double型に変換.
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
                    Debug.LogError($"illegal type = {type}, key = {key}, data = {data}");
                }
                return 0.0d;

            }
            else if (type == typeof(bool))
            {
                // bool型に変換.
                if (data == null)
                {
                    return false;
                }
                else if (int.TryParse(Convert.ToString(data), out _))
                {
                    return Convert.ToInt32(data[0]) > 0;
                }
                else
                {
                    string tmpStr = Convert.ToString(data);
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
                        Debug.LogError($"illegal type = {type}, key = {key}, data = {data}, tmpStr = {tmpStr}");
                    }
                }
                return false;

            }
            else
            {
                // その他の型(object型).
                return data[0] as object;
            }
        }
    }
}