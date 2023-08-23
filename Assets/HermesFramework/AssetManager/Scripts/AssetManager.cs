using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Hermes.Localize;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Hermes.Asset
{
    /// <summary>
    /// AddressableをLoad、Releaseするクラス
    /// </summary>
    public class AssetManager : SingletonMonoBehaviour<AssetManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>AsyncOperationHandleList</summary>
        Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> asyncOperationHandleList = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

        /// <summary>初期化完了しているか</summary>
        public bool IsInitializad { get; private set; } = false;

        /// <summary>
        /// Start
        /// </summary>
        async void Start()
        {
            // 初期化
            await Addressables.InitializeAsync();

            // カタログアップデート
            await UpdateCatalogs();

            // 初期化完了
            IsInitializad = true;
        }

        /// <summary>
        /// カタログアップデート
        /// </summary>
        /// <returns></returns>
        public async UniTask UpdateCatalogs()
        {
            // 変更されたカタログのロケータID一覧を取得
            var checkUpdatesHandle = Addressables.CheckForCatalogUpdates(false);
            await checkUpdatesHandle.Task;
            var updates = checkUpdatesHandle.Result;
            Addressables.Release(checkUpdatesHandle);
            if (updates.Count >= 1)
            {
                // カタログを更新する
                // 引数を指定しない（catalogをnullに指定する）とすべてのカタログを更新
                await Addressables.UpdateCatalogs();

                //// 特定のカタログだけフィルタリングして渡すこともできる
                //await Addressables.UpdateCatalogs(updates);

                // LocalizeManagerを再初期化
                await LocalizeManager.Instance.Reinitialize();
            }
        }

        /// <summary>
        /// ロードメソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Addressable Name</param>
        /// <param name="onLoaded">ロードが完了した時にデータが返ってくる</param>
        /// <param name="releaseTarget">指定したオブジェクトが破壊された時にAddressableをReleaseする</param>
        /// <param name="token">CancellationToken</param>
        public async void Load<T>(string key, Action<T> onLoaded, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
        {
            var ob = await LoadAsync<T>(key, releaseTarget, token);
            onLoaded?.Invoke(ob);
        }

        /// <summary>
        /// ロードが完了した時にデータが返ってくるAsyncメソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Addressable Name</param>
        /// <param name="releaseTarget">指定したオブジェクトが破壊された時にAddressableをReleaseする</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>UniTask<T>(データ)</returns>
        public async UniTask<T> LoadAsync<T>(string key, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
        {
            // 初期化が完了するまで待機
            await UniTask.WaitUntil(() => AssetManager.Instance.IsInitializad);

            if (asyncOperationHandleList.ContainsKey(key))
            {
                await asyncOperationHandleList[key].ToUniTask(cancellationToken: token);
                return (T)Convert<T>(asyncOperationHandleList[key].Result);
            }

            var handle = Addressables.LoadAssetAsync<UnityEngine.Object>(key);
            asyncOperationHandleList.Add(key, handle);
            await handle.ToUniTask(cancellationToken: token);

            // 自動的にGameObjectが破棄された時にhandleをreleaseする
            if (releaseTarget)
                releaseTarget.GetOrAddComponent<DestroyEventListener>().OnDestroyed += () => Release(key);

            return (T)Convert<T>(handle.Result);
        }

        /// <summary>
        /// AddressablesのHandleを指定してReleaseする
        /// </summary>
        /// <param name="key">Addressable Name</param>
        public void Release(string key)
        {
            if (asyncOperationHandleList.ContainsKey(key))
            {
                Addressables.Release(asyncOperationHandleList[key]);
                asyncOperationHandleList.Remove(key);
                return;
            }
        }

        /// <summary>
        /// AddressablesのHandleを全てReleaseする
        /// </summary>
        public void ReleaseAll()
        {
            foreach (var op in asyncOperationHandleList)
                Addressables.Release(op.Value);
            asyncOperationHandleList.Clear();
        }

        /// <summary>
        /// 変換メソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns>T</returns>
        object Convert<T>(UnityEngine.Object obj) where T : UnityEngine.Object
        {
            // Texture2D
            if (obj is Texture2D)
            {
                // Texture2Dに変換
                var tex = (Texture2D)obj;

                // T = Sprite
                if (typeof(T).Equals(typeof(Sprite)))
                {
                    // Texture2DをSpriteに変換
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                    return sprite;
                }
            }
            // そのまま返す
            return obj;
        }

        /// <summary>
        /// OnDestroy時にReleaseAll()を呼ぶ
        /// </summary>
        void OnDestroy()
        {
            ReleaseAll();
        }
    }
}