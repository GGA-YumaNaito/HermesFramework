#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// UIButtonの拡張
    /// </summary>
    public class UIButtonEditor : EditorWindow
    {
        [MenuItem("CONTEXT/Button/Change Button to UIButton")]
        static void ChangeButtonToUIButton(MenuCommand menuCommand)
        {
            ChangeProcess<UIButton>(menuCommand);
        }

        [MenuItem("CONTEXT/Button/Change Button to UIBackButton")]
        static void ChangeButtonToUIBackButton(MenuCommand menuCommand)
        {
            ChangeProcess<UIBackButton>(menuCommand);
        }
        static void ChangeProcess<T>(MenuCommand menuCommand) where T : Button
        {
            EditorGUI.BeginChangeCheck();
            // Buttonを取得
            var button = menuCommand.context as Button;
            // ゲームオブジェクトを取得
            var gameObject = button.gameObject;
            // Buttonのデータを保存
            var interactable = button.interactable;
            var transition = button.transition;
            var targetGraphic = button.targetGraphic;
            var colors = button.colors;
            var navigation = button.navigation;
            var onClick = button.onClick;
            // Buttonの削除
            DestroyImmediate(button);
            // UIButton追加
            var uiButton = gameObject.AddComponent<T>();
            // データの保存
            uiButton.interactable = interactable;
            uiButton.transition = transition;
            uiButton.targetGraphic = targetGraphic;
            if (uiButton.targetGraphic == null)
                uiButton.targetGraphic = gameObject.GetComponent<Image>() ?? gameObject.GetComponent<UIImage>();
            uiButton.colors = colors;
            uiButton.navigation = navigation;
            uiButton.onClick = onClick;
            // 保存
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif