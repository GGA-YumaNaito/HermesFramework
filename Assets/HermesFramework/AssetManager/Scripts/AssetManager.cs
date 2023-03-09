using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Hermes.Asset
{
    /// <summary>
    /// AssetManager
    /// </summary>
    public class AssetManager : SingletonMonoBehaviour<AssetManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>AsyncOperationHandleList</summary>
        static Dictionary<string, AsyncOperationHandle<UnityEngine.Object>?> asyncOperationHandleList = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>?>();

        /// <summary>
        /// Load
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="onLoaded"></param>
        /// <param name="releaseTarget"></param>
        /// <param name="token"></param>
        public static async void Load<T>(string key, Action<T> onLoaded, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
        {
            var ob = await LoadAsync<T>(key, releaseTarget, token);
            onLoaded?.Invoke(ob);
        }

        /// <summary>
        /// LoadAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="releaseTarget"></param>
        /// <param name="token"></param>
        /// <returns>UniTask<T></returns>
        public static async UniTask<T> LoadAsync<T>(string key, GameObject releaseTarget = null, CancellationToken token = default) where T : UnityEngine.Object
        {
            if (asyncOperationHandleList.ContainsKey(key))
                return (T)asyncOperationHandleList[key].Value.Result;

            AsyncOperationHandle<T>? handle = Addressables.LoadAssetAsync<T>(key);
            asyncOperationHandleList.Add(key, handle as AsyncOperationHandle<UnityEngine.Object>?);
            await handle.ToUniTask(cancellationToken: token);

            // 自動的にGameObjectが破棄された時にhandleをreleaseする
            if (releaseTarget)
                releaseTarget.GetOrAddComponent<DestroyEventListener>().OnDestroyed += () => Release(key);

            return handle.Value.Result;
        }

        /// <summary>
        /// Release
        /// </summary>
        /// <param name="key"></param>
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
        /// ReleaseAll
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var op in asyncOperationHandleList)
                Addressables.Release(op.Value);
            asyncOperationHandleList.Clear();
        }

        /// <summary>
        /// OnDestroy
        /// </summary>
        void OnDestroy()
        {
            ReleaseAll();
        }
    }
}