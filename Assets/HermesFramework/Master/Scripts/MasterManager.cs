using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

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

        /// <summary>
        /// マスター読み込みメソッド
        /// </summary>
        /// <returns>UniTask</returns>
        public async UniTask Load()
        {
            IsLoaded = false;

            var masterList = await Addressables.LoadAssetsAsync<UnityEngine.Object>(masterLabel, null);

            foreach (var master in masterList)
            {
                // タイプ取得
                var type = master.GetType();
                // メソッド取得(継承元で定義されたメソッドは持って来れないらしいので、BaseTypeから持ってきてる)
                var addListToRuntimeMasterInfo = type.BaseType.GetMethod("AddListToRuntimeMaster", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                // リスト取得
                var list = type.InvokeMember(
                    "List",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.GetField,
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
    }
}