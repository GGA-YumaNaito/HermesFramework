using System;
using Cysharp.Threading.Tasks;
using Hermes.API;
using Hermes.Localize;
using Hermes.Master;
using Hermes.Save;
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

        /// <summary>iOSのUUIDのkey</summary>
        const string HERMES_SAMPLE_UUID_KEY = "HERMES_SAMPLE_UUID_KEY";
        /// <summary>iOSのPlayerIDのkey</summary>
        const string HERMES_SAMPLE_PLAYER_ID_KEY = "HERMES_SAMPLE_PLAYER_ID_KEY";
        /// <summary>iOSのUserIDのkey</summary>
        const string HERMES_SAMPLE_USER_ID_KEY = "HERMES_SAMPLE_USER_ID_KEY";

        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// スタート
        /// </summary>
        public override async void OnStart()
        {
            // TODO:仮で現在の使用言語が英語だった場合、日本語にする
            if (LocalizeManager.Instance.SelectedCode == "en")
                await LocalizeManager.Instance.SetLocale("ja");

            // BGM呼び出し
            SoundManager.BGM.Play(bgmName);

            // ユーザー名
            var user_name = "user_name_sample";

            // Androidだと、Application.persistentDataPathはアプリ外に保存する
            // UUID取得
#if !UNITY_EDITOR && UNITY_IOS
            KeychainService keychain = new KeychainService();
            var uuid = keychain.Get(HERMES_SAMPLE_UUID_KEY);
            var playerId = keychain.Get(HERMES_SAMPLE_PLAYER_ID_KEY);
#else
            var uuid = string.Empty;
            var playerId = string.Empty;
            if (SaveManager.Instance.HasFile(HERMES_SAMPLE_UUID_KEY, Application.persistentDataPath))
                uuid = SaveManager.Instance.Load<string>(HERMES_SAMPLE_UUID_KEY, Application.persistentDataPath);
            if (SaveManager.Instance.HasFile(HERMES_SAMPLE_PLAYER_ID_KEY, Application.persistentDataPath))
                playerId = SaveManager.Instance.Load<string>(HERMES_SAMPLE_PLAYER_ID_KEY, Application.persistentDataPath);
#endif
            if (uuid.IsNullOrEmpty() || playerId.IsNullOrEmpty())
            {
                uuid = Guid.NewGuid().ToString("N");
#if !UNITY_EDITOR && UNITY_IOS
                keychain.Put(HERMES_SAMPLE_UUID_KEY, uuid);
#else
                SaveManager.Instance.Save(HERMES_SAMPLE_UUID_KEY, uuid, Application.persistentDataPath);
#endif
                // 作成
                UserCreate userCreate = new()
                {
#if !UNITY_EDITOR && UNITY_IOS
                    request = new UserCreate.Request(uuid, user_name, UserCreate.eOSType.iOS, UnityEngine.iOS.Device.generation.ToString())
#else
                    request = new UserCreate.Request(uuid, user_name, UserCreate.eOSType.Android, SystemInfo.deviceModel)
#endif
                };
                await userCreate.SendWebRequest(x =>
                {
                    playerId = x.data.User.PlayerId;
                    var userId = x.data.User.PlayerId;
                    // UserId と PlayerId を保存
#if !UNITY_EDITOR && UNITY_IOS
                    keychain.Put(HERMES_SAMPLE_PLAYER_ID_KEY, playerId);
                    keychain.Put(HERMES_SAMPLE_USER_ID_KEY, userId);
#else
                    SaveManager.Instance.Save(HERMES_SAMPLE_PLAYER_ID_KEY, x.data.User.PlayerId, Application.persistentDataPath);
                    SaveManager.Instance.Save(HERMES_SAMPLE_USER_ID_KEY, x.data.User.UserId, Application.persistentDataPath);
#endif
                });
            }

            // ログイン
            UserLogin userLogin = new()
            {
                request = new UserLogin.Request(playerId, uuid)
            };
            await userLogin.SendWebRequest();
        }

        /// <summary>
        /// 画面をクリックしたら
        /// </summary>
        public async void OnClickScreen()
        {
            // バリアON
            UIManager.Instance.SetActiveBarrier(true);

            // マスターロード
            await MasterManager.Instance.Load();

            // タイトル
            API.HomeData homeData = new()
            {
                request = new API.HomeData.Request(SaveManager.Instance.Load<string>(HERMES_SAMPLE_PLAYER_ID_KEY, Application.persistentDataPath))
            };
            await homeData.SendWebRequest();

            // HomeScene呼び出し
            await UIManager.Instance.LoadAsync<HomeScene>(new HomeScene.Options() { });

            // バリアOFF
            UIManager.Instance.SetActiveBarrier(false);
        }
    }
}
