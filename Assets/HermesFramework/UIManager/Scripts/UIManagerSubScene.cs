﻿using System;
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

namespace Hermes.UI.UIManagerParts
{
    /// <summary>
    /// UIManager SubScene
    /// </summary>
    public class UIManagerSubScene
    {
        /// <summary>SubSceneList</summary>
        [SerializeField] List<SubScene> subSceneList = new List<SubScene>();
        /// <summary>SubSceneList</summary>
        public List<SubScene> SubSceneList { get { return subSceneList; } private set { subSceneList = value; } }
        /// <summary>SubSceneInstanceList</summary>
        List<KeyValuePair<string, SceneInstance>> subSceneInstanceList = new List<KeyValuePair<string, SceneInstance>>();

        /// <summary>
        /// LoadAsync
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sceneName">シーン名</param>
        /// <param name="options">オプション</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask LoadAsync<T>(string sceneName, object options = null, CancellationToken cancellationToken = default) where T : SubScene
        {
            var type = typeof(T);
            // シーンロード
            var instance = await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
            var screen = GameObject.Find(sceneName).GetComponent(type) as SubScene;

            if (screen == null)
                throw new Exception($"{sceneName} is Null");

            SubSceneList.Add(screen);
            subSceneInstanceList.Add(new KeyValuePair<string, SceneInstance>(sceneName, instance));

            // Initialize & Load
            screen.Initialize();
            await screen.OnLoad(options);
            await screen.OnDisplay();
        }

        /// <summary>
        /// UnloadAsync
        /// </summary>
        /// <param name="sceneName">シーン名</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask UnloadAsync(string sceneName, CancellationToken cancellationToken = default)
        {
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
        }

        /// <summary>
        /// 全てアンロード
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask AllUnloadAsync(CancellationToken cancellationToken = default)
        {
            // サブシーン
            for (int i = 0; i < SubSceneList.Count; i++)
            {
                await SubSceneList[i].OnUnload();

                // シーンアンロード
                await Addressables.UnloadSceneAsync(subSceneInstanceList[i].Value).ToUniTask(cancellationToken: cancellationToken);
                SubSceneList.RemoveAt(i);
                subSceneInstanceList.RemoveAt(i);
            }
        }
    }
}