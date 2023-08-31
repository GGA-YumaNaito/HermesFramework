using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Hermes.API;

namespace API
{
    /// <summary>
    /// HomeData
    /// </summary>
    [Serializable]
    public class HomeData : APIData<HomeData, HomeData.Request, HomeData.Response>
    {
        protected override string api => "home";

        protected override bool isPost => true;

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase
        {
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
            /// <summary>プレイヤーID</summary>
            [SerializeField] string player_id;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Request()
            {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="player_id">プレイヤーID</param>
            public Request(string player_id)
            {
                this.player_id = player_id;
            }
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public override async UniTask SendWebRequest(Action<HomeData> onSuccess = null, Action<HomeData> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<HomeData, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}