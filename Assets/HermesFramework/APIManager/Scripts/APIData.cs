using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// サーバーデータのベースクラス
    /// </summary>
    [Serializable]
    public abstract class APIData<TData, TRequest, TResponse> : APIDataBase<TData>
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
    {
        /// <summary>
        /// リクエストのベースクラス
        /// </summary>
        public class RequestBase
        {
            /// <summary>
            /// コンストラクタ
            /// </summary>
            public RequestBase() { }
        }

        /// <summary>
        /// レスポンスのベースクラス
        /// </summary>
        public class ResponseBase
        {
            [SerializeField] eAPIErrorCode error_code;
            /// <summary>ErrorCode</summary>
            public eAPIErrorCode ErrorCode { get => error_code; }

            [SerializeField] string error_message;
            /// <summary>ErrorMessage</summary>
            public string ErrorMessage { get => error_message; }

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public ResponseBase() { }
        }

        /// <summary>共通データ</summary>
        public APIDataCommon common;
        /// <summary>リクエスト</summary>
        public TRequest request;
        /// <summary>レスポンス</summary>
        public TResponse data;

        /// <summary>
        /// POST通信
        /// </summary>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public abstract UniTask SendWebRequest(Action<TData> onSuccess = null, Action<TData> onFailed = null, bool isQueue = false, bool isRetry = true);
    }
}