using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hermes.Sound;
using System;
using UnityEngine.Events;
using UniRx;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hermes.UI
{
    /// <summary>
    /// ボタンのラッパークラス
    /// </summary>
    public class UIButton : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>SE Name</summary>
        public string seName = "";
        /// <summary>長押しするか</summary>
        public bool isLongPress = false;
        /// <summary>長押しの時間</summary>
        public float longPressDuration = 0.5f;
        /// <summary>長押しの押下時のUnityEvent</summary>
        public UnityEvent OnLongPressDown = new();
        /// <summary>長押しの離した時のUnityEvent</summary>
        public UnityEvent OnLongPressUp = new();
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

        /// <summary>長押し判定</summary>
        bool longPressed;
        /// <summary>オブジェクト上での長押し判定</summary>
        bool isLongPressed;
        /// <summary>長押し時間</summary>
        float longPressedTime;
        /// <summary>長押しのDisposable</summary>
        IDisposable longPressDisposable;

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

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // SE呼び出し
                if (!seName.IsNullOrEmpty())
                    SoundManager.SE.Play(seName);

                // 長押し
                if (isLongPressed)
                {
                    OnLongPressUp.Invoke();
                    isLongPressed = false;
                }
            }
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);

            // SE呼び出し
            if (!seName.IsNullOrEmpty())
                SoundManager.SE.Play(seName);

            // 長押し
            if (isLongPressed)
            {
                OnLongPressUp.Invoke();
                isLongPressed = false;
            }
        }

        /// <summary>
        /// ボタンを押下した時
        /// </summary>
        /// <param name="eventData">eventData</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!isLongPress)
                return;

            longPressed = true;
            isLongPressed = false;

            if (longPressDisposable == null)
            {
                longPressDisposable = Observable.EveryUpdate().Subscribe(i =>
                {
                    if (longPressed)
                    {
                        longPressedTime += Time.deltaTime;

                        if (longPressedTime >= longPressDuration)
                        {
                            longPressed = false;
                            OnLongPressDown.Invoke();
                            isLongPressed = true;
                        }
                    }
                }).AddTo(this);
            }
        }

        /// <summary>
        /// ボタンを離した時
        /// </summary>
        /// <param name="eventData">eventData</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            if (!isLongPress)
                return;

            longPressed = false;
            longPressedTime = 0f;
            longPressDisposable.Dispose();
            longPressDisposable = null;
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
    /// UIButtonEditor
    /// </summary>
    [CustomEditor(typeof(UIButton))]
    public class UIButtonEditor : UnityEditor.UI.ButtonEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var component = (UIButton)target;

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.seName)), new GUIContent("SE Name"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.isLongPress)), new GUIContent("Is Long Press"));
            if (component.isLongPress)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.longPressDuration)), new GUIContent("Long Press Duration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.OnLongPressDown)), new GUIContent("On Long Press Down"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.OnLongPressUp)), new GUIContent("On Long Press Up"));
                EditorGUI.indentLevel--;
            }

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