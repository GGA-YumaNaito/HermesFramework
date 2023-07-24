using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Hermes.UI;
using UnityEngine;
using UnityEngine.Networking;

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
        [SerializeField] int timeout = 10;
        /// <summary>リトライ最大回数.</summary>
        [SerializeField] int maxRetryNum = 3;
        /// <summary>リトライ回数.</summary>
        int retryNum = 0;
        /// <summary>溜まっているキューがリクエスト可能か.</summary>
        [SerializeField] bool isRequestQueue = true;
        /// <summary>TypeQueue.</summary>
        [SerializeField] Queue<Type> typeQueue = new Queue<Type>();
        /// <summary>リセットフラグ.</summary>
        bool isReset = false;
        /// <summary>タイトルシーン名</summary>
        [SerializeField] string titleName = "Title.TitleScene, Assembly-CSharp";
        /// <summary>LocalizeKey API error時のタイトル</summary>
        [SerializeField] string errorTitleAPIKey = "TITLE_API_ERROR";
        /// <summary>LocalizeKey HTTP error時のタイトル</summary>
        [SerializeField] string errorTitleHttpKey = "TITLE_HTTP_ERROR";
        /// <summary>LocalizeKey error時の本文</summary>
        [SerializeField] string errorBodyKey = "ERROR_BODY";
        /// <summary>LocalizeKey タイトルに戻る</summary>
        [SerializeField] string backToTitleKey = "BACK_TO_TITLE";

        /// <summary>暗号化の共有キー(非公開)</summary>
        static readonly string key = "abcdefghijklmnop";

        /// <summary>アクセストークン</summary>
        string token = string.Empty;

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public async UniTask SendWebRequest<TData, TRequest, TResponse>(TRequest postData, Action<TData> onSuccess = null, Action<TData> onFailed = null, bool isQueue = false, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            await SendWebRequest<TData, TRequest, TResponse>(postData, "", onSuccess, onFailed, isQueue, isRetry);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public async UniTask SendWebRequest<TData, TRequest, TResponse>(TRequest postData, Dictionary<string, object> queryParams, Action<TData> onSuccess = null, Action<TData> onFailed = null, bool isQueue = false, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            await SendWebRequest<TData, TRequest, TResponse>(postData, QueryParamConvertDictionaryToString(queryParams), onSuccess, onFailed, isQueue, isRetry);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        public async UniTask SendWebRequest<TData, TRequest, TResponse>(TRequest postData, object[] queryParams, Action<TData> onSuccess = null, Action<TData> onFailed = null, bool isQueue = false, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            await SendWebRequest<TData, TRequest, TResponse>(postData, QueryParamConvertObjectArrayToString(queryParams), onSuccess, onFailed, isQueue, isRetry);
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="isQueue">キューにためるか.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        async UniTask SendWebRequest<TData, TRequest, TResponse>(TRequest postData, string queryParams, Action<TData> onSuccess = null, Action<TData> onFailed = null, bool isQueue = false, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            // キューにためる場合.
            if (isQueue && Instance.retryNum <= 0)
            {
                // リセットフラグ解除.
                Instance.isReset = false;
                // キューにためる.
                Instance.typeQueue.Enqueue(typeof(TRequest));
                // 自分の番まで処理中断.
                while (true)
                {
                    await UniTask.Yield();
                    if (Instance.isRequestQueue)
                    {
                        if (Instance.typeQueue.Peek() == typeof(TRequest))
                        {
                            if (Instance.isReset)
                            {
                                Instance.typeQueue.Dequeue();
                                return;
                            }
                            Instance.isRequestQueue = false;
                            break;
                        }
                    }
                }
            }
            await PostSendWebRequest<TData, TRequest, TResponse>(
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
                    TData data = default;
                    if (string.IsNullOrWhiteSpace(request.downloadHandler.text))
                        onFailed?.Invoke(null);
                    else
                    {
                        data = GetResponseData<TData, TRequest, TResponse>(request);
                        data.SetHeaders(request.GetResponseHeaders());
                        onFailed?.Invoke(data);
                    }
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                    Debug.Log ("<color=red><" + typeof(TRequest) + "> Failed : " + data?.response.ErrorCode.Label() + "</color>");
#endif
                },
                // 通信失敗.
                (request) =>
                {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                    Debug.Log ("<color=red><" + typeof(TRequest) + "> Error : " + request.error + "</color>");
#endif
                }, isRetry);

            await UniTask.Yield();
            // キューにためる場合.
            if (isQueue && Instance.retryNum <= 0)
            {
                if (Instance.typeQueue.Count > 0 && Instance.typeQueue.Peek() == typeof(TRequest))
                {
                    // リクエスト再開.
                    Instance.isRequestQueue = true;
                    Instance.typeQueue.Dequeue();
                }
            }
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="onError">通信がエラーだった時のコールバック.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        async UniTask PostSendWebRequest<TData, TRequest, TResponse>(TRequest postData, string queryParams, Action<UnityWebRequest, TData> onSuccess = null, Action<UnityWebRequest> onFailed = null, Action<UnityWebRequest> onError = null, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            var serverData = new TData();

            // TODO: 今はPOSTでなかったら弾く
            if (!serverData.IsPost)
            {
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
                Debug.LogError("Failed!! Not POST!!!");
#endif
                return;
            }

            serverData.SetQueryParam(queryParams);
            var url = Instance.baseUrl + serverData.API;

            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                byte[] bytes;
                // 初期化ベクトル(公開)
                var iv = Guid.NewGuid().ToString("N").Substring(0, 16);

                bytes = GetRequestData(postData, iv);

                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = Instance.timeout;
                //request.SetRequestHeader("Content-Type", "application/json");
                //request.SetRequestHeader("accept-encoding", "gzip");
                //request.SetRequestHeader("user-agent", "gzip");
                request.SetRequestHeader("iv", iv);

                // start and wait
                var ao = request.SendWebRequest();

                await UniTask.WaitWhile(() => !ao.isDone);

                // 通信失敗
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    await PostSendWebRequestError<TData, TRequest, TResponse>(false, request, url, postData, queryParams, onSuccess, onFailed, onError, isRetry);
                }
                // 通信成功
                else
                {
                    // 通信は成功したが、APIが失敗していたら
                    var data = GetResponseData<TData, TRequest, TResponse>(request);
                    if (data.response.ErrorCode > eAPIErrorCode.None)
                    {
                        await PostSendWebRequestError<TData, TRequest, TResponse>(true, request, url, postData, queryParams, onSuccess, onFailed, onError, isRetry);
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
                        Instance.isReset = true;
                    }
                }
            }
        }

        /// <summary>
        /// POST通信失敗
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="isFailed">APIで失敗.</param>
        /// <param name="request">リクエスト</param>
        /// <param name="url">URL</param>
        /// <param name="postData">ポストデータ</param>
        /// <param name="queryParams">クエリパラメータ</param>
        /// <param name="onSuccess">成功コールバック</param>
        /// <param name="onFailed">失敗コールバック</param>
        /// <param name="onError">通信がエラーだった時のコールバック.</param>
        /// <param name="isRetry">リトライするか.</param>
        /// <returns>UniTask</returns>
        async UniTask PostSendWebRequestError<TData, TRequest, TResponse>(bool isFailed, UnityWebRequest request, string url, TRequest postData, string queryParams, Action<UnityWebRequest, TData> onSuccess = null, Action<UnityWebRequest> onFailed = null, Action<UnityWebRequest> onError = null, bool isRetry = true)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
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
                    // ダイアログ.
                    ErrorDialog dialog = null;
                    if (isFailed)
                    {
                        var data = GetResponseData<TData, TRequest, TResponse>(request);
                        dialog = await ErrorDialog.Create(Instance.errorTitleAPIKey, Instance.errorBodyKey, data.response.ErrorCode.Label(), true);
                    }
                    else
                        dialog = await ErrorDialog.Create(Instance.errorTitleHttpKey, Instance.errorBodyKey, request.error, true);
                    await UniTask.WaitUntil(() => dialog == null || !dialog.ClickStateWait());
                    if (dialog != null && dialog.ClickState == ErrorDialog.eClickState.Retry)
                    {
                        // リトライ.
                        await UIManager.Instance.BackAsync();
                        await PostSendWebRequest<TData, TRequest, TResponse>(postData, queryParams, onSuccess, onFailed, onError, isRetry);
                    }
                    else
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
                        Instance.isReset = true;
                        // 全てアンロード.
                        await UIManager.Instance.AllUnloadAsync();
                        // タイトルシーン.
                        await UIManager.Instance.LoadAsync(Instance.titleName);
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
                    Instance.isReset = true;
                    // ダイアログ.
                    ErrorDialog dialog = null;
                    if (isFailed)
                    {
                        var data = GetResponseData<TData, TRequest, TResponse>(request);
                        dialog = await ErrorDialog.Create(titleKey: Instance.backToTitleKey, error: data.response.ErrorCode.Label(), isRetry: false);
                    }
                    else
                        dialog = await ErrorDialog.Create(titleKey: Instance.backToTitleKey, error: request.error, isRetry: false);
                    await UniTask.WaitWhile(() => dialog.ClickStateWait());
                    // 失敗コールバックを実行.
                    if (isFailed)
                        onFailed?.Invoke(request);
                    // エラーコールバックを実行.
                    else
                        onError?.Invoke(request);
                    

                    // 全てアンロード.
                    await UIManager.Instance.AllUnloadAsync();
                    // タイトルシーン.
                    await UIManager.Instance.LoadAsync(Instance.titleName);
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
        /// <returns>クエリパラメータ</returns>
        string QueryParamConvertDictionaryToString(Dictionary<string, object> queryParams)
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
        /// <returns>クエリパラメータ</returns>
        string QueryParamConvertObjectArrayToString(object[] queryParams)
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

        /// <summary>
        /// リクエストデータ取得
        /// </summary>
        /// <typeparam name="T">class</typeparam>
        /// <param name="postData">ポストデータ</param>
        /// <param name="iv">初期化ベクトル(公開)</param>
        /// <returns>リクエストデータ</returns>
        byte[] GetRequestData<T>(T postData, string iv) where T : class
        {
            // 1. Class Instance -> Json
            var json = JsonUtility.ToJson(postData);
            // アクセストークンがあったら最後の文字を削除し、access_tokenを送る
            if (!Instance.token.IsNullOrEmpty())
                json = $"{json[..^1]},\"access_token\":\"{Instance.token}\"}}";

#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            Debug.Log($"postData = {json}");
#endif
            // 2. Json -> Gzip
            var gzip = GZipBase64.Compress(json);

            // 3. GZip -> AES
            var aes = AES128Base64.Encode(gzip, key, iv);

            // 4. AES -> Binary
            return Encoding.UTF8.GetBytes(aes);
        }

        /// <summary>
        /// レスポンスデータ取得
        /// </summary>
        /// <typeparam name="TData">Data</typeparam>
        /// <typeparam name="TRequest">RequestData</typeparam>
        /// <typeparam name="TResponse">ResponseData</typeparam>
        /// <param name="request">UnityWebRequest</param>
        /// <returns>レスポンスデータ</returns>
        TData GetResponseData<TData, TRequest, TResponse>(UnityWebRequest request)
        where TData : APIData<TData, TRequest, TResponse>, new()
        where TRequest : APIData<TData, TRequest, TResponse>.RequestBase
        where TResponse : APIData<TData, TRequest, TResponse>.ResponseBase
        {
            // 1. AES -> GZip
            var aes = AES128Base64.Decode(request.downloadHandler.text, key, request.GetResponseHeader("iv"));
            // 2. GZip -> Json
            var gzip = GZipBase64.Decompress(aes);
            // 3. Json -> Class Instance
            return JsonUtility.FromJson<TData>(gzip);
        }

        /// <summary>
        /// アクセストークン設定
        /// </summary>
        /// <param name="accessToken">アクセストークン</param>
        public void SetAccessToken(string accessToken)
        {
            token = accessToken;
        }
    }
}