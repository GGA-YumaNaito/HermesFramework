using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Inspectorの変数名を変更出来るようにする
/// </summary>
public class LabelAttribute : PropertyAttribute
{
    public readonly string Label;

    public LabelAttribute(string label)
    {
        Label = label;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var newLabel = attribute as LabelAttribute;
        label.text = newLabel.Label;
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}
#endif