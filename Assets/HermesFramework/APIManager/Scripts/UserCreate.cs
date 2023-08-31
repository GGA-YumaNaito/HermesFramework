using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// UserCreate
    /// </summary>
    [Serializable]
    public class UserCreate : APIData<UserCreate, UserCreate.Request, UserCreate.Response>
    {
        protected override string api => "user/create";

        protected override bool isPost => true;

        /// <summary>
        /// OSのタイプ
        /// </summary>
        public enum eOSType : int
        {
            iOS = 1,
            Android = 2
        }

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase
        {
            /// <summary>
            /// レスポンス
            /// </summary>
            [Serializable]
            public class UserData
            {
                [SerializeField] string user_id;
                /// <summary>ユーザーID</summary>
                public string UserId { get => user_id; }

                [SerializeField] string uuid;
                /// <summary>UUID</summary>
                public string UUID { get => uuid; }

                [SerializeField] string user_name;
                /// <summary>ユーザー名</summary>
                public string UserName { get => user_name; }

                [SerializeField] eOSType os_type;
                /// <summary>1:iOS, 2:Android</summary>
                public eOSType OSType { get => os_type; }

                [SerializeField] string device;
                /// <summary>デバイス名</summary>
                public string Device { get => device; }

                [SerializeField] string player_id;
                /// <summary>プレイヤーID</summary>
                public string PlayerId { get => player_id; }
            }

            [SerializeField] UserData t_user;
            /// <summary>ユーザーデータ</summary>
            public UserData User { get => t_user; }
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
            /// <summary>UUID</summary>
            [SerializeField] string uuid;
            /// <summary>ユーザー名</summary>
            [SerializeField] string user_name;
            /// <summary>1:iOS, 2:Android</summary>
            [SerializeField] eOSType os_type;
            /// <summary>デバイス名</summary>
            [SerializeField] string device;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Request()
            {
            }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="uuid">UUID</param>
            /// <param name="uuid">ユーザー名</param>
            /// <param name="uuid">1:iOS, 2:Android</param>
            /// <param name="uuid">デバイス名</param>
            public Request(string uuid, string user_name, eOSType os_type, string device)
            {
                this.uuid = uuid;
                this.user_name = user_name;
                this.os_type = os_type;
                this.device = device;
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
        public override async UniTask SendWebRequest(Action<UserCreate> onSuccess = null, Action<UserCreate> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<UserCreate, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}