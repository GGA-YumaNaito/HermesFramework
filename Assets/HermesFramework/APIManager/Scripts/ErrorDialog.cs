using Cysharp.Threading.Tasks;
using Hermes.Localize;
using Hermes.UI;
using TMPro;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// エラーダイアログ
    /// </summary>
    public class ErrorDialog : Dialog
    {
        public override bool IsBack { get; protected set; } = false;

        /// <summary>タイトル</summary>
        [SerializeField] TextMeshProUGUI titleText = null;
        /// <summary>本文</summary>
        [SerializeField] TextMeshProUGUI bodyText = null;
        /// <summary>エラー</summary>
        [SerializeField] TextMeshProUGUI errorText = null;
        /// <summary>リトライボタン</summary>
        [SerializeField] GameObject retryButton = null;
        /// <summary>ボタンテキスト1</summary>
        [SerializeField] TextMeshProUGUI buttonText1 = null;
        /// <summary>ボタンテキスト2</summary>
        [SerializeField] TextMeshProUGUI buttonText2 = null;
        /// <summary>LocalizeKey リトライボタンテキスト</summary>
        [SerializeField] string retryButtonTextKey = "RETRY";
        /// <summary>LocalizeKey タイトルボタンテキスト</summary>
        [SerializeField] string titleButtonTextKey = "TITLE";

        /// <summary>Click State</summary>
        public eClickState ClickState { get; protected set; } = eClickState.None;

        /// <summary>オプション</summary>
        Options options = new Options();

        /// <summary>
        /// オプション
        /// </summary>
        public class Options
        {
            /// <summary>TitleKey</summary>
            public string TitleKey;
            /// <summary>BodyKey</summary>
            public string BodyKey;
            /// <summary>Error</summary>
            public string Error;
            /// <summary>IsRetry</summary>
            public bool IsRetry;
        }

        /// <summary>
        /// Click State
        /// </summary>
        public enum eClickState
        {
            /// <summary>None</summary>
            None,
            /// <summary>Wait</summary>
            Wait,
            /// <summary>Retry</summary>
            Retry,
            /// <summary>End</summary>
            End
        }

        /// <summary>
        /// 作成
        /// </summary>
        /// <param name="titleKey">タイトルKey</param>
        /// <param name="bodyKey">本文Key</param>
        /// <param name="error">エラー</param>
        /// <param name="isRetry">リトライ</param>
        public static async UniTask<ErrorDialog> Create(
            string titleKey = null,
            string bodyKey = null,
            string error = null,
            bool isRetry = false)
        {
            await UIManager.Instance.LoadAsync<ErrorDialog>(new Options() { TitleKey = titleKey, BodyKey = bodyKey, Error = error, IsRetry = isRetry });
            await UniTask.WaitUntil(() => UIManager.Instance.CurrentView.GetType() == typeof(ErrorDialog));
            return (ErrorDialog)UIManager.Instance.CurrentView;
        }

        public override UniTask OnLoad(object options)
        {
            this.options = options as Options;
            titleText.SetTextLocalizeOrInactive(this.options.TitleKey);
            bodyText.SetTextLocalizeOrInactive(this.options.BodyKey);
            errorText.SetTextLocalizeOrInactive(this.options.Error);
            retryButton.SetActive(this.options.IsRetry);
            if (this.options.IsRetry)
            {
                buttonText1.SetTextLocalize(retryButtonTextKey);
                buttonText2.SetTextLocalize(titleButtonTextKey);
            }
            else
            {
                buttonText1.SetTextLocalize(titleButtonTextKey);
                buttonText2.gameObject.SetActive(false);
            }
            ClickState = eClickState.Wait;
            return base.OnLoad(options);
        }

        /// <summary>
        /// ボタンクリック
        /// </summary>
        [EnumAction(typeof(eClickState))]
        public void OnClickButton(int state)
        {
            if (options.IsRetry)
                ClickState = (eClickState)state;
            else
                ClickState = eClickState.End;
        }

        /// <summary>
        /// ClickStateWait
        /// </summary>
        /// <returns>ClickStateがWaitならtrue</returns>
        public bool ClickStateWait() => ClickState == eClickState.Wait;
    }
}