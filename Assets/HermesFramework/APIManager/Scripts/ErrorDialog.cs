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
            /// <summary>Title</summary>
            public string Title;
            /// <summary>Body</summary>
            public string Body;
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
        /// <param name="title">タイトル</param>
        /// <param name="body">本文</param>
        /// <param name="error">エラー</param>
        /// <param name="isRetry">リトライ</param>
        public static async UniTask<ErrorDialog> Create(
            string title = null,
            string body = null,
            string error = null,
            bool isRetry = false,
            string buttonText1 = null,
            string buttonText2 = null)
        {
            await UIManager.Instance.LoadAsync<ErrorDialog>(new Options() { Title = title, Body = body, Error = error, IsRetry = isRetry });
            await UniTask.WaitUntil(() => UIManager.Instance.CurrentView.GetType() == typeof(ErrorDialog));
            return (ErrorDialog)UIManager.Instance.CurrentView;
        }

        public override UniTask OnLoad(object options)
        {
            this.options = options as Options;
            titleText.SetTextLocalizeOrInactive(this.options.Title);
            bodyText.SetTextLocalizeOrInactive(this.options.Body);
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
                this.ClickState = (eClickState)state;
            else
                this.ClickState = eClickState.End;
        }

        /// <summary>
        /// ClickStateWait
        /// </summary>
        /// <returns>ClickStateがWaitならtrue</returns>
        public bool ClickStateWait() => ClickState == eClickState.Wait;
    }
}