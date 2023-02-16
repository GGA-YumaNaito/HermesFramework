#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// UITextの拡張
    /// </summary>
    public class UITextEditor : EditorWindow
    {
        [MenuItem("CONTEXT/Text/Change Text to UIText")]
        static void ChangeTextToUIText(MenuCommand menuCommand)
        {
            EditorGUI.BeginChangeCheck();
            // Textを取得
            var _text = menuCommand.context as Text;
            // ゲームオブジェクトを取得
            var gameObject = _text.gameObject;
            // Textのデータを保存
            var text = _text.text;
            var font = _text.font;
            var fontStyle = _text.fontStyle;
            var fontSize = _text.fontSize;
            var lineSpacing = _text.lineSpacing;
            var supportRichText = _text.supportRichText;
            var alignment = _text.alignment;
            var alignByGeometry = _text.alignByGeometry;
            var horizontalOverflow = _text.horizontalOverflow;
            var verticalOverflow = _text.verticalOverflow;
            var resizeTextForBestFit = _text.resizeTextForBestFit;
            var color = _text.color;
            var material = _text.material;
            var raycastTarget = _text.raycastTarget;
            var raycastPadding = _text.raycastPadding;
            var maskable = _text.maskable;
            // Textの削除
            DestroyImmediate(_text);
            // UIText追加
            var uiText = gameObject.AddComponent<UIText>();
            // データの保存
            uiText.text = text;
            uiText.font = font;
            uiText.fontStyle = fontStyle;
            uiText.fontSize = fontSize;
            uiText.lineSpacing = lineSpacing;
            uiText.supportRichText = supportRichText;
            uiText.alignment = alignment;
            uiText.alignByGeometry = alignByGeometry;
            uiText.horizontalOverflow = horizontalOverflow;
            uiText.verticalOverflow = verticalOverflow;
            uiText.resizeTextForBestFit = resizeTextForBestFit;
            uiText.color = color;
            uiText.material = material;
            uiText.raycastTarget = raycastTarget;
            uiText.raycastPadding = raycastPadding;
            uiText.maskable = maskable;
            // 保存
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif