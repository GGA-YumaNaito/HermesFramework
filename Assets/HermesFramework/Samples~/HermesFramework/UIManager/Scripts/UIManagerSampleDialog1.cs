using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSampleDialog1 : Dialog
    {
        /// <summary>オプション</summary>
        public class Options
        {
            public string sumpleText;
        }
        public override async UniTask OnLoad(object options)
        {
            var op = options as Options;
            if (op != null)
            {
                Debug.Log(op.sumpleText);
            }
            await UniTask.CompletedTask;
        }

        public async void OnClickSampleDialog2()
        {
            await UIManager.Instance.LoadAsync<UIManagerSampleDialog2>(new UIManagerSampleDialog2.Options() { sumpleText = "UIManagerSampleDialog2" }, this.GetCancellationTokenOnDestroy());
        }
    }
}
