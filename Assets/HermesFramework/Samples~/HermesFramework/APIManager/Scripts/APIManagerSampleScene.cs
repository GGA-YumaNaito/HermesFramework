using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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

        /// <summary>Assembly</summary>
        Assembly assembly;
        /// <summary>Types</summary>
        List<Type> types;
        /// <summary>Type</summary>
        Type type;
        /// <summary>Request type</summary>
        Type requestType;
        /// <summary>Request instance</summary>
        object requestInstance;
        /// <summary>SendWebRequest method</summary>
        MethodInfo sendWebRequestMethod;

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
        void Start()
        {
            assembly = Assembly.GetExecutingAssembly();

            types = assembly.GetTypes()
                .Where(p => p.BaseType.IsGenericType && p.BaseType.GetGenericTypeDefinition() == typeof(APIData<>))
                .OrderBy(o => o.Name)
                .ToList();

            type = types[0];
            requestType = assembly.GetType($"{type.FullName}+Request");

            requestInstance = Activator.CreateInstance(requestType);

            requestList.Initialized();

            requestList.SetItem(requestType);

            var stringTypes = types.Select(x => x.Name).ToList();

            dropdown.ClearOptions();
            dropdown.AddOptions(stringTypes);
            dropdown.onValueChanged.AddListener(_ => DropdownValueChanged(dropdown));

            var paramActionType = typeof(Action<>).MakeGenericType(type);

            var methods = typeof(APIManager).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.Name == "SendWebRequest" && x.IsGenericMethod);
            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 6 &&
                    //requestType.IsClass &&
                    //parameters[1].ParameterType == paramActionType &&
                    //parameters[2].ParameterType == paramActionType &&
                    parameters[3].ParameterType == typeof(bool) &&
                    parameters[4].ParameterType == typeof(bool) &&
                    parameters[5].ParameterType == typeof(ulong))
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
            type = types[change.value];
            requestType = assembly.GetType($"{type.FullName}+Request");

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
                    .MakeGenericMethod(requestType, type)
                    .Invoke(type, new object[] { requestInstance, null, null, true, true, 0ul });

                await (UniTask)uniTask;
            }
            else if (isPost)
                await PostText();
            else
                await GetText();
        }
    }
}