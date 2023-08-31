using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hermes.Asset;
using UnityEngine;

namespace Hermes.UI.UIManagerParts
{
    /// <summary>
    /// UIManager Dialog
    /// </summary>
    [Serializable]
    public class UIManagerDialog
    {
        /// <summary>ダイアログ用BG</summary>
        [SerializeField] GameObject dialogBG;
        /// <summary>ダイアログRoot</summary>
        [SerializeField] Transform dialogRoot;

        /// <summary>現在のダイアログ</summary>
        [SerializeField] ViewBase currentDialog;
        /// <summary>現在のダイアログ</summary>
        public ViewBase CurrentDialog { get => currentDialog; private set => currentDialog = value; }
        /// <summary>DialogList</summary>
        [SerializeField] List<StackData> dialogList = new List<StackData>();

        /// <summary>
        /// StackData
        /// </summary>
        [Serializable]
        public class StackData
        {
            /// <summary>名前</summary>
            public string viewName;
            /// <summary>タイプ</summary>
            public Type viewType;
            /// <summary>オプション</summary>
            public object options;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="viewName"></param>
            /// <param name="viewType"></param>
            /// <param name="options"></param>
            public StackData(string viewName, Type viewType, object options)
            {
                this.viewName = viewName;
                this.viewType = viewType;
                this.options = options;
            }
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <typeparam name="T">ViewBase</typeparam>
        /// <param name="viewName">View名</param>
        /// <param name="releaseTarget">指定したオブジェクトが破壊された時にAddressableをReleaseする</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask<ViewBase></returns>
        public async UniTask<ViewBase> LoadAsync<T>(string viewName, GameObject releaseTarget, object options = null, CancellationToken cancellationToken = default) where T : ViewBase
        {
            return await LoadAsync(viewName, typeof(T), releaseTarget, options, cancellationToken);
        }

        /// <summary>
        /// ViewBaseを継承したクラスのLoadAsync
        /// </summary>
        /// <param name="viewName">View名</param>
        /// <param name="type">タイプ</param>
        /// <param name="releaseTarget">指定したオブジェクトが破壊された時にAddressableをReleaseする</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask<ViewBase></returns>
        public async UniTask<ViewBase> LoadAsync(string viewName, Type type, GameObject releaseTarget, object options = null, CancellationToken cancellationToken = default)
        {
            // バリアON
            dialogBG.SetActive(true);

            // ロードアセット
            var gameObject = await AssetManager.Instance.LoadAsync<GameObject>(viewName, releaseTarget, cancellationToken);

            // DialogBGの位置を上げる
            dialogBG.transform.SetAsLastSibling();

            // Instantiate
            CurrentDialog = (ViewBase)GameObject.Instantiate(gameObject, dialogRoot).GetComponent(type);

            if (CurrentDialog == null)
                throw new Exception($"{viewName} is Null");

            CurrentDialog.name = viewName;

            StackPush(viewName, type, options);

            // Initialize & Load
            CurrentDialog.Initialize();
            await CurrentDialog.OnLoad(options);
            await CurrentDialog.OnDisplay();
            await UniTask.WaitUntil(() => CurrentDialog.Status.Value == eStatus.Display, cancellationToken: cancellationToken);
            CurrentDialog.OnStart();

            return CurrentDialog;
        }

        /// <summary>
        /// UnloadAsync
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask UnloadAsync(CancellationToken cancellationToken = default)
        {
            await UnloadAsync(false, cancellationToken);
        }

        /// <summary>
        /// UnloadAsync
        /// </summary>
        /// <param name="isBack">前画面に戻る処理ならtrue</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask UnloadAsync(bool isBack, CancellationToken cancellationToken = default)
        {
            if (CurrentDialog == null)
                return;

            if (isBack)
                await CurrentDialog.OnEnd();
            await CurrentDialog.OnUnload();
            if (isBack)
                await UniTask.WaitUntil(() => CurrentDialog.Status.Value == eStatus.End, cancellationToken: cancellationToken);
            GameObject.Destroy(CurrentDialog.gameObject);
            if (isBack)
            {
                // DialogBGの位置を下げる
                var dialogBGTransform = dialogBG.transform;
                dialogBGTransform.SetSiblingIndex(dialogBGTransform.GetSiblingIndex() - 1);
            }
            var unloadData = dialogList.Pop();

            AssetManager.Instance.Release(unloadData.viewName);

            if (dialogList.Count > 0)
            {
                var data = dialogList.Peek();
                CurrentDialog = (ViewBase)dialogRoot.Find(data.viewName).GetComponent(data.viewType);
            }
            else
            {
                dialogBG.SetActive(false);
                CurrentDialog = null;
            }
        }

        /// <summary>
        /// 全てアンロード
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask AllUnloadAsync(CancellationToken cancellationToken = default)
        {
            // ダイアログ
            var count = dialogList.Count;
            if (count == 0)
                return;
            for (int i = 0; i < count; i++)
            {
                UnloadAsync(cancellationToken).Forget();
            }
            dialogBG.SetActive(false);

            await UniTask.WaitUntil(() => CurrentDialog == null, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Stack push
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="options">Options</param>
        void StackPush(string name, Type type, object options)
        {
            dialogList.Push(new StackData(name, type, options));
        }
    }
}