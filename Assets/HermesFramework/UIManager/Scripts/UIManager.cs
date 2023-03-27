using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hermes.Asset;
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
                // 現在の最新がダイアログだったら削除する
                if (CurrentView is Dialog)
                {
                    // TODO:現在ダイアログから他のシーンに遷移する時にダイアログをDestroyしてからシーンをUnloadしてるけど、不自然なためシーンの中にダイアログを入れ込む形にした方が良さそう
                    // なぜかダイアログから他のシーンに遷移する時に画面がチラつくからBGをOFFにする
                    dialogBG.SetActive(false);
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
                        CurrentView = (ViewBase)FindObjectOfType(t);
                        OnUnloadDialog(CurrentView, false, cancellationToken).Forget();
                    }
                    count = stackType.Count;
                    for (int i = 0; i < count; i++)
                    {
                        StackPush(stackType.Pop(), stackOptions.Pop());
                    }
                    // ダイアログが全て消えるまで待機
                    for (int i = 0; i < this.stackType.Count; i++)
                    {
                        stackName.Pop();
                        stackType.Push(this.stackType.Pop());
                        stackOptions.Push(this.stackOptions.Pop());
                        var t = stackType.Peek();
                        if (t.IsSubclassOf(typeof(Screen)))
                            break;
                        CurrentView = (ViewBase)FindObjectOfType(t);
                        await UniTask.WaitUntil(() => CurrentView == null, cancellationToken: cancellationToken);
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
                    await OnUnloadScreen(CurrentScene, cancellationToken);
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
            await CurrentView.OnEnableAnimation();
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
            // 現在の最新がダイアログだったら削除する
            if (CurrentView is Dialog)
            {
                // なぜかダイアログから他のシーンに遷移する時に画面がチラつくからBGをOFFにする
                dialogBG.SetActive(false);
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
                    CurrentView = (ViewBase)FindObjectOfType(t);
                    OnUnloadDialog(CurrentView, false, cancellationToken).Forget();
                }
                count = stackType.Count;
                for (int i = 0; i < count; i++)
                {
                    StackPush(stackType.Pop(), stackOptions.Pop());
                }
                // ダイアログが全て消えるまで待機
                for (int i = 0; i < this.stackType.Count; i++)
                {
                    stackName.Pop();
                    stackType.Push(this.stackType.Pop());
                    stackOptions.Push(this.stackOptions.Pop());
                    var t = stackType.Peek();
                    if (t.IsSubclassOf(typeof(Screen)))
                        break;
                    CurrentView = (ViewBase)FindObjectOfType(t);
                    await UniTask.WaitUntil(() => CurrentView == null, cancellationToken: cancellationToken);
                }
                count = stackType.Count;
                for (int i = 0; i < count; i++)
                {
                    if (!stackType.Peek().IsSubclassOf(typeof(Screen)))
                        continue;
                    StackPush(stackType.Pop(), stackOptions.Pop());
                }
            }
            Type type = CurrentScene.GetType();
            // まだCurrentSceneがなかったらUnloadはしない
            if (CurrentScene != null)
            {
                await OnUnloadScreen(CurrentScene, cancellationToken);
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
            await CurrentView.OnEnableAnimation();
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
                // TODO:前の画面に戻れない時の処理

            }
            if (stackType.Count > 1)
            {
                StackPop();
                await BackProcess(CurrentView is Screen, cancellationToken);
            }
            else
            {
                // TODO:スタックが無かったらゲーム終了
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
            await CurrentView.OnEnableAnimation();
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
            await viewBase.OnDisableAnimation();
            await viewBase.OnUnload();
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
                await viewBase.OnDisableAnimation();
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
            await screen.OnEnableAnimation();
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
                    await SubSceneList[i].OnDisableAnimation();
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
    }
}