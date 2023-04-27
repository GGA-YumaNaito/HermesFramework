using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hermes.Asset;
using Mobcast.Coffee.Transition;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

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
        /// <summary>現在のScene</summary>
        [SerializeField] Screen currentScene;
        /// <summary>現在のScene</summary>
        public Screen CurrentScene { get { return currentScene; } private set { currentScene = value; } }
        /// <summary>現在のScene名</summary>
        [SerializeField] string currentSceneName;
        /// <summary>バリア</summary>
        [SerializeField] GameObject barrier;
        /// <summary>ダイアログ用BG</summary>
        [SerializeField] GameObject dialogBG;
        /// <summary>ダイアログRoot</summary>
        [SerializeField] Transform dialogRoot;
        /// <summary>シーン遷移アニメーション</summary>
        [SerializeField] UITransition sceneTransition;

        /// <summary>SubSceneList</summary>
        [SerializeField] List<SubScene> subSceneList = new List<SubScene>();
        /// <summary>SubSceneList</summary>
        public List<SubScene> SubSceneList { get { return subSceneList; } private set { subSceneList = value; } }
        /// <summary>SubSceneInstanceList</summary>
        List<KeyValuePair<string, SceneInstance>> subSceneInstanceList = new List<KeyValuePair<string, SceneInstance>>();

        /// <summary>遷移StackName</summary>
        [SerializeField] List<string> stackName = new List<string>();
        /// <summary>遷移StackType</summary>
        Stack<Type> stackType = new Stack<Type>();
        /// <summary>遷移StackOptions</summary>
        Stack<object> stackOptions = new Stack<object>();

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
            if (CurrentScene == null && type.IsSubclassOf(typeof(Dialog)))
            {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                Debug.LogError("The scene does not yet exist, but it is trying to load the dialog.");
#endif
                return;
            }

            // バリアON
            barrier.SetActive(true);

            // ロードダイアログリスト
            var loadDialogList = new List<KeyValuePair<string, KeyValuePair<Type, object>>>();

            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                var unloadDialogList = new List<ViewBase>();
                // 現在の最新がダイアログだったら削除する
                if (CurrentView is Dialog)
                {
                    var stackType = new Stack<Type>();
                    var stackName = new Stack<string>();
                    var count = this.stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        stackType.Push(this.stackType.Pop());
                        stackName.Push(this.stackName.Pop());
                        var t = stackType.Peek();
                        if (t.IsSubclassOf(typeof(Screen)))
                            break;
                        unloadDialogList.Add((ViewBase)dialogRoot.Find(stackName.Peek()).GetComponent(t));
                    }
                    count = stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        this.stackType.Push(stackType.Pop());
                        this.stackName.Push(stackName.Pop());
                    }
                }
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
                            loadDialogList.Add(new KeyValuePair<string, KeyValuePair<Type, object>>(stackName.Pop(), new KeyValuePair<Type, object>(stackType.Pop(), stackOptions.Pop())));
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
                // まだCurrentSceneがなかったらUnloadはしない
                if (CurrentScene != null)
                {
                    await OnUnloadScreen(currentSceneName, CurrentScene, unloadDialogList, cancellationToken);
                }
                // シーンロード
                await SceneManager.LoadSceneAsync(viewName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
                
                CurrentScene = GameObject.Find(viewName).GetComponent<T>() as Screen;
                CurrentView = CurrentScene;
                currentSceneName = viewName;
            }
            // Dialog
            else
            {
                dialogBG.SetActive(true);

                // ロードアセット
                var dialog = await AssetManager.LoadAsync<GameObject>(viewName, CurrentView.gameObject, cancellationToken);

                // DialogBGの位置を上げる
                dialogBG.transform.SetAsLastSibling();

                // Instantiate
                CurrentView = Instantiate(dialog, dialogRoot).GetComponent<T>();
                CurrentView.name = viewName;
            }

            StackPush(viewName, type, options);

            if (CurrentView == null)
                throw new Exception($"{viewName} is Null");

            // Initialize & Load
            CurrentView.Initialize();
            await CurrentView.OnLoad(options);
            if (CurrentView is Screen)
                // シーン遷移退出アニメーション
                await OnDisableSceneTransition();
            await CurrentView.OnDisplay();
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

            // ダイアログがあったらロード
            foreach (var dialog in loadDialogList)
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
            // Screen
            var dialogList = new List<ViewBase>();
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
                    dialogList.Add((ViewBase)FindObjectOfType(t));
                }
            }
            var type = CurrentScene.GetType();
            var name = stackName.Peek();
            // まだCurrentSceneがなかったらUnloadはしない
            if (CurrentScene != null)
            {
                await OnUnloadScreen(name, CurrentScene, dialogList, cancellationToken);
            }
            // シーンロード
            await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);

            CurrentScene = GameObject.Find(name).GetComponent(type) as Screen;
            CurrentView = CurrentScene;

            if (CurrentView == null)
                throw new Exception($"{name} is Null");

            // Initialize & Load
            CurrentView.Initialize();
            await CurrentView.OnLoad(stackOptions.Peek());
            // シーン遷移退出アニメーション
            await OnDisableSceneTransition();
            await CurrentView.OnDisplay();
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

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
                // スタックが無かったらゲーム終了
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            // ダイアログが無かったらバリアをOFFにする
            dialogBG.SetActive(CurrentView is Dialog);

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
            Debug.Log($"BackProcess : viewName = {viewName} : name = {name}");
            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                if (isScreen)
                {
                    await OnUnloadScreen(viewName, CurrentScene, cancellationToken);
                    await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
                    CurrentScene = GameObject.Find(name).GetComponent(type) as Screen;
                    CurrentView = CurrentScene;
                    currentSceneName = name;
                }
                else
                {
                    await OnUnloadDialog(CurrentView, true, cancellationToken);
                    CurrentView = (ViewBase)FindObjectOfType(type);
                    return;
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

                    // ロードアセット
                    var dialog = await AssetManager.LoadAsync<GameObject>(name, CurrentView.gameObject, cancellationToken);

                    // DialogBGをON
                    dialogBG.SetActive(true);

                    // DialogBGの位置を上げる
                    dialogBG.transform.SetAsLastSibling();

                    // Instantiate
                    CurrentView = (ViewBase)Instantiate(dialog, dialogRoot).GetComponent(type);
                    CurrentView.name = name;
                }
                else
                {
                    await OnUnloadDialog(CurrentView, true, cancellationToken);
                    CurrentView = (ViewBase)FindObjectOfType(type);
                    return;
                }
            }
            if (CurrentView == null)
                throw new Exception($"{name} is Null");

            CurrentView.Initialize();
            await CurrentView.OnLoad(options);
            // シーン遷移退出アニメーション
            if (CurrentView is Screen)
                await OnDisableSceneTransition();
            await CurrentView.OnDisplay();
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// スクリーン削除
        /// </summary>
        /// <param name="viewName">ViewName</param>
        /// <param name="viewBase">ViewBase</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadScreen(string viewName, ViewBase viewBase, CancellationToken cancellationToken)
        {
            await viewBase.OnEnd();
            // シーン遷移出現アニメーション
            await OnEnableSceneTransition();
            await viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            Debug.Log($"OnUnloadScreen : {viewName}");
            await SceneManager.UnloadSceneAsync(viewName).ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// スクリーン削除
        /// </summary>
        /// <param name="viewName">ViewName</param>
        /// <param name="viewBase">ViewBase</param>
        /// <param name="dialogList">DialogList</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadScreen(string viewName, ViewBase viewBase, List<ViewBase> dialogList, CancellationToken cancellationToken)
        {
            await viewBase.OnEnd();
            // シーン遷移出現アニメーション
            await OnEnableSceneTransition();
            await viewBase.OnUnload();
            if (dialogList != null && dialogList.Count > 0)
            {
                foreach (var dialog in dialogList)
                {
                    await dialog.OnUnload();
                    Destroy(dialog.gameObject);
                }
                foreach (var dialog in dialogList)
                {
                    await UniTask.WaitUntil(() => dialog == null, cancellationToken: cancellationToken);
                }
                dialogBG.SetActive(false);
            }
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            Debug.Log($"OnUnloadScreen : {viewName}");
            await SceneManager.UnloadSceneAsync(viewName).ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// ダイアログ削除
        /// </summary>
        /// <param name="viewBase">ViewBase</param>
        /// <param name="isBack">前画面に戻る処理ならtrue</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadDialog(ViewBase viewBase, bool isBack, CancellationToken cancellationToken = default)
        {
            if (isBack)
                await viewBase.OnEnd();
            await viewBase.OnUnload();
            if (isBack)
                await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            Destroy(viewBase.gameObject);
            if (isBack)
            {
                // DialogBGの位置を下げる
                var dialogBGTransform = dialogBG.transform;
                dialogBGTransform.SetSiblingIndex(dialogBGTransform.GetSiblingIndex() - 1);
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
        /// <returns>UniTask</returns>
        public async UniTask SubSceneLoadAsync<T>(string sceneName, object options = null, CancellationToken cancellationToken = default) where T : SubScene
        {
            // バリアON
            barrier.SetActive(true);

            var type = typeof(T);
            // シーンロード
            var instance = await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
            var screen = GameObject.Find(sceneName).GetComponent(type) as SubScene;

            if (screen == null)
            {
                barrier.SetActive(false);
                throw new Exception($"{sceneName} is Null");
            }

            SubSceneList.Add(screen);
            subSceneInstanceList.Add(new KeyValuePair<string, SceneInstance>(sceneName, instance));

            // Initialize & Load
            screen.Initialize();
            await screen.OnLoad(options);
            await screen.OnDisplay();
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
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

            for (int i = 0; i < SubSceneList.Count; i++)
            {
                if (subSceneInstanceList[i].Key == sceneName)
                {
                    await SubSceneList[i].OnEnd();
                    await SubSceneList[i].OnUnload();
                    await UniTask.WaitUntil(() => SubSceneList[i].Status.Value == eStatus.End, cancellationToken: cancellationToken);

                    // シーンアンロード
                    await Addressables.UnloadSceneAsync(subSceneInstanceList[i].Value).ToUniTask(cancellationToken: cancellationToken);
                    SubSceneList.RemoveAt(i);
                    subSceneInstanceList.RemoveAt(i);
                    break;
                }
            }

            // バリアOFF
            barrier.SetActive(false);
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
            for (int i = 0; i < SubSceneList.Count; i++)
            {
                await SubSceneList[i].OnUnload();

                // シーンアンロード
                await Addressables.UnloadSceneAsync(subSceneInstanceList[i].Value).ToUniTask(cancellationToken: cancellationToken);
                SubSceneList.RemoveAt(i);
                subSceneInstanceList.RemoveAt(i);
            }

            // ダイアログ
            var count = this.stackType.Count;
            for (int i = 0; i < count; i++)
            {
                var t = stackType.Peek();
                if (t.IsSubclassOf(typeof(Screen)))
                    break;
                var n = stackName.Peek();
                StackPop();
                OnUnloadDialog((ViewBase)dialogRoot.Find(n).GetComponent(t), false, cancellationToken).Forget();
            }
            dialogBG.SetActive(false);

            // シーン
            await OnUnloadScreen(stackName.Peek(), CurrentScene, cancellationToken);

            CurrentScene = null;
            CurrentView = null;
            currentSceneName = null;

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
        /// シーン遷移アニメーション設定
        /// </summary>
        /// <param name="transition">遷移アニメーション</param>
        public void SetSceneTransition(UITransition transition)
        {
            this.sceneTransition = transition;
        }

        /// <summary>
        /// シーン遷移出現アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        async UniTask OnEnableSceneTransition()
        {
            if (sceneTransition)
            {
                sceneTransition.gameObject.SetActive(true);
                await UniTask.WaitUntil(() => !sceneTransition.isShow);
                sceneTransition.Show();
                await UniTask.WaitUntil(() => !sceneTransition.isPlaying);
            }
        }

        /// <summary>
        /// シーン遷移退出アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        protected virtual async UniTask OnDisableSceneTransition()
        {
            if (sceneTransition)
            {
                sceneTransition.Hide();
                await UniTask.WaitUntil(() => !sceneTransition.isPlaying);
                sceneTransition.gameObject.SetActive(false);
            }
        }
    }
}