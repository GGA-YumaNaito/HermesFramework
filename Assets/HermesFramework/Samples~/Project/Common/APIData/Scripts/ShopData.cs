using System;
using Cysharp.Threading.Tasks;
using Hermes.API;
using UnityEngine;

namespace API
{
    /// <summary>
    /// ショップデータ
    /// </summary>
    [Serializable]
    public class ShopData : APIData<ShopData, ShopData.Request, ShopData.Response>
    {
        protected override string api => "shop/list";

        protected override bool isPost => true;

        /// <summary>
        /// レスポンス
        /// </summary>
        [Serializable]
        public class Response : ResponseBase, ISerializationCallbackReceiver
        {
            [SerializeField] string name;
            /// <summary>ショップ名</summary>
            public string Name { get => name; }

            [SerializeField] string open_date_time;
            DateTime _open_date_time;
            /// <summary>開始時間</summary>
            public DateTime OpenDateTime { get => _open_date_time; }

            [SerializeField] string close_date_time;
            DateTime _close_date_time;
            /// <summary>終了時間</summary>
            public DateTime CloseDateTime { get => _close_date_time; }

            public void OnBeforeSerialize()
            {
            }

            public void OnAfterDeserialize()
            {
                if (string.IsNullOrEmpty(open_date_time) || string.IsNullOrEmpty(close_date_time))
                    return;
                DateTime date;
                if (DateTime.TryParse(open_date_time, out date))
                    _open_date_time = date;
                if (DateTime.TryParse(close_date_time, out date))
                    _close_date_time = date;
            }
        }

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request : RequestBase
        {
            public int shopId;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Request()
            {
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
        public override async UniTask SendWebRequest(Action<ShopData> onSuccess = null, Action<ShopData> onFailed = null, bool isQueue = false, bool isRetry = true)
        {
            await APIManager.Instance.SendWebRequest<ShopData, Request, Response>(request, onSuccess, onFailed, isQueue, isRetry);
        }
    }
}