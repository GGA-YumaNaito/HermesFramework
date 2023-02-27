using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Hermes.Log
{
    /// <summary>
    /// ObjectDumpLog
    /// </summary>
    public class ObjectDumpLog : MonoBehaviour
    {
        /// <summary>StringBuilder.</summary>
        private static StringBuilder m_StringBuilder;
        /// <summary>The method info caches.</summary>
        private static Dictionary<string, MethodInfo> m_MethodInfoCaches = new Dictionary<string, MethodInfo>();
        /// <summary>The delegate caches.</summary>
        private static Dictionary<string, Delegate> m_DelegateCaches = new Dictionary<string, Delegate>();
        /// <summary>Count.</summary>
        private static int m_Count;
        /// <summary>
        /// デバッグログ.
        /// Jsonの形で出力される.
        /// </summary>
        /// <returns>The log.</returns>
        /// <param name="data">Data.</param>
        public static string DebugLog(object data)
        {
            m_StringBuilder = m_StringBuilder ?? new StringBuilder();
            m_Count = 1;
            m_StringBuilder.Append("{");
            m_StringBuilder.AppendLine();
            DebugLogMember(data);
            m_StringBuilder.Append("}");
            m_StringBuilder.AppendLine();
            Debug.Log("<color=red>↓↓↓↓↓ DebugLog(Json ver) ↓↓↓↓↓</color>");
            string log = m_StringBuilder.ToString();
            Debug.Log(log);
            m_StringBuilder = null;
            m_Count = 0;
            Debug.Log("<color=red>↑↑↑↑↑ DebugLog(Json ver) ↑↑↑↑↑</color>");
            return log;
        }
        /// <summary>Debugs the log member.</summary>
        /// <param name="data">Data.</param>
        private static void DebugLogMember(object data)
        {
            if (data == null)
            {
                return;
            }
            MemberInfo[] membersInfo;
            // キャッシュしていたらキャッシュから取得.
            if (Cache.MembersInfoCaches.ContainsKey(data.GetType().ToString()))
            {
                membersInfo = Cache.MembersInfoCaches[data.GetType().ToString()];
            }
            else
            {
                membersInfo = data.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                Cache.MembersInfoCaches[data.GetType().ToString()] = membersInfo;
            }
            Type memberType;
            for (int i = 0; i < membersInfo.Count(); i++)
            {
                // メンバタイプ取得.
                if (membersInfo[i].MemberType == MemberTypes.Field)
                {
                    memberType = ((FieldInfo)membersInfo[i]).FieldType;
                }
                else if (membersInfo[i].MemberType == MemberTypes.Property)
                {
                    memberType = ((PropertyInfo)membersInfo[i]).PropertyType;
                }
                else
                {
                    continue;
                }
                DebugLogSpace(m_Count);
                m_StringBuilder.Append("\"");
                m_StringBuilder.Append(membersInfo[i].Name);
                m_StringBuilder.Append("\" : ");
                object value = (membersInfo[i].MemberType == MemberTypes.Field ? ((FieldInfo)membersInfo[i]).GetValue(data) : ((PropertyInfo)membersInfo[i]).GetValue(data, null));
                DebugLogValue(memberType, value);
                m_StringBuilder.Append(",");
                m_StringBuilder.AppendLine();
            }
            m_StringBuilder.Remove(m_StringBuilder.Length - 2, 2);
            m_StringBuilder.AppendLine();
        }
        /// <summary>Debugs the log value.</summary>
        /// <param name="type">Type.</param>
        /// <param name="data">Data.</param>
        private static void DebugLogValue(Type type, object data)
        {
            m_Count++;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list.
                DebugLogValueList(type, data);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Dictionary.
                DebugLogValueDictionary(type, data);
            }
            else if (type.IsClass && type != typeof(int) && type != typeof(long) && type != typeof(float) && type != typeof(double) && type != typeof(string) && type != typeof(bool) && type != typeof(object))
            {
                // class.
                m_StringBuilder.Append("{");
                m_StringBuilder.AppendLine();
                DebugLogMember(data);
                DebugLogSpace(m_Count - 1);
                m_StringBuilder.Append("}");
            }
            else if (type == typeof(int) || type.IsEnum && type.GetCustomAttribute(typeof(FlagsAttribute)) == null)
            {
                // int.
                m_StringBuilder.Append((int)data);
            }
            else if (type == typeof(long))
            {
                // long.
                m_StringBuilder.Append((long)data);
            }
            else if (type == typeof(float))
            {
                // float.
                m_StringBuilder.Append((float)data);
            }
            else if (type == typeof(double))
            {
                // double.
                m_StringBuilder.Append((double)data);
            }
            else if (type == typeof(string))
            {
                // string.
                m_StringBuilder.Append("\"");
                m_StringBuilder.Append((string)data);
                m_StringBuilder.Append("\"");
            }
            else if (type == typeof(bool))
            {
                // bool.
                m_StringBuilder.Append(Convert.ToString(data) == "TRUE" ? true : false);
            }
            else
            {
                // object.
                m_StringBuilder.Append("\"");
                m_StringBuilder.Append(data);
                m_StringBuilder.Append("\"");
            }
            m_Count--;
        }
        /// <summary>Debugs the log value list.</summary>
        /// <param name="type">Type.</param>
        /// <param name="data">Data.</param>
        private static void DebugLogValueList(Type type, object data)
        {
            m_StringBuilder.Append("[");
            m_StringBuilder.AppendLine();
            // リストの要素の型を取得する.
            // ex. List<int>だったら、int型.
            Type elementType = type.GetGenericArguments()[0];
            Action<int, int, object> listAction = (i, max, o) =>
            {
                DebugLogSpace(m_Count);
                DebugLogValue(elementType, o);
                if (i < max - 1)
                {
                    m_StringBuilder.Append(",");
                }
                m_StringBuilder.AppendLine();
            };
            if (data == null)
            {
            }
            else if (elementType == typeof(int) || elementType.IsEnum && type.GetCustomAttribute(typeof(FlagsAttribute)) == null)
            {
                // int.
                ICollection<int> list = data as ICollection<int>;
                int[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            else if (elementType == typeof(long))
            {
                // long.
                ICollection<long> list = data as ICollection<long>;
                long[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            else if (elementType == typeof(float))
            {
                // float.         
                ICollection<float> list = data as ICollection<float>;
                float[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            else if (elementType == typeof(double))
            {
                // double.     
                ICollection<double> list = data as ICollection<double>;
                double[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            else if (elementType == typeof(string))
            {
                // string.         
                ICollection<string> list = data as ICollection<string>;
                string[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list.          
                Type listType = typeof(List<>).MakeGenericType(elementType);
                Type actionType = typeof(Action<>).MakeGenericType(elementType);
                // DebugLogValueListDataMethod.
                MethodInfo debugLogValueListDataMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey("DebugLogValueListData"))
                {
                    debugLogValueListDataMethod = m_MethodInfoCaches["DebugLogValueListData"];
                }
                else
                {
                    debugLogValueListDataMethod = typeof(ObjectDumpLog).GetMethod("DebugLogValueListData", BindingFlags.Static | BindingFlags.NonPublic);
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches["DebugLogValueListData"] = debugLogValueListDataMethod;
                }
                // Delegate.
                Delegate action;
                // キャッシュしていたらキャッシュから取得.
                if (m_DelegateCaches.ContainsKey(elementType.ToString()))
                {
                    action = m_DelegateCaches[elementType.ToString()];
                }
                else
                {
                    action = Delegate.CreateDelegate(actionType, null, debugLogValueListDataMethod);
                    // キャッシュされていなかったらキャッシュ.
                    m_DelegateCaches[elementType.ToString()] = action;
                }
                // ForEach.
                MethodInfo forEachMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey(elementType + "_ForEach"))
                {
                    forEachMethod = m_MethodInfoCaches[elementType + "_ForEach"];
                }
                else
                {
                    forEachMethod = listType.GetMethod("ForEach");
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches[elementType + "_ForEach"] = forEachMethod;
                }
                // 実行.
                forEachMethod.Invoke(data, new object[] { action });
                m_StringBuilder.Remove(m_StringBuilder.Length - 2, 2);
                m_StringBuilder.AppendLine();
            }
            else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Dictionary.            DebugLogValueDictionary (elementType, data);
            }
            else if (elementType.IsClass)
            {
                // class.
                Type listType = typeof(List<>).MakeGenericType(elementType);
                Type actionType = typeof(Action<>).MakeGenericType(elementType);
                // DebugLogValueClassMethod.
                MethodInfo debugLogValueClassMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey("DebugLogValueClass"))
                {
                    debugLogValueClassMethod = m_MethodInfoCaches["DebugLogValueClass"];
                }
                else
                {
                    debugLogValueClassMethod = typeof(ObjectDumpLog).GetMethod("DebugLogValueClass", BindingFlags.Static | BindingFlags.NonPublic);
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches["DebugLogValueClass"] = debugLogValueClassMethod;
                }
                // Delegate.
                Delegate action;
                // キャッシュしていたらキャッシュから取得.
                if (m_DelegateCaches.ContainsKey(elementType.ToString()))
                {
                    action = m_DelegateCaches[elementType.ToString()];
                }
                else
                {
                    action = Delegate.CreateDelegate(actionType, null, debugLogValueClassMethod);
                    // キャッシュされていなかったらキャッシュ.
                    m_DelegateCaches[elementType.ToString()] = action;
                }
                // ForEach.
                MethodInfo forEachMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey(elementType + "_ForEach"))
                {
                    forEachMethod = m_MethodInfoCaches[elementType + "_ForEach"];
                }
                else
                {
                    forEachMethod = listType.GetMethod("ForEach");
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches[elementType + "_ForEach"] = forEachMethod;
                }
                // 実行.
                forEachMethod.Invoke(data, new object[] { action });
                m_StringBuilder.Remove(m_StringBuilder.Length - 2, 2);
                m_StringBuilder.AppendLine();
            }
            else if (elementType == typeof(bool))
            {
                // bool.
                ICollection<bool> list = data as ICollection<bool>;
                bool[] array = list.ToArray();
                for (int i = 0; i < array.Count(); i++)
                {
                    listAction(i, array.Count(), (object)array[i]);
                }
            }
            DebugLogSpace(m_Count - 1);
            m_StringBuilder.Append("]");
        }
        /// <summary>Debugs the log value dictionary.</summary>
        /// <param name="type">Type.</param>
        /// <param name="data">Data.</param>
        private static void DebugLogValueDictionary(Type type, object data)
        {
            m_StringBuilder.Append("{");
            m_StringBuilder.AppendLine();
            // ディクショナリーの要素の型を取得する.
            // ex. Dictionary<string, object>だったら、string型.
            Type elementType = type.GetGenericArguments()[0];
            // ex. Dictionary<string, object>だったら、object型.
            Type elementValue = type.GetGenericArguments()[1];
            Action<int, int, object, object> dictionaryAction = (i, max, key, value) =>
            {
                DebugLogSpace(m_Count);
                m_StringBuilder.Append("\"");
                m_StringBuilder.Append(key);
                m_StringBuilder.Append("\"");
                m_StringBuilder.Append(" : ");
                DebugLogValue(elementValue, value);
                if (i < max - 1)
                {
                    m_StringBuilder.Append(",");
                }
                m_StringBuilder.AppendLine();
            };
            if (data == null)
            {
            }
            else if (elementType == typeof(int) || elementType == typeof(long) || elementType.IsEnum && type.GetCustomAttribute(typeof(FlagsAttribute)) == null)
            {
                // int.
                IDictionary dic = data as IDictionary;
                int i = 0;
                List<long> listKey = new List<long>();
                foreach (object k in dic.Keys)
                {
                    listKey.Add(Convert.ToInt64(k));
                }
                foreach (object v in dic.Values)
                {
                    dictionaryAction(i, dic.Count, (object)listKey[i], v);
                    i++;
                }
            }
            else if (elementType == typeof(float) || elementType == typeof(double))
            {
                // float.
                // double.
                IDictionary dic = data as IDictionary;
                int i = 0;
                List<double> listKey = new List<double>();
                foreach (object k in dic.Keys)
                {
                    listKey.Add(Convert.ToDouble(k));
                }
                foreach (object v in dic.Values)
                {
                    dictionaryAction(i, dic.Count, (object)listKey[i], v);
                    i++;
                }
            }
            else if (elementType == typeof(string))
            {
                // string.
                IDictionary dic = data as IDictionary;
                int i = 0;
                List<string> listKey = new List<string>();
                foreach (object k in dic.Keys)
                {
                    listKey.Add(Convert.ToString(k));
                }
                foreach (object v in dic.Values)
                {
                    dictionaryAction(i, dic.Count, (object)listKey[i], v);
                    i++;
                }
            }
            else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list.
                DebugLogValueList(elementType, data);
            }
            else if (elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Dictionary.
                DebugLogValueDictionary(elementType, data);
            }
            else if (elementType.IsClass)
            {
                // class.
                Type listType = typeof(List<>).MakeGenericType(elementType);
                Type actionType = typeof(Action<>).MakeGenericType(elementType);
                // DebugLogValueClassMethod.
                MethodInfo debugLogValueClassMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey("DebugLogValueClass"))
                {
                    debugLogValueClassMethod = m_MethodInfoCaches["DebugLogValueClass"];
                }
                else
                {
                    debugLogValueClassMethod = typeof(ObjectDumpLog).GetMethod("DebugLogValueClass", BindingFlags.Static | BindingFlags.NonPublic);
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches["DebugLogValueClass"] = debugLogValueClassMethod;
                }
                // Delegate.
                Delegate action;
                // キャッシュしていたらキャッシュから取得.
                if (m_DelegateCaches.ContainsKey(elementType.ToString()))
                {
                    action = m_DelegateCaches[elementType.ToString()];
                }
                else
                {
                    action = Delegate.CreateDelegate(actionType, null, debugLogValueClassMethod);
                    // キャッシュされていなかったらキャッシュ.
                    m_DelegateCaches[elementType.ToString()] = action;
                }
                // ForEach.
                MethodInfo forEachMethod;
                // キャッシュしていたらキャッシュから取得.
                if (m_MethodInfoCaches.ContainsKey(elementType + "_ForEach"))
                {
                    forEachMethod = m_MethodInfoCaches[elementType + "_ForEach"];
                }
                else
                {
                    forEachMethod = listType.GetMethod("ForEach");
                    // キャッシュされていなかったらキャッシュ.
                    m_MethodInfoCaches[elementType + "_ForEach"] = forEachMethod;
                }
                // 実行.
                forEachMethod.Invoke(data, new object[] { action });
                m_StringBuilder.Remove(m_StringBuilder.Length - 2, 2);
                m_StringBuilder.AppendLine();
            }
            else if (elementType == typeof(bool))
            {
                // bool.
                IDictionary dic = data as IDictionary;
                int i = 0;
                List<bool> listKey = new List<bool>();
                foreach (object k in dic.Keys)
                {
                    listKey.Add(Convert.ToBoolean(k));
                }
                foreach (object v in dic.Values)
                {
                    dictionaryAction(i, dic.Count, (object)listKey[i], v);
                    i++;
                }
            }
            DebugLogSpace(m_Count - 1);
            m_StringBuilder.Append("}");
        }
        /// <summary>Debugs the log value list data.</summary>
        /// <param name="data">Data.</param>
        private static void DebugLogValueListData(object data)
        {
            DebugLogSpace(m_Count);
            m_Count++;
            DebugLogValueList(data.GetType(), data);
            m_Count--;
            m_StringBuilder.Append(",");
            m_StringBuilder.AppendLine();
        }
        /// <summary>Debugs the log value class.</summary>
        /// <param name="data">Data.</param>
        private static void DebugLogValueClass(object data)
        {
            DebugLogSpace(m_Count);
            m_StringBuilder.Append("{");
            m_StringBuilder.AppendLine();
            m_Count++;
            DebugLogMember(data);
            m_Count--;
            DebugLogSpace(m_Count);
            m_StringBuilder.Append("},");
            m_StringBuilder.AppendLine();
        }
        /// <summary>スペースの埋め込み.</summary>
        /// <param name="count">Count.</param>
        private static void DebugLogSpace(int count)
        {
            for (int i = 0; i < count; i++)
            {
                m_StringBuilder.Append("    ");
            }
        }
    }
}