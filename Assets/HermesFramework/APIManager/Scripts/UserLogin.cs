using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// ユーザーログイン
    /// </summary>
    [Serializable]
    public class UserLogin : APIData<UserLogin, UserLogin.Request, UserLogin.Response>
    {
        protected override string api => "user/login";

        protected override bool isPost => true;

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase, ISerializationCallbackReceiver
        {
            /// <summary>アクセストークン</summary>
            [SerializeField] string access_token;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Response()
            {
            }

            public void OnBeforeSerialize() { }

            public void OnAfterDeserialize()
            {
                APIManager.Instance.SetAccessToken(access_token);
            }
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
            /// <summary>プレイヤーID</summary>
            [SerializeField] string player_id;
            /// <summary>UUID</summary>
            [SerializeField] string uuid;

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
            /// <param name="uuid">UUID</param>
            public Request(string player_id, string uuid)
            {
                this.player_id = player_id;
                this.uuid = uuid;
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
        public override async UniTask SendWebRequest(Action<UserLogin> onSuccess = null, Action<UserLogin> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<UserLogin, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}