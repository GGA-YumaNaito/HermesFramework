using System;
using System.Collections;
using System.Collections.Generic;
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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                await BackAsync();
            }
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync(View名から呼び出し)
        /// </summary>
        /// <param name="viewName">View名</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync(string viewName, object options = null, CancellationToken cancellationToken = default)
        {
            var viewType = Type.GetType(viewName);
            object uniTask = this.GetType()
                .GetMethod(
                    "LoadAsync",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(object), typeof(CancellationToken) },
                    null
                )
                .MakeGenericMethod(viewType)
                .Invoke(this, new object[] { options, cancellationToken });
            await (UniTask)uniTask;
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
            var type = typeof(T);
            // 同じ画面なら表示しない
            if (CurrentScene != null && type == CurrentScene.GetType())
                return;

            // バリアON
            barrier.SetActive(true);
            // 既に存在していたらStackから外していく
            Action<Type> StackPopAction = type =>
            {
                if (!stackType.Contains(type))
                    return;
                var count = stackType.Count;
                for (int i = 0; i < count; i++)
                {
                    stackName.Pop();
                    var t = stackType.Pop();
                    stackOptions.Pop();
                    if (t == type)
                        break;
                }
            };

            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                var dialogList = new List<ViewBase>();
                // 現在の最新がダイアログだったら削除する
                if (CurrentView is Dialog)
                {
                    var stackType = new Stack<Type>();
                    var stackOptions = new Stack<object>();
                    var count = this.stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        stackName.Pop();
                        stackType.Push(this.stackType.Pop());
                        stackOptions.Push(this.stackOptions.Pop());
                        var t = stackType.Peek();
                        if (t.IsSubclassOf(typeof(Screen)))
                            break;
                        dialogList.Add((ViewBase)FindObjectOfType(t));
                    }
                    count = stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        StackPush(stackType.Pop(), stackOptions.Pop());
                    }
                }
                // 既にシーンが存在したら
                StackPopAction(type);
                // まだCurrentSceneがなかったらUnloadはしない
                if (CurrentScene != null)
                {
                    await OnUnloadScreen(CurrentScene, dialogList, cancellationToken);
                }
                // シーンロード
                await SceneManager.LoadSceneAsync(type.Name, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
                
                CurrentScene = FindObjectOfType<T>() as Screen;
                CurrentView = CurrentScene;
            }
            // Dialog
            else
            {
                dialogBG.SetActive(true);
                // 既にダイアログが存在したら
                //StackPopAction(type); // TODO: 今は外しておく

                // ロードアセット
                var dialog = await AssetManager.LoadAsync<GameObject>(type.Name, CurrentView.gameObject, cancellationToken);

                // DialogBGの位置を上げる
                dialogBG.transform.SetAsLastSibling();

                // Instantiate
                CurrentView = Instantiate(dialog, dialogRoot).GetComponent<T>();
            }

            StackPush(type, options);

            if (CurrentView == null)
                throw new Exception($"{typeof(T).Name} is Null");

            // Initialize & Load
            CurrentView.Initialize();
            await CurrentView.OnLoad(options);
            if (CurrentView is Screen)
            {
                // シーン遷移退出アニメーション
                await OnDisableSceneTransition();
            }
            await CurrentView.OnDisplay();
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

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
            Type type = CurrentScene.GetType();
            // まだCurrentSceneがなかったらUnloadはしない
            if (CurrentScene != null)
            {
                await OnUnloadScreen(CurrentScene, dialogList, cancellationToken);
            }
            // シーンロード
            await SceneManager.LoadSceneAsync(type.Name, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);

            CurrentScene = FindObjectOfType(type) as Screen;
            CurrentView = CurrentScene;

            if (CurrentView == null)
                throw new Exception($"{type.Name} is Null");

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
                StackPop();
                await BackProcess(CurrentView is Screen, cancellationToken);
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
        /// <param name="isScreen">Screenならtrue</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask BackProcess(bool isScreen, CancellationToken cancellationToken)
        {
            var name = stackName.Peek();
            var type = stackType.Peek();
            var options = stackOptions.Peek();
            // Screen
            if (type.IsSubclassOf(typeof(Screen)))
            {
                if (isScreen)
                {
                    await OnUnloadScreen(CurrentScene, cancellationToken);
                    await SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
                    CurrentScene = FindObjectOfType(type) as Screen;
                    CurrentView = CurrentScene;
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

                    await BackProcess(true, cancellationToken);

                    // stackを戻す
                    StackPush(type, options);

                    // ロードアセット
                    var dialog = await AssetManager.LoadAsync<GameObject>(name, CurrentView.gameObject, cancellationToken);

                    // DialogBGをON
                    dialogBG.SetActive(true);

                    // DialogBGの位置を上げる
                    dialogBG.transform.SetAsLastSibling();

                    // Instantiate
                    CurrentView = (ViewBase)Instantiate(dialog, dialogRoot).GetComponent(type);
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
        /// <param name="viewBase">ViewBase</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadScreen(ViewBase viewBase, CancellationToken cancellationToken)
        {
            await viewBase.OnEnd();
            // シーン遷移出現アニメーション
            await OnEnableSceneTransition();
            await viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            await SceneManager.UnloadSceneAsync(viewBase.GetType().Name).ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// スクリーン削除
        /// </summary>
        /// <param name="viewBase">ViewBase</param>
        /// <param name="dialogList">DialogList</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadScreen(ViewBase viewBase, List<ViewBase> dialogList, CancellationToken cancellationToken)
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
            await SceneManager.UnloadSceneAsync(viewBase.GetType().Name).ToUniTask(cancellationToken: cancellationToken);
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
            var screen = FindObjectOfType(type) as SubScene;

            if (screen == null)
                throw new Exception($"{typeof(T).Name} is Null");

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
                    return;
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
                stackName.Pop();
                stackOptions.Pop();
                var t = stackType.Pop();
                if (t.IsSubclassOf(typeof(Screen)))
                    break;
                OnUnloadDialog((ViewBase)FindObjectOfType(t), false, cancellationToken).Forget();
            }
            dialogBG.SetActive(false);

            // シーン
            await OnUnloadScreen(CurrentScene, cancellationToken);

            CurrentScene = null;
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
        /// <param name="type"></param>
        /// <param name="options"></param>
        void StackPush(Type type, object options)
        {
            stackName.Push(type.Name);
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