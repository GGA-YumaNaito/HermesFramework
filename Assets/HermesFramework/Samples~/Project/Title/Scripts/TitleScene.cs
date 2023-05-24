using Cysharp.Threading.Tasks;
using Hermes.Localize;
using Hermes.Master;
using Hermes.Sound;
using Hermes.UI;
using Home;
using UnityEngine;

namespace Title
{
    /// <summary>
    /// タイトルシーン
    /// </summary>
    public class TitleScene : Hermes.UI.Screen
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>BGM Name</summary>
        [SerializeField] string bgmName = "";

        /// <summary>オプション</summary>
        public class Options
        {
        }

        // Start is called before the first frame update
        async void Start()
        {
            // TODO:マスターロードやHome画面へ遷移するのは本来画面をクリックした後
            await UniTask.WhenAll(
                // ローカライズの初期化が終わるまで待機
                UniTask.WaitUntil(() => LocalizeManager.Instance.IsInitializad),
                // マスターロード
                MasterManager.Instance.Load()
            );

            // TODO:仮で現在の使用言語が英語だった場合、日本語にする
            if (LocalizeManager.Instance.SelectedCode == "en")
                await LocalizeManager.Instance.SetLocale("ja");

            // BGM呼び出し
            SoundManager.BGM.Play(bgmName);

            // HomeScene呼び出し
            await UIManager.Instance.LoadAsync<HomeScene>(new HomeScene.Options() { });
        }
    }
}
