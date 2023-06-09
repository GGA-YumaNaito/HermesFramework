using Cysharp.Threading.Tasks;
using Hermes.UI;
using UnityEngine;

namespace Home
{
    /// <summary>
    /// Home Scene
    /// </summary>
    public class HomeScene : Hermes.UI.Screen
    {
        // Home画面はBack出来ないようにする
        public override bool IsBack { get; protected set; } = false;

        /// <summary>BGM Name</summary>
        [SerializeField] string bgmName = "";
        /// <summary>タイトルに戻るのタイトルキー</summary>
        [SerializeField] string backTitleTitleKey = "BACK_TITLE_TITLE";
        /// <summary>タイトルに戻るの本文キー</summary>
        [SerializeField] string backTitleBodyKey = "BACK_TITLE_BODY";

        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="options"></param>
        public override async UniTask OnLoad(object options)
        {
            // TODO: 仮のBGM
            Hermes.Sound.SoundManager.BGM.Play(bgmName);
            // Header, Footer呼び出し
            if (!UIManager.Instance.HasSubScene<Header.HeaderScene>())
            {
                await UniTask.WhenAll(
                    UIManager.Instance.SubSceneLoadAsync<Header.HeaderScene>(),
                    UIManager.Instance.SubSceneLoadAsync<Footer.FooterScene>()
                );
            }
        }

        /// <summary>
        /// アンロード
        /// </summary>
        /// <returns></returns>
        public override async UniTask OnUnload()
        {
            await UniTask.WhenAll(
                UIManager.Instance.SubSceneUnloadAsync<Header.HeaderScene>(),
                UIManager.Instance.SubSceneUnloadAsync<Footer.FooterScene>()
            );
        }

        public override async UniTask ActionInsteadOfBack()
        {
            await CommonDialog.Create(
                backTitleTitleKey,
                backTitleBodyKey,
                CommonDialog.eButtonType.YesOrNo,
                async () => {
                    await UIManager.Instance.LoadAsync<Title.TitleScene>();
                });
        }
    }
}
