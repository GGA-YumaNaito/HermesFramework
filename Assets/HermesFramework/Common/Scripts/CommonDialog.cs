using System;
using Cysharp.Threading.Tasks;
using Hermes.Localize;
using TMPro;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// 汎用ダイアログ
    /// </summary>
    public class CommonDialog : Dialog
    {
        /// <summary>タイトル</summary>
        [SerializeField] TextMeshProUGUI titleText = null;
        /// <summary>本文</summary>
        [SerializeField] TextMeshProUGUI bodyText = null;
        /// <summary>ボタンテキスト1</summary>
        [SerializeField] TextMeshProUGUI buttonText1 = null;
        /// <summary>ボタン2</summary>
        [SerializeField] GameObject button2 = null;
        /// <summary>ボタンテキスト2</summary>
        [SerializeField] TextMeshProUGUI buttonText2 = null;
        /// <summary>LocalizeKey OKボタンテキスト</summary>
        [SerializeField] string okButtonTextKey = "OK";
        /// <summary>LocalizeKey Yesボタンテキスト</summary>
        [SerializeField] string yesButtonTextKey = "YES";
        /// <summary>LocalizeKey NOボタンテキスト</summary>
        [SerializeField] string noButtonTextKey = "NO";

        /// <summary>クリック時のアクション</summary>
        Action onClickAction = null;

        /// <summary>
        /// オプション
        /// </summary>
        public class Options
        {
            /// <summary>Title</summary>
            public string Title;
            /// <summary>Body</summary>
            public string Body;
            /// <summary>ボタンタイプ</summary>
            public eButtonType ButtonType;
            /// <summary>クリック時のアクション</summary>
            public Action OnClickAction;
        }

        /// <summary>
        /// ボタンタイプ
        /// </summary>
        public enum eButtonType
        {
            /// <summary>OK</summary>
            OK,
            /// <summary>Yes or No</summary>
            YesOrNo,
        }

        /// <summary>
        /// Click State
        /// </summary>
        public enum eClickState
        {
            /// <summary>OK or Yes</summary>
            OkOrYes,
            /// <summary>No</summary>
            No
        }

        /// <summary>
        /// 作成
        /// </summary>
        /// <param name="title">タイトル</param>
        /// <param name="body">本文</param>
        /// <param name="buttonType">ボタンタイプ</param>
        /// <param name="onClickAction">クリック時のアクション</param>
        public static async UniTask<CommonDialog> Create(string title = null, string body = null, eButtonType buttonType = eButtonType.OK, Action onClickAction = null)
        {
            await UIManager.Instance.LoadAsync<CommonDialog>(new Options() { Title = title, Body = body, ButtonType = buttonType });
            await UniTask.WaitUntil(() => UIManager.Instance.CurrentView.GetType() == typeof(CommonDialog));
            return (CommonDialog)UIManager.Instance.CurrentView;
        }

        public override UniTask OnLoad(object options)
        {
            var op = options as Options;
            titleText.SetTextLocalize(op.Title);
            bodyText.SetTextLocalize(op.Body);
            if (op.ButtonType == eButtonType.OK)
            {
                buttonText1.SetTextLocalize(okButtonTextKey);
                button2.SetActive(false);
            }
            else
            {
                buttonText1.SetTextLocalize(yesButtonTextKey);
                buttonText2.SetTextLocalize(noButtonTextKey);
            }
            onClickAction = op.OnClickAction;
            return base.OnLoad(options);
        }

        /// <summary>
        /// ボタンのクリック制御
        /// </summary>
        [EnumAction(typeof(eClickState))]
        public async void OnClickButton(int state)
        {
            if (((eClickState)state) == eClickState.OkOrYes)
                onClickAction?.Invoke();
            await UIManager.Instance.BackAsync();
        }
    }
}