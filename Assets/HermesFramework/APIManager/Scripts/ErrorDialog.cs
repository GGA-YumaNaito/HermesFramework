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
            SetOrInactive(titleText, this.options.Title);
            SetOrInactive(bodyText, this.options.Body);
            SetOrInactive(errorText, this.options.Error);
            retryButton.SetActive(this.options.IsRetry);
            if (this.options.IsRetry)
            {
                buttonText1.text = LocalizeManager.Instance.GetValue(retryButtonTextKey);
                buttonText2.text = LocalizeManager.Instance.GetValue(titleButtonTextKey);
            }
            else
            {
                buttonText1.text = LocalizeManager.Instance.GetValue(titleButtonTextKey);
                buttonText2.gameObject.SetActive(false);
            }
            ClickState = eClickState.Wait;
            return base.OnLoad(options);
        }

        /// <summary>
        /// 値があったらテキストを設定し、なかったら非活性にする
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        void SetOrInactive(TextMeshProUGUI text, string value)
        {
            if (value.IsNullOrEmpty())
                text.gameObject.SetActive(false);
            else
                text.text = LocalizeManager.Instance.GetValue(value);
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