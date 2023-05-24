using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hermes.UI.UIManagerParts;
using Mobcast.Coffee.Transition;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// Screen, Dialog, SubSceneをロード、アンロードするクラス
    /// </summary>
    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>現在のView</summary>
        [SerializeField] ViewBase currentView;
        /// <summary>現在のView</summary>
        public ViewBase CurrentView { get { return currentView; } private set { currentView = value; } }
        /// <summary>バリア</summary>
        [SerializeField] GameObject barrier;

        /// <summary>UIManagerScreen</summary>
        [SerializeField] UIManagerScreen uiScreen = new UIManagerScreen();
        /// <summary>UIManagerDialog</summary>
        [SerializeField] UIManagerDialog uiDialog = new UIManagerDialog();
        /// <summary>UIManagerSubScene</summary>
        [SerializeField] UIManagerSubScene uiSubScene = new UIManagerSubScene();

        /// <summary>遷移StackName</summary>
        [SerializeField] List<string> stackName = new List<string>();
        /// <summary>遷移StackType</summary>
        Stack<Type> stackType = new Stack<Type>();
        /// <summary>遷移StackOptions</summary>
        Stack<object> stackOptions = new Stack<object>();

        /// <summary>LocalizeKey ゲーム終了タイトルテキスト</summary>
        [SerializeField] string quitTitleKey = "QUIT_TITLE";
        /// <summary>LocalizeKey ゲーム終了本文テキスト</summary>
        [SerializeField] string quitBodyKey = "QUIT_BODY";

        /// <summary>
        /// 駆動
        /// </summary>
        async void Update()
        {
            // Backキー押下
            if (!barrier.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            {
                await BackAsync();
            }
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(Viewタイプから呼び出し)
        /// <para>別アセンブリのViewはロード出来ない</para>
        /// <para>別アセンブリの場合、完全修飾で指定しなければいけない</para>
        /// <para>例) Hermes.UI.Sample.UISampleScene, Assembly-CSharp</para>
        /// </summary>
        /// <param name="viewType">Viewタイプ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(string viewType, object options = null, CancellationToken cancellationToken = default)
        {
            await LoadAsync(viewType, false, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(Viewタイプから呼び出し)
        /// <para>別アセンブリのViewはロード出来ない</para>
        /// <para>別アセンブリの場合、完全修飾で指定しなければいけない</para>
        /// <para>例) Hermes.UI.Sample.UISampleScene, Assembly-CSharp</para>
        /// </summary>
        /// <param name="viewType">Viewタイプ</param>
        /// <param name="resumeDialog">遷移先のシーンが存在している時に出ていたダイアログをロードするフラグ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(string viewType, bool resumeDialog, object options = null, CancellationToken cancellationToken = default)
        {
            var type = Type.GetType(viewType);
            await LoadAsync(type.Name, type, resumeDialog, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(Viewタイプから呼び出し)
        /// </summary>
        /// <param name="viewType">Viewタイプ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(Type viewType, object options = null, CancellationToken cancellationToken = default)
        {
            await LoadAsync(viewType.Name, viewType, false, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(Viewタイプから呼び出し)
        /// </summary>
        /// <param name="viewType">View</param>
        /// <param name="resumeDialog">遷移先のシーンが存在している時に出ていたダイアログをロードするフラグ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(Type viewType, bool resumeDialog, object options = null, CancellationToken cancellationToken = default)
        {
            await LoadAsync(viewType.Name, viewType, resumeDialog, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(View名から呼び出し)
        /// </summary>
        /// <param name="viewName">View名</param>
        /// <param name="viewType">View</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(string viewName, Type viewType, object options = null, CancellationToken cancellationToken = default)
        {
            await LoadAsync(viewName, viewType, false, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(View名から呼び出し)
        /// </summary>
        /// <param name="viewName">View名</param>
        /// <param name="viewType">View</param>
        /// <param name="resumeDialog">遷移先のシーンが存在している時に出ていたダイアログをロードするフラグ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(string viewName, Type viewType, bool resumeDialog, object options = null, CancellationToken cancellationToken = default)
        {
            var methods = this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == "LoadAsync" && x.IsGenericMethod);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 4 &&
                    parameters[0].ParameterType == typeof(string) &&
                    parameters[1].ParameterType == typeof(bool) &&
                    parameters[2].ParameterType == typeof(object) &&
                    parameters[3].ParameterType == typeof(CancellationToken))
                {
                    var uniTask = method
                        .MakeGenericMethod(viewType)
                        .Invoke(this, new object[] { viewName, resumeDialog, options, cancellationToken });
                    await (UniTask)uniTask;
                    return;
                }
            }
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync<T>(object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            await LoadAsync<T>(typeof(T).Name, false, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="viewName">View名</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync<T>(string viewName, object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            await LoadAsync<T>(viewName, false, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="resumeDialog">遷移先のシーンが存在している時に出ていたダイアログをロードするフラグ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync<T>(bool resumeDialog, object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            await LoadAsync<T>(typeof(T).Name, resumeDialog, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="viewName">View名</param>
        /// <param name="resumeDialog">遷移先のシーンが存在している時に出ていたダイアログをロードするフラグ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync<T>(string viewName, bool resumeDialog, object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            var type = typeof(T);

            // Screenがない時にDialogだったらエラー
            if (uiScreen.CurrentScreen == null && type.IsSubclassOf(typeof(Dialog)))
            {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                Debug.LogError("The scene does not yet exist, but it is trying to load the dialog.");
#endif
                return;
            }

            // バリアON
            barrier.SetActive(true);

            // 再表示ダイアログリスト
            var resumeDialogList = new List<KeyValuePair<string, KeyValuePair<Type, object>>>();

            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {

                // 既にシーンが存在した時
                if (stackType.Contains(type))
                {
                    // 遷移先のシーンが存在している時に出ていたダイアログをロード
                    if (resumeDialog)
                    {
                        var stackType = new Stack<Type>();
                        var stackName = new Stack<string>();
                        var stackOptions = new Stack<object>();
                        var count = this.stackType.Count;
                        for (int i = 0; i < count; i++)
                        {
                            var t = this.stackType.Pop();
                            var n = this.stackName.Pop();
                            var o = this.stackOptions.Pop();
                            if (t == type)
                                break;
                            stackType.Push(t);
                            stackName.Push(n);
                            stackOptions.Push(o);
                        }
                        count = stackType.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (stackType.Peek().IsSubclassOf(typeof(Screen)))
                                break;
                            resumeDialogList.Add(new KeyValuePair<string, KeyValuePair<Type, object>>(stackName.Pop(), new KeyValuePair<Type, object>(stackType.Pop(), stackOptions.Pop())));
                        }
                    }
                    // Stackから外していく
                    else
                    {
                        var count = stackType.Count;
                        for (int i = 0; i < count; i++)
                        {
                            stackName.Pop();
                            var t = stackType.Pop();
                            stackOptions.Pop();
                            if (t == type)
                                break;
                        }
                    }
                }
                // まだCurrentScreenがなかったらUnloadはしない
                if (uiScreen.CurrentScreen != null)
                {
                    await uiScreen.UnloadAsync(cancellationToken);
                    await uiDialog.AllUnloadAsync(cancellationToken);
                }
                // シーンロード
                CurrentView = await uiScreen.LoadAsync<T>(viewName, options, cancellationToken);
            }
            // Dialog
            else
            {
                CurrentView = await uiDialog.LoadAsync<T>(viewName, CurrentView.gameObject, options, cancellationToken);
            }

            StackPush(viewName, type, options);

            // ダイアログがあったらロード
            foreach (var dialog in resumeDialogList)
                await LoadAsync(dialog.Key, dialog.Value.Key, false, dialog.Value.Value, cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// シーン(Screen)をリロードするAsyncメソッド
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask ReloadSceneAsync(CancellationToken cancellationToken = default)
        {
            // バリアON
            barrier.SetActive(true);
            // 現在の最新がダイアログだったら削除する
            if (CurrentView is Dialog)
            {
                var count = stackType.Count;
                for (int i = 0; i < count; i++)
                {
                    var t = stackType.Peek();
                    if (t.IsSubclassOf(typeof(Screen)))
                        break;
                    StackPop();
                }
            }
            var type = stackType.Peek(); ;
            var name = stackName.Peek();
            // まだCurrentScreenがなかったらUnloadはしない
            if (uiScreen.CurrentScreen != null)
            {
                await uiScreen.UnloadAsync(cancellationToken);
                await uiDialog.AllUnloadAsync(cancellationToken);
            }
            // シーンロード
            CurrentView = await uiScreen.LoadAsync(name, stackType.Peek(), stackOptions.Peek(), cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// 前画面表示
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask BackAsync(CancellationToken cancellationToken = default)
        {
            barrier.SetActive(true);
            if (CurrentView != null && !CurrentView.IsBack)
            {
                // 前の画面に戻れない時の処理
                await CurrentView.ActionInsteadOfBack();
                barrier.SetActive(false);
                return;
            }
            if (stackType.Count > 1)
            {
                var viewName = stackName.Pop();
                stackType.Pop();
                stackOptions.Pop();
                await BackProcess(viewName, CurrentView is Screen, cancellationToken);
            }
            else
            {
                // スタックが無かったらダイアログ表示
                await LoadAsync<CommonDialog>(new CommonDialog.Options()
                {
                    Title = quitTitleKey,
                    Body = quitBodyKey,
                    ButtonType = CommonDialog.eButtonType.YesOrNo,
                    OnClickAction = () =>
                    {
                        // ゲーム終了
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
                    }
                });
            }

            barrier.SetActive(false);
        }

        /// <summary>
        /// 前画面表示処理
        /// </summary>
        /// <param name="viewName">ViewName</param>
        /// <param name="isScreen">Screenならtrue</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask BackProcess(string viewName, bool isScreen, CancellationToken cancellationToken)
        {
            var name = stackName.Peek();
            var type = stackType.Peek();
            var options = stackOptions.Peek();
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            Debug.Log($"BackProcess : viewName = {viewName} : name = {name}");
#endif
            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                if (isScreen)
                {
                    await uiScreen.UnloadAsync(cancellationToken);
                    CurrentView = await uiScreen.LoadAsync(name, type, options, cancellationToken);
                }
                else
                {
                    await uiDialog.UnloadAsync(true, cancellationToken);
                    CurrentView = (ViewBase)GameObject.Find(name).GetComponent(type);
                }
            }
            // Dialog
            else
            {
                if (isScreen)
                {
                    // stackを一時的に抜いておく
                    StackPop();

                    await BackProcess(viewName, true, cancellationToken);

                    // stackを戻す
                    StackPush(name, type, options);

                    // Instantiate
                    CurrentView = await uiDialog.LoadAsync(name, type, CurrentView.gameObject, options, cancellationToken);
                }
                else
                {
                    await uiDialog.UnloadAsync(true, cancellationToken);
                    CurrentView = (ViewBase)GameObject.Find(name).GetComponent(type);
                }
            }
        }

        /// <summary>
        /// SubSceneのLoadAsync
        /// </summary>
        /// <typeparam name="T">SubScene</typeparam>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask SubSceneLoadAsync<T>(object options = null, CancellationToken cancellationToken = default) where T : SubScene
        {
            await SubSceneLoadAsync<T>(typeof(T).Name, options, cancellationToken);
        }

        /// <summary>
        /// SubSceneのLoadAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sceneName">シーン名</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask<SubScene></returns>
        public async UniTask<SubScene> SubSceneLoadAsync<T>(string sceneName, object options = null, CancellationToken cancellationToken = default) where T : SubScene
        {
            // バリアON
            barrier.SetActive(true);

            var scene = await uiSubScene.LoadAsync<T>(sceneName, options, cancellationToken);
            await UniTask.WaitUntil(() => scene.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
            return scene;
        }

        /// <summary>
        /// SubSceneのUnloadAsync
        /// </summary>
        /// <typeparam name="T">SubScene</typeparam>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask SubSceneUnloadAsync<T>(CancellationToken cancellationToken = default) where T : SubScene
        {
            await SubSceneUnloadAsync(typeof(T).Name, cancellationToken);
        }

        /// <summary>
        /// SubSceneのUnloadAsync
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask SubSceneUnloadAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            // バリアON
            barrier.SetActive(true);

            await uiSubScene.UnloadAsync(sceneName, cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// SubSceneが既に呼び出されているか
        /// </summary>
        /// <typeparam name="T">SubScene</typeparam>
        /// <returns>SubSceneを持っていたらtrue, 持っていなかったらfalse</returns>
        public bool HasSubScene<T>() where T : SubScene
        {
            foreach (var subScene in uiSubScene.SubSceneList)
            {
                if (typeof(T) == subScene.GetType())
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 全てアンロード
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask AllUnloadAsync(CancellationToken cancellationToken = default)
        {
            // バリアON
            barrier.SetActive(true);

            // サブシーン
            await uiSubScene.AllUnloadAsync(cancellationToken);

            // ダイアログ
            await uiDialog.AllUnloadAsync(cancellationToken);

            // シーン
            await uiScreen.UnloadAsync(cancellationToken);

            CurrentView = null;

            ClearStack();

            // バリアOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// Stack pop
        /// </summary>
        void StackPop()
        {
            stackName.Pop();
            stackType.Pop();
            stackOptions.Pop();
        }

        /// <summary>
        /// Stack push
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        void StackPush(string name, Type type, object options)
        {
            stackName.Push(name);
            stackType.Push(type);
            stackOptions.Push(options);
        }

        /// <summary>
        /// スタッククリア
        /// </summary>
        public void ClearStack()
        {
            stackName.Clear();
            stackType.Clear();
            stackOptions.Clear();
        }

        /// <summary>
        /// 一つ残してスタックをクリアする
        /// </summary>
        public void ClearStackLeaveOne()
        {
            // 一つ残してクリアする
            var count = stackType.Count - 1;
            for (int i = 0; i < count; i++)
            {
                stackName.Pop();
                stackType.Pop();
                stackOptions.Pop();
            }
        }

        /// <summary>
        /// 指定したシーンまでのスタックをクリアする
        /// </summary>
        public void ClearStackSpecifiedView<T>() where T : ViewBase
        {
            var type = typeof(T);
            var count = stackType.Count;
            for (int i = 0; i < count; i++)
            {
                if (type == stackType.Peek())
                    return;
                stackName.Pop();
                stackType.Pop();
                stackOptions.Pop();
            }
        }

        /// <summary>
        /// シーン遷移アニメーション設定
        /// </summary>
        /// <param name="transition">遷移アニメーション</param>
        public void SetSceneTransition(UITransition transition)
        {
            uiScreen.SetSceneTransition(transition);
        }
    }
}