using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Hermes.UI.UIManagerParts
{
    /// <summary>
    /// UIManager SubScene
    /// </summary>
    [Serializable]
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
        /// <returns>UniTask<SubScene></returns>
        public async UniTask<SubScene> LoadAsync<T>(string sceneName, object options = null, CancellationToken cancellationToken = default) where T : SubScene
        {
            var type = typeof(T);
            // シーンロード
            var instance = await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellationToken);
            var subScene = GameObject.Find(sceneName).GetComponent(type) as SubScene;

            if (subScene == null)
                throw new Exception($"{sceneName} is Null");

            SubSceneList.Add(subScene);
            subSceneInstanceList.Add(new KeyValuePair<string, SceneInstance>(sceneName, instance));

            // Initialize & Load
            subScene.Initialize();
            await subScene.OnLoad(options);
            await subScene.OnDisplay();
            return subScene;
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
                    var subScene = SubSceneList[i];
                    var subSceneInstance = subSceneInstanceList[i];
                    await subScene.OnEnd();
                    await subScene.OnUnload();
                    await UniTask.WaitUntil(() => subScene.Status.Value == eStatus.End, cancellationToken: cancellationToken);

                    // シーンアンロード
                    await Addressables.UnloadSceneAsync(subSceneInstance.Value).ToUniTask(cancellationToken: cancellationToken);
                    SubSceneList.Remove(subScene);
                    subSceneInstanceList.Remove(subSceneInstance);
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