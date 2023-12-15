using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hermes.UI
{
    /// <summary>
    /// ドラッグボタン
    /// </summary>
    public class UIDragButton : UIButton, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>ドラッグするか</summary>
        public bool isDrag = false;
        /// <summary>ドラッグ前の位置に戻すフラグ : true 戻す, false 戻さない</summary>
        public bool isPrevPosition = true;

        /// <summary>DragEndDelegate</summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="eventData">EventData</param>
        public delegate void DragEndDelegate<T>(T eventData);
        /// <summary>Drag終了時のイベント</summary>
        public event DragEndDelegate<PointerEventData> DragEndEvent = delegate { };

        /// <summary>カメラ</summary>
        Camera dragCamera;
        /// <summary>ドラッグ前の位置</summary>
        Vector2 prevPos;
        /// <summary>RectTransform</summary>
        RectTransform rectTransform;
        /// <summary>親のRectTransform</summary>
        RectTransform parentRectTransform;

        protected override void Awake()
        {
            base.Awake();

            rectTransform = gameObject.GetComponent<RectTransform>();
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas && canvas.renderMode == RenderMode.ScreenSpaceCamera)
                dragCamera = canvas.worldCamera;
            if (!parentRectTransform)
                parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// ドラッグ前
        /// </summary>
        /// <param name="eventData">eventData</param>
        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDrag)
                return;

            // ドラッグ前の位置を記憶しておく
            prevPos = transform.localPosition;
        }

        /// <summary>
        /// ドラッグ中
        /// </summary>
        /// <param name="eventData">eventData</param>
        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!isDrag)
                return;

            Vector2 result;
            if (dragCamera)
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, dragCamera, out result);
            else
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, null, out result);
            rectTransform.localPosition = result;
        }

        /// <summary>
        /// ドラッグ後
        /// </summary>
        /// <param name="eventData">eventData</param>
        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!isDrag)
                return;

            // ドラッグ前の位置に戻す
            if (isPrevPosition)
                transform.localPosition = prevPos;

            // ドラッグ終了イベント
            DragEndEvent(eventData);
        }

        /// <summary>
        /// ドラッグ前の位置に戻すフラグ設定
        /// <para>true 戻す, false 戻さない</para>
        /// </summary>
        /// <param name="isPrevPosition">isPrevPosition</param>
        public void SetIsPrevPosition(bool isPrevPosition)
        {
            this.isPrevPosition = isPrevPosition;
        }
    }

    //==============================================================================================

#if UNITY_EDITOR
    /// <summary>
    /// UIBackButtonEditor
    /// </summary>
    [CustomEditor(typeof(UIDragButton))]
    public class UIDragButtonEditor : UIButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var component = (UIDragButton)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.isDrag)), new GUIContent("Is Drag"));
            if (component.isDrag)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.isPrevPosition)), new GUIContent("Is Prev Position"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}