using UnityEngine;

namespace Hermes.UI.Sample
{
    public class SampleScreen : MonoBehaviour
    {
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

        public async void OnClickSampleDialog1()
        {
            await UIManager.Instance.LoadAsync<UIManagerSampleDialog1>(new UIManagerSampleDialog1.Options() { sumpleText = "UIManagerSampleDialog1" });
        }

        public async void OnClickBackButton()
        {
            await UIManager.Instance.BackAsync();
        }

        public void OnClickClearStackLeaveOneButton()
        {
            UIManager.Instance.ClearStackLeaveOne();
        }
    }
}
