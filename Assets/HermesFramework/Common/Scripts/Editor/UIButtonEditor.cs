#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// UIButton�̊g��
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
            // Button���擾
            var button = menuCommand.context as Button;
            // �Q�[���I�u�W�F�N�g���擾
            var gameObject = button.gameObject;
            // Button�̃f�[�^��ۑ�
            var interactable = button.interactable;
            var transition = button.transition;
            var targetGraphic = button.targetGraphic;
            var colors = button.colors;
            var navigation = button.navigation;
            var onClick = button.onClick;
            // Button�̍폜
            DestroyImmediate(button);
            // UIButton�ǉ�
            var uiButton = gameObject.AddComponent<T>();
            // �f�[�^�̕ۑ�
            uiButton.interactable = interactable;
            uiButton.transition = transition;
            uiButton.targetGraphic = targetGraphic;
            if (uiButton.targetGraphic == null)
                uiButton.targetGraphic = gameObject.GetComponent<Image>() ?? gameObject.GetComponent<UIImage>();
            uiButton.colors = colors;
            uiButton.navigation = navigation;
            uiButton.onClick = onClick;
            // �ۑ�
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif