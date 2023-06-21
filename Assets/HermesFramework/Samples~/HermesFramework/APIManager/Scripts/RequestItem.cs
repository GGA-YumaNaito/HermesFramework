using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace Hermes.API.Sample
{
    /// <summary>
    /// RequestItem
    /// </summary>
    public class RequestItem : MonoBehaviour
    {
        /// <summary>パラメータ名のテキスト</summary>
        [SerializeField] TextMeshProUGUI paramNameText;
        /// <summary>パラメータ値のテキスト</summary>
        [SerializeField] TMP_InputField paramValueText;

        /// <summary>Param name</summary>
        string paramName;
        /// <summary>Type</summary>
        Type type;

        /// <summary>
        /// 一致するか
        /// </summary>
        /// <param name="paramName">Param name</param>
        /// <returns>true = 一致, false = 不一致</returns>
        public bool IsMatch(string paramName) => this.paramName == paramName;

        /// <summary>
        /// SET
        /// </summary>
        /// <param name="paramName">Param name</param>
        /// <param name="type">Type</param>
        public void Set(string paramName, Type type)
        {
            this.paramName = paramName;
            this.type = type;

            paramNameText.text = paramName;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list.
                // TODO: 今はListを許容しない
                paramValueText.text = "";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Dictionary.
                // Dictionaryは許容しない
                paramValueText.text = "";
            }
            else if (type.IsClass && type != typeof(int) && type != typeof(long) && type != typeof(float) && type != typeof(double) && type != typeof(string) && type != typeof(bool) && type != typeof(object))
            {
                // class.
                // TODO: 今はclassを許容しない
                paramValueText.text = "";
            }
            else if (type == typeof(int) || type.IsEnum && type.GetCustomAttribute(typeof(FlagsAttribute)) == null)
            {
                // int.
                paramValueText.text = default(int).ToString();
            }
            else if (type == typeof(long))
            {
                // long.
                paramValueText.text = default(long).ToString();
            }
            else if (type == typeof(float))
            {
                // float.
                paramValueText.text = default(float).ToString();
            }
            else if (type == typeof(double))
            {
                // double.
                paramValueText.text = default(double).ToString();
            }
            else if (type == typeof(string))
            {
                // string.
                paramValueText.text = default;
            }
            else if (type == typeof(bool))
            {
                // bool.
                paramValueText.text = default(bool).ToString();
            }
            else
            {
                // object.
                paramValueText.text = "";
            }

        }

        /// <summary>
        /// Get value
        /// </summary>
        public object GetValue()
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                // list.
                // TODO: 今はListを許容しない
                return null;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                // Dictionary.
                // Dictionaryは許容しない
                return null;
            }
            else if (type.IsClass && type != typeof(int) && type != typeof(long) && type != typeof(float) && type != typeof(double) && type != typeof(string) && type != typeof(bool) && type != typeof(object))
            {
                // class.
                // TODO: 今はclassを許容しない
                return null;
            }
            else if (type == typeof(int) || type.IsEnum && type.GetCustomAttribute(typeof(FlagsAttribute)) == null)
            {
                // int.
                return Convert.ToInt32(paramValueText.text);
            }
            else if (type == typeof(long))
            {
                // long.
                return Convert.ToInt64(paramValueText.text);
            }
            else if (type == typeof(float))
            {
                // float.
                return Convert.ToSingle(paramValueText.text);
            }
            else if (type == typeof(double))
            {
                // double.
                return Convert.ToDouble(paramValueText.text);
            }
            else if (type == typeof(string))
            {
                // string.
                return paramValueText.text;
            }
            else if (type == typeof(bool))
            {
                // bool.
                return Convert.ToBoolean(paramValueText.text);
            }
            else
            {
                // object.
                paramValueText.text = "";
                return null;
            }
        }
    }
}