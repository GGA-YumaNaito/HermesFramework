using Cysharp.Threading.Tasks;
using Hermes.UI;
using Hermes.Prefs;
using Title;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boot
{
    /// <summary>
    /// 起動シーン
    /// </summary>
    public class BootScene : MonoBehaviour
    {
        /// <summary>起動キー</summary>
        static readonly string bootedKey = "Booted";

        // Start is called before the first frame update
        async void Start()
        {
            // 起動したことがあるか
            var booted = PrefsManager.Instance.Load<bool>(bootedKey);
            if (booted)
            {
                // TitleScene呼び出し
                await UIManager.Instance.LoadAsync<TitleScene>(new TitleScene.Options() { }, this.GetCancellationTokenOnDestroy());
            }
            else
            {
                // TODO: 本来はプッシュ通知や諸々の設定を行うための画面に飛ばすし、多分ローカルじゃなくてサーバーでやるんじゃないかな？
                // 初回起動画面呼び出し
                await UIManager.Instance.LoadAsync<TitleScene>(new TitleScene.Options() { }, this.GetCancellationTokenOnDestroy());
                // TODO: 本来は初回起動画面でセーブする
                // 保存
                PrefsManager.Instance.Set(bootedKey, true);
                // セーブ
                PrefsManager.Instance.Save();
            }

            await SceneManager.UnloadSceneAsync(GetType().Name);
        }
    }
}
