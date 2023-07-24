using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Hermes.API.Sample
{
    /// <summary>
    /// APIManagerSample Scene
    /// </summary>
    public class APIManagerSampleScene : UI.Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>APIManagerを使用するか</summary>
        [SerializeField] bool isApiManager;
        /// <summary>POSTかGetか</summary>
        [SerializeField] bool isPost;
        /// <summary>URL</summary>
        [SerializeField] string url;
        /// <summary>TMP_Dropdown</summary>
        [SerializeField] TMP_Dropdown dropdown;
        /// <summary>RequestList</summary>
        [SerializeField] RequestList requestList;

        /// <summary>データTypes</summary>
        List<Type> dataTypes = new();
        /// <summary>レスポンスTypes</summary>
        List<Type> responseTypes = new();
        /// <summary>リクエストTypes</summary>
        List<Type> requestTypes = new();
        /// <summary>Data Type</summary>
        Type dataType;
        /// <summary>Response Type</summary>
        Type responseType;
        /// <summary>Request type</summary>
        Type requestType;
        /// <summary>Request instance</summary>
        object requestInstance;
        /// <summary>SendWebRequest method</summary>
        MethodInfo sendWebRequestMethod;

        static bool isOne = false;

        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="options"></param>
        public override UniTask OnLoad(object options)
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// スタート
        /// </summary>
        async void Start()
        {
            if (!isOne)
            {
                isOne = true;
                await SceneManager.UnloadSceneAsync(GetType().Name);
                await UI.UIManager.Instance.LoadAsync<APIManagerSampleScene>();
                return;
            }
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                dataTypes.AddRange(assembly.GetTypes()
                    .Where(p => p.BaseType != null && p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(APIData<,,>))
                    .OrderBy(o => o.Name)
                    .ToList());
                requestTypes.AddRange(assembly.GetTypes()
                    .Where(p => dataTypes.Find(x => p.FullName.Contains(x.FullName + "+Request")) != null)
                    .OrderBy(o => o.Name)
                    .ToList());
                responseTypes.AddRange(assembly.GetTypes()
                    .Where(p => dataTypes.Find(x => p.FullName.Contains(x.FullName + "+Response")) != null)
                    .OrderBy(o => o.Name)
                    .ToList());
            }
            dataTypes = dataTypes.OrderBy(o => o.Name).ToList();
            requestTypes = requestTypes.OrderBy(o => o.Name).ToList();
            responseTypes = responseTypes.OrderBy(o => o.Name).ToList();

            dataType = dataTypes[0];
            requestType = requestTypes.Find(x => x.FullName.Contains(dataType.Name));
            responseType = responseTypes.Find(x => x.FullName.Contains(dataType.Name));

            requestInstance = Activator.CreateInstance(requestType);

            requestList.Initialized();

            requestList.SetItem(requestType);

            var stringTypes = dataTypes.Select(x => x.Name).ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(stringTypes);
            dropdown.onValueChanged.AddListener(_ => DropdownValueChanged(dropdown));

            //var paramActionType = typeof(Action<>).MakeGenericType(dataType);

            var methods = typeof(APIManager).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(x => x.Name == "SendWebRequest" && x.IsGenericMethod);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 5 &&
                    //requestType.IsClass &&
                    //parameters[1].ParameterType == paramActionType &&
                    //parameters[2].ParameterType == paramActionType &&
                    parameters[3].ParameterType == typeof(bool) &&
                    parameters[4].ParameterType == typeof(bool))
                {
                    sendWebRequestMethod = method;
                    break;
                }
            }
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <returns>UniTask</returns>
        async UniTask PostText()
        {
            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                // start and wait
                var ao = request.SendWebRequest();

                await UniTask.WaitWhile(() => !ao.isDone);

                // 通信失敗
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error : {request.error}");
                }
                // 通信成功
                else
                {
                    Debug.Log(request.downloadHandler.text);
                }
            }
        }

        /// <summary>
        /// POST通信
        /// </summary>
        /// <returns>UniTask</returns>
        async UniTask GetText()
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();

                var ao = request.SendWebRequest();

                await UniTask.WaitWhile(() => !ao.isDone);

                // 通信失敗
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error : {request.error}");
                }
                // 通信成功
                else
                {
                    Debug.Log(request.downloadHandler.text);

                    // または、バイナリデータとして結果を取得
                    byte[] results = request.downloadHandler.data;
                }
            }
        }

        /// <summary>
        /// ドロップダウンが変更された時の処理
        /// </summary>
        /// <param name="change">TMP_Dropdown</param>
        void DropdownValueChanged(TMP_Dropdown change)
        {
            dataType = dataTypes[change.value];
            requestType = requestTypes.Find(x => x.FullName.Contains(dataType.Name));
            responseType = responseTypes.Find(x => x.FullName.Contains(dataType.Name));

            requestInstance = Activator.CreateInstance(requestType);

            requestList.SetItem(requestType);
        }

        /// <summary>
        /// クリック処理
        /// </summary>
        public async void OnClick()
        {
            if (isApiManager)
            {
                requestList.SetInstance(requestInstance);

                var uniTask = sendWebRequestMethod
                    .MakeGenericMethod(dataType, requestType, responseType)
                    .Invoke(APIManager.Instance, new object[] { requestInstance, null, null, true, true });

                await (UniTask)uniTask;
            }
            else if (isPost)
                await PostText();
            else
                await GetText();
        }
    }
}