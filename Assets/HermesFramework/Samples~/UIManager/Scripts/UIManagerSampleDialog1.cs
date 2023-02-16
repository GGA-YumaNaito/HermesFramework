using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSampleDialog1 : Dialog
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>オプション</summary>
        public class Options
        {
            public string sumpleText;
        }
        public override void OnLoad(object options)
        {
            base.OnLoad(options);
            var op = options as Options;
            if (op != null)
            {
                Debug.Log(op.sumpleText);
            }
        }

        public async void OnClickSampleDialog2()
        {
            await UIManager.Instance.LoadAsync<UIManagerSampleDialog2>(new UIManagerSampleDialog2.Options() { sumpleText = "UIManagerSampleDialog2" });
        }
    }
}
