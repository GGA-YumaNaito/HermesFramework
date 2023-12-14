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
    public class UIButton : Button
    {
        /// <summary>SE Name</summary>
        public string seName = "";
        /// <summary>SE Name</summary>
        public AudioClip seClip = null;
        /// <summary>長押しするか</summary>
        public bool isLongPress = false;
        /// <summary>長押しの時間</summary>
        public float longPressDuration = 0.5f;
        /// <summary>長押しの押下時のUnityEvent</summary>
        public UnityEvent OnLongPressDown = new();
        /// <summary>長押しの離した時のUnityEvent</summary>
        public UnityEvent OnLongPressUp = new();

        /// <summary>長押し判定</summary>
        bool longPressed;
        /// <summary>オブジェクト上での長押し判定</summary>
        bool isLongPressed;
        /// <summary>長押し時間</summary>
        float longPressedTime;
        /// <summary>長押しのDisposable</summary>
        IDisposable longPressDisposable;

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
            
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // SE呼び出し
                if (!seName.IsNullOrEmpty())
                    SoundManager.SE.Play(seName);
                else if (seClip != null)
                    SoundManager.SE.Play(seClip);

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
            else if (seClip != null)
                SoundManager.SE.Play(seClip);

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
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.seClip)), new GUIContent("SE Audio Clip"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.isLongPress)), new GUIContent("Is Long Press"));
            if (component.isLongPress)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.longPressDuration)), new GUIContent("Long Press Duration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.OnLongPressDown)), new GUIContent("On Long Press Down"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(component.OnLongPressUp)), new GUIContent("On Long Press Up"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}