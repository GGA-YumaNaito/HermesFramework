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

        /// <summary>Click State</summary>
        public eClickState ClickState { get; protected set; } = eClickState.None;

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
        public static async UniTask<ErrorDialog> Create(string title = null, string body = null, string error = null, bool isRetry = false)
        {
            await UIManager.Instance.LoadAsync<ErrorDialog>(new Options() { Title = title, Body = body, Error = error, IsRetry = isRetry });
            await UniTask.WaitUntil(() => UIManager.Instance.CurrentView.GetType() == typeof(ErrorDialog));
            return (ErrorDialog)UIManager.Instance.CurrentView;
        }

        public override UniTask OnLoad(object options)
        {
            var op = options as Options;
            SetOrInactive(titleText, op.Title);
            SetOrInactive(bodyText, op.Body);
            SetOrInactive(errorText, op.Error);
            retryButton.SetActive(op.IsRetry);
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
            this.ClickState = (eClickState)state;
        }

        /// <summary>
        /// ClickStateWait
        /// </summary>
        /// <returns>ClickStateがWaitならtrue</returns>
        public bool ClickStateWait() => ClickState == eClickState.Wait;
    }
}