using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Hermes.UI;

namespace Hermes.API
{
    /// <summary>
    /// APIマネージャー
    /// </summary>
    public class APIManager : SingletonMonoBehaviour<APIManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>接続先</summary>
        [SerializeField] string baseUrl = "https://www.";
        /// <summary>タイムアウト.</summary>
        [SerializeField] private int timeout = 10;
        /// <summary>進捗待ち時間.</summary>
        [SerializeField] private float m_ProgressWaitTime = 3f;
        /// <summary>リトライ最大回数.</summary>
        [SerializeField] private int maxRetryNum = 3;
        /// <summary>リトライ回数.</summary>
        private int retryNum = 0;
        /// <summary>溜まっているキューがリクエスト可能か.</summary>
        [SerializeField] private bool m_IsRequestQueue = true;
        /// <summary>TypeQueue.</summary>
        [SerializeField] private Queue<Type> m_TypeQueue = new Queue<Type>();
        /// <summary>シーケンスID.</summary>
        private static ulong m_SequenceId = 0;
        /// <summary>リセットフラグ.</summary>
        private bool m_IsReset = false;
        /// <summary>起動シーン名</summary>
        [SerializeField] string launchName = "LaunchScene";

        // TODO:シーケンスIDをどう扱うかを考える

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        public static async UniTask SendWebRequest<T, T2>(T postData, Action<T2> onSuccess = null, Action<T2> onFailed = null, bool isQueue = false, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
            await SendWebRequest(postData, "", onSuccess, onFailed, isQueue, isRetry, sequenceId);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        public static async UniTask SendWebRequest<T, T2>(T postData, Dictionary<string, object> queryParams, Action<T2> onSuccess = null, Action<T2> onFailed = null, bool isQueue = false, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
            await SendWebRequest(postData, QueryParamConvertDictionaryToString(queryParams), onSuccess, onFailed, isQueue, isRetry, sequenceId);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        public static async UniTask SendWebRequest<T, T2>(T postData, object[] queryParams, Action<T2> onSuccess = null, Action<T2> onFailed = null, bool isQueue = false, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
            await SendWebRequest(postData, QueryParamConvertObjectArrayToString(queryParams), onSuccess, onFailed, isQueue, isRetry, sequenceId);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        static async UniTask SendWebRequest<T, T2>(T postData, string queryParams, Action<T2> onSuccess = null, Action<T2> onFailed = null, bool isQueue = false, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
            // キューにためる場合.
            if (isQueue && Instance.retryNum <= 0)
            {
                // リセットフラグ解除.
                Instance.m_IsReset = false;
                // キューにためる.
                Instance.m_TypeQueue.Enqueue(typeof(T));
                // 自分の番まで処理中断.
                while (true)
                {
                    await UniTask.Yield();
                    if (Instance.m_IsRequestQueue)
                    {
                        if (Instance.m_TypeQueue.Peek() == typeof(T))
                        {
                            if (Instance.m_IsReset)
                            {
                                Instance.m_TypeQueue.Dequeue();
                                return;
                            }
                            Instance.m_IsRequestQueue = false;
                            break;
                        }
                    }
                }
            }
            await PostSendWebRequest<T, T2>(
                postData, queryParams,
                // 成功コールバックを実行.
                (request, data) =>
                {
                    if (string.IsNullOrWhiteSpace(request.downloadHandler.text))
                        onSuccess?.Invoke(null);
                    else
                    {
                        data.SetHeaders(request.GetResponseHeaders());
                        onSuccess?.Invoke(data);
                    }
                },
                // 失敗コールバックを実行.
                (request) =>
                {
                    if (string.IsNullOrWhiteSpace(request.downloadHandler.text))
                        onFailed?.Invoke(null);
                    else
                    {
                        var json = GzipBase64.Decompress(request.downloadHandler.text);
                        var data = JsonUtility.FromJson<T2>(json);
                        data.SetHeaders(request.GetResponseHeaders());
                        onFailed?.Invoke(data);
                    }
#if DEBUG_LOG
                    Debug.Log ("<color=red><" + typeof(T) + "> Failed : " + request.errorCode.Label () + "</color>");
#endif
                },
                // 通信失敗.
                (request) =>
                {
#if DEBUG_LOG
                    Debug.Log ("<color=red><" + typeof(T) + "> Error : " + request.error + "</color>");
#endif
                }, isRetry, sequenceId);

            await UniTask.Yield();
            // キューにためる場合.
            if (isQueue && Instance.retryNum <= 0)
            {
                if (Instance.m_TypeQueue.Count > 0 && Instance.m_TypeQueue.Peek() == typeof(T))
                {
                    // リクエスト再開.
                    Instance.m_IsRequestQueue = true;
                    Instance.m_TypeQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="onError">通信がエラーだった時のコールバック.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        static async UniTask PostSendWebRequest<T, T2>(T postData, string queryParams, Action<UnityWebRequest, T2> onSuccess = null, Action<UnityWebRequest> onFailed = null, Action<UnityWebRequest> onError = null, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
            var serverData = new T2();
            serverData.SetQueryParam(queryParams);
            var url = Instance.baseUrl + serverData.API;

            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                Debug.Log("postData = " + JsonUtility.ToJson(postData));
#endif
                var gzip = GzipBase64.Compress(JsonUtility.ToJson(postData));
                byte[] bytes = Encoding.UTF8.GetBytes(gzip);

                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = Instance.timeout;
                request.SetRequestHeader("Content-Type", "application/json");
                //request.SetRequestHeader("accept-encoding", "gzip");
                //request.SetRequestHeader("user-agent", "gzip");

                // start and wait
                var ao = request.SendWebRequest();

                await UniTask.WaitWhile(() => !ao.isDone);

                // 通信失敗
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    await PostSendWebRequestError<T, T2>(false, request, url, postData, queryParams, onSuccess, onFailed, onError, isRetry, sequenceId);
                }
                // 通信成功
                else
                {
                    // 通信は成功したが、APIが失敗していたら
                    var json = GzipBase64.Decompress(request.downloadHandler.text);
                    var data = JsonUtility.FromJson<T2>(json);
                    if (data.ErrorCode > eAPIErrorCode.None)
                    {
                        await PostSendWebRequestError<T, T2>(true, request, url, postData, queryParams, onSuccess, onFailed, onError, isRetry, sequenceId);
                    }
                    // APIが成功だったら
                    else
                    {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                        Debug.Log("Success!! url = " + url);
                        Debug.Log(request.downloadHandler.text);
#endif
                        onSuccess?.Invoke(request, data);
                        Instance.retryNum = 0;
                        // リセットフラグ起動.
                        Instance.m_IsReset = true;
                    }
                }
            }
        }

        /// <summary>
        /// POST通信失敗
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="isFailed">APIで失敗.</param>
        /// <param name="request">リクエスト</param>
        /// <param name="url">URL</param>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="onError">通信がエラーだった時のコールバック.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <param name="sequenceId">シーケンスID.</param>
        /// <returns>UniTask</returns>
        static async UniTask PostSendWebRequestError<T, T2>(bool isFailed, UnityWebRequest request, string url, T postData, string queryParams, Action<UnityWebRequest, T2> onSuccess = null, Action<UnityWebRequest> onFailed = null, Action<UnityWebRequest> onError = null, bool isRetry = true, ulong sequenceId = 0) where T : class where T2 : APIDataBase<T2>, new()
        {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            if (isFailed)
                Debug.LogError("Failed!! url = " + url);
            else
                Debug.LogError("Error!! url = " + url);
            Debug.LogError(request.error);
#endif

            // リトライするか.
            if (isRetry)
            {
                if (Instance.retryNum <= Instance.maxRetryNum)
                {
                    Instance.retryNum++;
                    // TODO: 共通の汎用ダイアログを作るか否か
                    // ダイアログ.
                    //Dialog dialog = null;
                    //if (isFailed)
                    //{
                    //    var json = GzipBase64.Decompress(request.downloadHandler.text);
                    //    var data = JsonUtility.FromJson<T2>(json);
                    //    dialog = DialogServer.Create("APIエラー", "接続が正常に行われておりません。", data.ErrorCode.Label(), true);
                    //}
                    //else
                    //    dialog = DialogServer.Create("HTTPエラー", "接続が正常に行われておりません。", request.error, true);
                    //var dialog = UI.UIManager.Instance.LoadAsync<Dialog>();
                    //await UniTask.WaitUntil(() => dialog.StateWait());
                    //if (dialog.state == DialogServer.State.Retry)
                    //{
                    //    // リトライ.
                    //    await UIManager.Instance.BackAsync();
                    //    await PostSendWebRequest<T, T2>(postData, queryParams, onSuccess, onFailed, onError, isRetry, sequenceId);
                    //}
                    //else
                    {
                        // タイトルシーンに戻る.
                        // 失敗コールバックを実行.
                        if (isFailed)
                            onFailed?.Invoke(request);
                        // エラーコールバックを実行.
                        else
                            onError?.Invoke(request);
                        Instance.retryNum = 0;
                        // リセットフラグ起動.
                        Instance.m_IsReset = true;
                        // 全てアンロード.
                        await UIManager.Instance.AllUnloadAsync();
                        // タイトルシーン.
                        await UIManager.Instance.LoadAsync(Instance.launchName);
                    }
                }
                else
                {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                    if (isFailed)
                        Debug.LogError("Failed!! Max retry num");
                    else
                        Debug.LogError("Error!! Max retry num");
#endif
                    Instance.retryNum = 0;
                    // リセットフラグ起動.
                    Instance.m_IsReset = true;
                    // TODO: 共通の汎用ダイアログを作るか否か
                    // ダイアログ.
                    //Dialog dialog = null;
                    //if (isFailed)
                    //{
                    //    var json = GzipBase64.Decompress(request.downloadHandler.text);
                    //    var data = JsonUtility.FromJson<T2>(json);
                    //    dialog = DialogServer.Create(title: "タイトルに戻ります", error: data.ErrorCode.Label(), isRetry: false);
                    //}
                    //else
                    //    dialog = DialogServer.Create(title: "タイトルに戻ります", error: request.error, isRetry: false);
                    //var dialog = UIManager.Instance.LoadAsync<Dialog>();
                    //await UniTask.WaitUntil(() => dialog.StateWait());
                    // 失敗コールバックを実行.
                    if (isFailed)
                        onFailed?.Invoke(request);
                    // エラーコールバックを実行.
                    else
                        onError?.Invoke(request);
                    

                    // 全てアンロード.
                    await UIManager.Instance.AllUnloadAsync();
                    // タイトルシーン.
                    await UIManager.Instance.LoadAsync(Instance.launchName);
                }
            }
            else
            {
                // 失敗コールバックを実行.
                if (isFailed)
                    onFailed?.Invoke(request);
                // エラーコールバックを実行.
                else
                    onError?.Invoke(request);
            }
        }

        /// <summary>
        /// クエリパラメータ変換
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        static string QueryParamConvertDictionaryToString(Dictionary<string, object> queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
                return "";
            var i = 0;
            var sb = new StringBuilder();
            foreach (var pair in queryParams)
            {
                if (i == 0)
                {
                    sb.Append("?");
                }
                else
                {
                    sb.Append("&");
                }
                sb.Append(pair.Key);
                sb.Append("=");
                sb.Append(pair.Value);
                i++;
            }
            return sb.ToString();
        }

        /// <summary>
        /// クエリパラメータ変換
        /// </summary>
        /// <param name="queryParams"></param>
        /// <returns></returns>
        static string QueryParamConvertObjectArrayToString(object[] queryParams)
        {
            if (queryParams == null || queryParams.Length == 0)
                return "";
            var i = 0;
            var sb = new StringBuilder();
            foreach (var param in queryParams)
            {
                if (i == 0)
                {
                    sb.Append("?");
                }
                else
                {
                    sb.Append("&");
                }
                sb.Append(param);
                i++;
            }
            return sb.ToString();
        }
    }
}