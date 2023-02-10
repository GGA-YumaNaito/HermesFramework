using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSampleDialog2 : Dialog
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>ƒIƒvƒVƒ‡ƒ“</summary>
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
        public async void OnClickSample1()
        {
            await UIManager.Instance.LoadAsync<UIManagerSample1>(new UIManagerSample1.Options() { sumpleText = "UIManagerSample1" });
        }

        public async void OnClickSample2()
        {
            await UIManager.Instance.LoadAsync<UIManagerSample2>(new UIManagerSample2.Options() { sumpleText = "UIManagerSample2" });
        }

        public async void OnClickSample3()
        {
            await UIManager.Instance.LoadAsync<UIManagerSample3>(new UIManagerSample3.Options() { sumpleText = "UIManagerSample3" });
        }
    }
}
