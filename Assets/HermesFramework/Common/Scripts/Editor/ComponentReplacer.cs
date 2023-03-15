using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// コンポーネントを置き換える
/// </summary>
public class ComponentReplacer : EditorWindow
{
    // ウィンドウを開いたときのデフォルトの置き換え元、置き換え先を変える場合はこの2つを書き換える
    private static readonly Type DefaultFromType = typeof(Button);
    private static readonly Type DefaultToType = typeof(Hermes.UI.UIButton);

    private AnimBool showInfo;
    private string fromTypeString = $"{DefaultFromType.FullName}, {DefaultFromType.Assembly.GetName().Name}";
    private string toTypeString = $"{DefaultToType.FullName}, {DefaultToType.Assembly.GetName().Name}";

    // メニューのHermesにComponentReplacerという項目を追加。
    [MenuItem("Hermes/ComponentReplacer")]
    static void Open()
    {
        var window = GetWindow<ComponentReplacer>("Component Replacer");
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
            // 「From Type」、「To Type」の文字列と対応する型を探す
            var infoMessage = string.Empty;
            var infoType = MessageType.None;
            Type fromType = null;
            Type toType = null;
            try
            {
                this.fromTypeString = EditorGUILayout.TextField("From Type", this.fromTypeString);
                fromType = Type.GetType(this.fromTypeString, true);
                if (!typeof(Component).IsAssignableFrom(fromType))
                {
                    // 置き換え元の型がComponentから派生していなければ失敗
                    throw new Exception($"{this.fromTypeString} is not compatible with {typeof(Component)}.");
                }

                this.toTypeString = EditorGUILayout.TextField("To Type", this.toTypeString);
                toType = Type.GetType(this.toTypeString, true);
                if (!fromType.IsAssignableFrom(toType))
                {
                    // 置き換え先の型が置き換え元の型から派生していなければ失敗
                    throw new Exception($"{this.toTypeString} is not compatible with {fromType}.");
                }
            }
            catch (Exception exception)
            {
                infoMessage = exception.Message;
                infoType = MessageType.Error;
            }

            // もしエラーメッセージが出ていたら表示する
            this.showInfo.target = !string.IsNullOrEmpty(infoMessage);
            using (var fadeGroup = new EditorGUILayout.FadeGroupScope(this.showInfo.faded))
            {
                if (fadeGroup.visible)
                {
                    EditorGUILayout.HelpBox(infoMessage, infoType, true);
                }
            }

            // 選択されているゲームオブジェクトやそれらの子階層からfromTypeコンポーネントを探し、
            // 1つ以上見つかって、かつエラーが出ていなければ「Replace」ボタンを使用可能にする
            var selectedObjects = Selection.gameObjects;
            var selectedComponents =
                selectedObjects.SelectMany(gameObject => gameObject.GetComponentsInChildren(fromType)).Distinct();
            using (new EditorGUI.DisabledScope((infoType == MessageType.Error) || !selectedComponents.Any()))
            {
                if (GUILayout.Button("Replace"))
                {
                    this.Replace(fromType, toType, selectedComponents);
                }
            }
        }
    }

    private void Replace(Type fromType, Type toType, IEnumerable<Component> components)
    {
        Debug.Log($"{fromType} -> {toType}");
        foreach (var component in components)
        {
            var gameObject = component.gameObject;
            Debug.Log($"\t{gameObject.name}");
            Undo.SetCurrentGroupName($"Replace Components");

            // まず元のコンポーネントをゲームオブジェクトごと複製
            var cloneObject = Instantiate(gameObject, gameObject.transform.parent);
            var cloneComponent = cloneObject.GetComponent(fromType);

            // なぜかtargetGraphicはコピーしてくれないから手動で移動
            if (typeof(Selectable).IsAssignableFrom(fromType))
            {
                var targetGraphic = gameObject.GetComponent<Selectable>().targetGraphic;
                ((Selectable)cloneComponent).targetGraphic = targetGraphic;
            }

            // 元のコンポーネントは削除し、置き換え先のコンポーネントをアタッチ
            Undo.DestroyObjectImmediate(component);
            var newComponent = Undo.AddComponent(gameObject, toType);

            // 複製しておいた元コンポーネントから新しいコンポーネントにデータをコピー
            EditorUtility.CopySerialized(cloneComponent, newComponent);

            // 複製は不要になったので削除する
            DestroyImmediate(cloneObject);
        }
    }
}