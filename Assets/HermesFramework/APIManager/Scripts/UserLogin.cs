using System;
using Cysharp.Threading.Tasks;
using Hermes.API;
using UnityEngine;

namespace API
{
    /// <summary>
    /// ユーザーログイン
    /// </summary>
    [Serializable]
    public class UserLogin : APIData<UserLogin, UserLogin.Request, UserLogin.Response>
    {
        protected override string api => "user/login";

        protected override bool isPost => true;

        public override Request request { get; set; }
        public override Response response { get; set; }

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
            /// <summary>ユーザーID</summary>
            [SerializeField] string user_id;
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
            public Request(string user_id, string uuid)
            {
                this.user_id = user_id;
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