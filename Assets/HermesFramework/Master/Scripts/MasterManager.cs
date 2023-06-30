using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Hermes.Master
{
    /// <summary>
    /// MasterManager
    /// </summary>
    public class MasterManager : SingletonMonoBehaviour<MasterManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>Addressables Groupsで登録されているLabels</summary>
        [SerializeField] string masterLabel = "Master";

        /// <summary>マスターが読み込み完了しているか</summary>
        public bool IsLoaded { get; private set; } = false;

        /// <summary>マスターHandle</summary>
        AsyncOperationHandle<System.Collections.Generic.IList<Object>> handle;

        /// <summary>
        /// マスター読み込みメソッド
        /// </summary>
        /// <returns>UniTask</returns>
        public async UniTask Load()
        {
            // ロード済み
            if (IsLoaded)
                return;

            handle = Addressables.LoadAssetsAsync<Object>(masterLabel, null);
            await handle.Task;
            var masterList = handle.Result;
            System.Type type;
            System.Reflection.MethodInfo addListToRuntimeMasterInfo;
            object list;

            if (masterList == null)
                Debug.LogError($"masterList error!!!");

            foreach (var master in masterList)
            {
                // タイプ取得
                type = master.GetType();
                // メソッド取得(継承元で定義されたメソッドは持って来れないらしいので、BaseTypeから持ってきてる)
                addListToRuntimeMasterInfo = type.BaseType.GetMethod("AddListToRuntimeMaster", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                // リスト取得
                list = type.InvokeMember(
                    "List",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetProperty,
                    null,
                    master,
                    null
                    );
                // メソッド実行
                addListToRuntimeMasterInfo.Invoke(null, new object[] { list });
#if DEBUG_LOG
                Log.ObjectDumpLog.DebugLog(master);
#endif
            }

            // 読み込み完了
            IsLoaded = true;
        }

        /// <summary>
        /// マスター削除メソッド
        /// </summary>
        public void Clear()
        {
            // ロードしていなかったら
            if (!IsLoaded)
                return;

            var masterList = handle.Result;
            System.Type type;
            System.Reflection.MethodInfo clearRuntimeMasterInfo;

            foreach (var master in masterList)
            {
                // タイプ取得
                type = master.GetType();
                // メソッド取得(継承元で定義されたメソッドは持って来れないらしいので、BaseTypeから持ってきてる)
                clearRuntimeMasterInfo = type.BaseType.GetMethod("ClearRuntimeMaster", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                // メソッド実行
                clearRuntimeMasterInfo.Invoke(null, null);
#if DEBUG_LOG
                Log.ObjectDumpLog.DebugLog(master);
#endif
            }

            // リリース
            Addressables.Release(handle);
            IsLoaded = false;
        }
    }
}