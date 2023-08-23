using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace Hermes.UI.Editor
{
    /// <summary>
    /// HermesのUIに置き換える
    /// <para>選択したオブジェクトの中にHermes.UIに</para>
    /// <para>置換出来るコンポーネントが有った場合変換する</para>
    /// </summary>
    public class HermesUIReplacer : EditorWindow
    {
        private AnimBool showInfo;

        // メニューのHermesにHermesUIReplacerという項目を追加。
        [MenuItem("Hermes/HermesUIReplacer")]
        static void Open()
        {
            var window = GetWindow<HermesUIReplacer>("Hermes UI Replacer");
            window.Show();
        }

        // エラーメッセージ表示用のAnimBoolを作成
        private void OnEnable()
        {
            this.showInfo = new AnimBool(true);
            this.showInfo.valueChanged.AddListener(this.Repaint);
        }

        // Windowのクライアント領域のGUI処理を記述
        void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                var infoMessage = string.Empty;
                var infoType = MessageType.None;

                // もしエラーメッセージが出ていたら表示する
                this.showInfo.target = !string.IsNullOrEmpty(infoMessage);
                using (var fadeGroup = new EditorGUILayout.FadeGroupScope(this.showInfo.faded))
                {
                    if (fadeGroup.visible)
                    {
                        EditorGUILayout.HelpBox(infoMessage, infoType, true);
                    }
                }

                // 選択されているゲームオブジェクトやそれらの子階層から変換出来るコンポーネントを探し、
                // 1つ以上見つかって、かつエラーが出ていなければ「Replace」ボタンを使用可能にする
                var selectedObjects = Selection.gameObjects;
                List<Component> selectedComponents = new();
                var types = new List<Type>() { typeof(Image), typeof(Button), typeof(Text) };
                foreach (var type in types)
                {
                    var components = selectedObjects.SelectMany(gameObject => gameObject.GetComponentsInChildren(type)).Distinct();
                    foreach (var component in components)
                    {
                        if (component.GetType() == type)
                            selectedComponents.Add(component);
                    }
                }

                try
                {
                    // Hermes.UIに変換できるオブジェクトがなかったら失敗
                    if (!selectedComponents.Any())
                    {
                        throw new Exception($"There are no objects that can be converted to Hermes.UI.");
                    }
                }
                catch (Exception exception)
                {
                    infoMessage = exception.Message;
                    infoType = MessageType.Error;
                }

                using (new EditorGUI.DisabledScope(infoType == MessageType.Error))
                {
                    if (GUILayout.Button("Replace"))
                    {
                        Replace();
                    }
                }
            }
        }

        private void Replace()
        {
            Type fromType = null, toType = null;
            var selectedObjects = Selection.gameObjects;
            var types = new List<KeyValuePair<Type, Type>>() {
                new KeyValuePair<Type, Type>(typeof(Image), typeof(UIImage)),
                new KeyValuePair<Type, Type>(typeof(Button), typeof(UIButton)),
                new KeyValuePair<Type, Type>(typeof(Text), typeof(UIText))
            };
            foreach (var type in types)
            {
                fromType = type.Key;
                toType = type.Value;
                var selectedComponents = selectedObjects.SelectMany(gameObject => gameObject.GetComponentsInChildren(fromType)).Distinct();

                Debug.Log($"{fromType} -> {toType}");
                foreach (var component in selectedComponents)
                {
                    if (component.GetType() == toType)
                        continue;
                    var gameObject = component.gameObject;
                    Debug.Log($"Object Name = {gameObject.name}");
                    Undo.SetCurrentGroupName($"Replace Hermes UI");

                    // まず元のコンポーネントをゲームオブジェクトごと複製
                    var cloneObject = Instantiate(gameObject, gameObject.transform.parent);
                    var cloneComponent = cloneObject.GetComponent(fromType);

                    // なぜかtargetGraphicはコピーしてくれないから手動で移動
                    if (typeof(Selectable).IsAssignableFrom(fromType))
                    {
                        var targetGraphic = gameObject.GetComponent<Selectable>().targetGraphic;
                        ((Selectable)cloneComponent).targetGraphic = targetGraphic;
                    }

                    // typeがImageの時、同じオブジェクトのTarget Graphicに設定されていた場合設定する
                    var isTargetGraphic = false;
                    if (fromType == typeof(Image))
                    {
                        if (gameObject.TryGetComponent<Selectable>(out var selectable))
                        {
                            isTargetGraphic = selectable.targetGraphic == component;
                        }
                    }

                    // 元のコンポーネントは削除し、置き換え先のコンポーネントをアタッチ
                    Undo.DestroyObjectImmediate(component);
                    var newComponent = Undo.AddComponent(gameObject, toType);

                    // 複製しておいた元コンポーネントから新しいコンポーネントにデータをコピー
                    EditorUtility.CopySerializedManagedFieldsOnly(cloneComponent, newComponent);

                    // typeがImageの時、同じオブジェクトのTarget Graphicに設定されていた場合設定する
                    if (isTargetGraphic)
                    {
                        gameObject.GetComponent<Selectable>().targetGraphic = (Graphic)newComponent;
                    }

                    // 複製は不要になったので削除する
                    DestroyImmediate(cloneObject);
                }
            }
        }
    }
}