using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        static Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> asyncOperationHandleList = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

        /// <summary>
        /// ロードメソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Addressable Name</param>
        /// <param name="onLoaded">ロードが完了した時にデータが返ってくる</param>
        /// <param name="releaseTarget">指定したオブジェクトが破壊された時にAddressableをReleaseする</param>
        /// <param name="token">CancellationToken</param>
        public static async void Load<T>(string key, Action<T> onLoaded, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
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
        public static async UniTask<T> LoadAsync<T>(string key, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
        {
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
        public static void Release(string key)
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
        public static void ReleaseAll()
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
        static object Convert<T>(UnityEngine.Object obj) where T : UnityEngine.Object
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