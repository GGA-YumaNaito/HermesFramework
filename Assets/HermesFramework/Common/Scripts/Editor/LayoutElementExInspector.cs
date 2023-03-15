using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// LayoutElementExInspector
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LayoutElementEx))]
    public class LayoutElementExInspector : LayoutElementEditor
    {
        /// <summary>preferredの代わりとなるRectTransform</summary>
        SerializedProperty preferredRectTransform;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            this.preferredRectTransform = serializedObject.FindProperty("preferredRectTransform");
        }


        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            LayoutElementEx script = (LayoutElementEx)target;

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(script), typeof(MonoScript), false);
            GUI.enabled = true;

            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(this.preferredRectTransform);

            serializedObject.ApplyModifiedProperties();
        }
    }
}