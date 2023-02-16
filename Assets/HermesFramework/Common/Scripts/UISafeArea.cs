using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// UISafeArea(iPhoneのノッチ対策)
    /// </summary>
    public class UISafeArea : MonoBehaviour
    {
        /// <summary>targetTransform</summary>
        [SerializeField] RectTransform targetTransform = null;
        // セーフエリアに合わせたい箇所をtrueにする
        /// <summary>top</summary>
        [SerializeField] bool top;
        /// <summary>bottom</summary>
        [SerializeField] bool bottom;
        /// <summary>left</summary>
        [SerializeField] bool left;
        /// <summary>right</summary>
        [SerializeField] bool right;

        void Start()
        {
            if (targetTransform == null)
                return;

            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            if (top) anchorMax.y /= Screen.height;
            else anchorMax.y = 1;

            if (bottom) anchorMin.y /= Screen.height;
            else anchorMin.y = 0;

            if (left) anchorMin.x /= Screen.width;
            else anchorMin.x = 0;

            if (right) anchorMax.x /= Screen.width;
            else anchorMax.x = 1;

            targetTransform.anchorMin = anchorMin;
            targetTransform.anchorMax = anchorMax;
        }
    }
}