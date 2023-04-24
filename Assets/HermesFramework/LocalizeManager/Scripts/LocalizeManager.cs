using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Hermes.Asset;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Hermes.Localize
{
    /// <summary>
    /// ローカライズマネージャー
    /// </summary>
    public class LocalizeManager : SingletonMonoBehaviour<LocalizeManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>テーブルコレクションの名前</summary>
        [SerializeField] string tableCollectionName = string.Empty;

        /// <summary>初期化完了しているか</summary>
        public bool IsInitializad { get; private set; } = false;

        /// <summary>StringTable</summary>
        StringTable stringTable = null;
        /// <summary>現在のLocale</summary>
        Locale selectedLocale = null;

        /// <summary>現在のコード</summary>
        public string SelectedCode { get => selectedLocale.Formatter.ToString(); }
        /// <summary>コードリスト</summary>
        public List<string> CodeList { get => LocalizationSettings.AvailableLocales.Locales.Select(x => x.Formatter.ToString()).ToList(); }

        /// <summary>
        /// Start
        /// </summary>
        async void Start()
        {
            // AssetManager の初期化が完了するまで待機
            await UniTask.WaitUntil(() => AssetManager.Instance.IsInitializad);
            // Localization の初期化が完了するまで待機
            await LocalizationSettings.InitializationOperation.Task;

            // StringTable取得
            stringTable = await LocalizationSettings.StringDatabase.GetDefaultTableAsync().Task;

            // 現在のロケールを取得
            selectedLocale = LocalizationSettings.SelectedLocale;

            // ロケールが変更されたら
            LocalizationSettings.SelectedLocaleChanged += (x) =>
            {
                // 変更されたロケールを設定する
                selectedLocale = x;
                // StringTable更新
                stringTable = LocalizationSettings.StringDatabase.GetTable(tableCollectionName, x);
            };

            // 初期化完了
            IsInitializad = true;
        }

        /// <summary>
        /// ロケール設定
        /// </summary>
        /// <param name="code">locale code</param>
        /// <returns>UniTask</returns>
        public async UniTask SetLocale(string code)
        {
            // 現在のコードと同じ場合は処理をしない
            if (LocalizationSettings.SelectedLocale.Formatter.ToString() == code)
                return;

            // 設定
            var locale = LocalizationSettings.AvailableLocales.Locales.Find(x => x.Formatter.ToString() == code);
            LocalizationSettings.SelectedLocale = locale;
            // 変更されるまで待機
            await LocalizationSettings.InitializationOperation.Task;
        }

        /// <summary>
        /// データ取得
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public string GetValue(string key)
        {
            return stringTable.GetEntry(key).Value;
        }

        /// <summary>
        /// 再初期化
        /// </summary>
        /// <returns></returns>
        public async UniTask Reinitialize()
        {
            IsInitializad = false;

            // テーブルを全てReset
            LocalizationSettings.Instance.ResetState();

            // Localization の初期化が完了するまで待機
            await LocalizationSettings.InitializationOperation.Task;

            // Tableプリロード
            await LocalizationSettings.StringDatabase.PreloadTables(tableCollectionName);

            // TODO:もしかしたらSelectedLocaleChangedで自動的に入れ替えているかもしれないから、要チェック
            //// 現在のロケールを取得
            //selectedLocale = LocalizationSettings.SelectedLocale;
            //// StringTable更新
            //stringTable = LocalizationSettings.StringDatabase.GetTable(tableCollectionName, selectedLocale);

            // 初期化完了
            IsInitializad = true;
        }
    }
}