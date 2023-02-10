#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// UIImage�̊g��
    /// </summary>
    public class UIImageEditor : EditorWindow
    {
        [MenuItem("CONTEXT/Image/Change Image to UIImage")]
        static void ChangeImageToUIImage(MenuCommand menuCommand)
        {
            EditorGUI.BeginChangeCheck();
            // Image���擾
            var image = menuCommand.context as Image;
            // �Q�[���I�u�W�F�N�g���擾
            var gameObject = image.gameObject;
            // Image�̃f�[�^��ۑ�
            var sprite = image.sprite;
            var color = image.color;
            var material = image.material;
            var raycastTarget = image.raycastTarget;
            var raycastPadding = image.raycastPadding;
            var maskable = image.maskable;
            // Image�̍폜
            DestroyImmediate(image);
            // UIImage�ǉ�
            var uiImage = gameObject.AddComponent<UIImage>();
            // �f�[�^�̕ۑ�
            uiImage.sprite = sprite;
            uiImage.color = color;
            uiImage.material = material;
            uiImage.raycastTarget = raycastTarget;
            uiImage.raycastPadding = raycastPadding;
            uiImage.maskable = maskable;
            // �ۑ�
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif