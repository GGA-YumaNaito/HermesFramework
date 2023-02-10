#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// UIImageの拡張
    /// </summary>
    public class UIImageEditor : EditorWindow
    {
        [MenuItem("CONTEXT/Image/Change Image to UIImage")]
        static void ChangeImageToUIImage(MenuCommand menuCommand)
        {
            EditorGUI.BeginChangeCheck();
            // Imageを取得
            var image = menuCommand.context as Image;
            // ゲームオブジェクトを取得
            var gameObject = image.gameObject;
            // Imageのデータを保存
            var sprite = image.sprite;
            var color = image.color;
            var material = image.material;
            var raycastTarget = image.raycastTarget;
            var raycastPadding = image.raycastPadding;
            var maskable = image.maskable;
            // Imageの削除
            DestroyImmediate(image);
            // UIImage追加
            var uiImage = gameObject.AddComponent<UIImage>();
            // データの保存
            uiImage.sprite = sprite;
            uiImage.color = color;
            uiImage.material = material;
            uiImage.raycastTarget = raycastTarget;
            uiImage.raycastPadding = raycastPadding;
            uiImage.maskable = maskable;
            // 保存
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif