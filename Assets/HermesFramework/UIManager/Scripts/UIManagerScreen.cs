using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mobcast.Coffee.Transition;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hermes.UI.UIManagerParts
{
    /// <summary>
    /// UIManager Screen
    /// </summary>
    [Serializable]
    public class UIManagerScreen
    {
        /// <summary>現在のスクリーン</summary>
        [SerializeField] ViewBase currentScreen;
        /// <summary>現在のスクリーン</summary>
        public ViewBase CurrentScreen { get => currentScreen; private set => currentScreen = value; }
        /// <summary>現在のScene名</summary>
        [SerializeField] string currentScreenName;
        /// <summary>シーン遷移アニメーション</summary>
        [SerializeField] UITransition sceneTransition;

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="viewName">View名</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask<ViewBase></returns>
        public async UniTask<ViewBase> LoadAsync<T>(string viewName, object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            return await LoadAsync(viewName, typeof(T), options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="viewName">View名</param>
        /// <param name="type">タイプ</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask<ViewBase></returns>
        public async UniTask<ViewBase> LoadAsync(string viewName, Type type, object options = null, CancellationToken cancellationToken = default)
        {
            // シーンロード
            await SceneManager.LoadSceneAsync(viewName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
            //var instance = await Addressables.LoadSceneAsync(viewName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);

            CurrentScreen = (ViewBase)GameObject.Find(viewName).GetComponent(type);
            currentScreenName = viewName;

            if (CurrentScreen == null)
                throw new Exception($"{viewName} is Null");

            // Initialize & Load
            CurrentScreen.Initialize();
            await CurrentScreen.OnLoad(options);
            // シーン遷移退出アニメーション
            await OnDisableSceneTransition();
            await CurrentScreen.OnDisplay();
            await UniTask.WaitUntil(() => CurrentScreen.Status.Value == eStatus.Display, cancellationToken: cancellationToken);

            return CurrentScreen;
        }

        /// <summary>
        /// UnloadAsync
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask UnloadAsync(CancellationToken cancellationToken)
        {
            await CurrentScreen.OnEnd();
            // シーン遷移出現アニメーション
            await OnEnableSceneTransition();
            await CurrentScreen.OnUnload();
            await UniTask.WaitUntil(() => CurrentScreen.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            await SceneManager.UnloadSceneAsync(currentScreenName).ToUniTask(cancellationToken: cancellationToken);

            CurrentScreen = null;
            currentScreenName = null;
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