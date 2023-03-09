using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Hermes.UI
{
    /// <summary>
    /// UIManager
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
        /// <summary>現在のView</summary>
        public Screen CurrentScene { get { return currentScene; } private set { currentScene = value; } }
        /// <summary>バリア</summary>
        [SerializeField] GameObject barrier;
        /// <summary>ダイアログ用BG</summary>
        [SerializeField] GameObject dialogBG;
        /// <summary>ダイアログRoot</summary>
        [SerializeField] Transform dialogRoot;

        /// <summary>遷移StackName</summary>
        [SerializeField] List<string> stackName = new List<string>();
        /// <summary>遷移StackType</summary>
        Stack<Type> stackType = new Stack<Type>();
        /// <summary>遷移StackOptions</summary>
        Stack<object> stackOptions = new Stack<object>();

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="options"></param>
        /// <param name="cancellationToken"></param>
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
                        await OnUnloadDialog(CurrentView, cancellationToken);
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
                var handle = Addressables.LoadAssetAsync<GameObject>(type.Name);
                await handle.ToUniTask(cancellationToken: cancellationToken);
                var dialog = handle.Result;

                // Instantiate
                CurrentView = Instantiate(dialog, dialogRoot).GetComponent<T>();
                // AsyncOperationHandleをDialogに渡す
                ((Dialog)CurrentView).SetAsyncOperationHandle(handle);
            }

            StackPush(type, options);

            if (CurrentView == null)
                throw new Exception($"{typeof(T).Name} is Null");

            // Initialize & Load
            CurrentView.Initialize();
            CurrentView.OnEnableAnimation();
            CurrentView.OnLoad(options);
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

            // バリアOFF
            barrier.SetActive(false);
        }

        /// <summary>
        /// 前画面表示
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>UniTask</returns>
        public async UniTask BackAsync(CancellationToken cancellationToken)
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
        /// <param name="isScreen"></param>
        /// <param name="cancellationToken"></param>
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
                    await OnUnloadDialog(CurrentView, cancellationToken);
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
                    var handle = Addressables.LoadAssetAsync<GameObject>(name);
                    await handle.ToUniTask(cancellationToken: cancellationToken);
                    var dialog = handle.Result;

                    // Instantiate
                    CurrentView = (ViewBase)Instantiate(dialog, dialogRoot).GetComponent(type);
                    // AsyncOperationHandleをDialogに渡す
                    ((Dialog)CurrentView).SetAsyncOperationHandle(handle);
                }
                else
                {
                    await OnUnloadDialog(CurrentView, cancellationToken);
                    CurrentView = (ViewBase)FindObjectOfType(type);
                    return;
                }
            }
            if (CurrentView == null)
                throw new Exception($"{name} is Null");

            CurrentView.Initialize();
            CurrentView.OnEnableAnimation();
            CurrentView.OnLoad(options);
            await UniTask.WaitUntil(() => CurrentView.Status.Value == eStatus.Display, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// スクリーン削除
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadScreen(ViewBase viewBase, CancellationToken cancellationToken)
        {
            viewBase.OnDisableAnimation();
            viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            await SceneManager.UnloadSceneAsync(viewBase.GetType().Name).ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// ダイアログ削除
        /// </summary>
        /// <param name="viewBase"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>UniTask</returns>
        async UniTask OnUnloadDialog(ViewBase viewBase, CancellationToken cancellationToken)
        {
            viewBase.OnDisableAnimation();
            viewBase.OnUnload();
            await UniTask.WaitUntil(() => viewBase.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            Destroy(viewBase.gameObject);
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
                stackType.Pop();
                stackOptions.Pop();
            }
        }
    }
}