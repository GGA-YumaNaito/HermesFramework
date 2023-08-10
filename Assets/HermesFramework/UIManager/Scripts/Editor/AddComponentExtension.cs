using System.Reflection;
using UnityEngine;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// AddComponentExtension
    /// </summary>
    public static class AddComponentExtension
    {
        /// <summary>
        /// AddComponentExt
        /// </summary>
        /// <param name="obj">this GameObject</param>
        /// <param name="nameSpace">namespace</param>
        /// <param name="scriptName">Script name</param>
        /// <returns>Component</returns>
        public static Component AddComponentExt(this GameObject obj, string nameSpace, string scriptName)
        {
            Assembly asm = Assembly.Load("Assembly-CSharp");
            var type = asm?.GetType(nameSpace + "." + scriptName) ?? null;
            if (type == null)
            {
                Debug.LogError($"Failed to ComponentType:{scriptName}");
                return null;
            }
            return obj.AddComponent(type);
        }
    }
}