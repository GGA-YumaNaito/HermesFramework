using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.UI.Sample
{
    public class UIManagerSampleDialog2 : Dialog
    {
        public override bool IsBack { get; protected set; } = true;

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
        public async void OnClickSample1()
        {
            await UIManager.Instance.LoadAsync<UIManagerSample1>(true, new UIManagerSample1.Options() { sumpleText = "UIManagerSample1" });
        }

        public async void OnClickSample2()
        {
            string name = "Hermes.UI.Sample.UIManagerSample2, Assembly-CSharp";
            await UIManager.Instance.LoadAsync(name, new UIManagerSample2.Options() { sumpleText = "UIManagerSample2" });
        }

        public async void OnClickSample3()
        {
            await UIManager.Instance.LoadAsync<UIManagerSample3>(new UIManagerSample3.Options() { sumpleText = "UIManagerSample3" });
        }
    }
}
