using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Hermes.UI.Sample
{
    public class UIManagerSampleDialog2 : Dialog
    {
        public override bool IsBack { get; protected set; } = true;

        /// <summary>Camera</summary>
        new Camera camera = null;

        /// <summary>CameraData</summary>
        UniversalAdditionalCameraData cameraData = null;

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

            if (!camera)
                camera = Camera.main;
            if (!cameraData)
                cameraData = camera.GetUniversalAdditionalCameraData();
        }

        async UniTask SetCamera<T>() where T : Screen
        {
            await UniTask.WaitUntil(() => FindObjectOfType<T>());
            if (cameraData.renderType == CameraRenderType.Base)
                cameraData.renderType = CameraRenderType.Overlay;
            FindObjectOfType<T>().AddCameraStack(camera);
        }

        public async void OnClickSample1()
        {
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync<UIManagerSample1>(true, new UIManagerSample1.Options() { sumpleText = "UIManagerSample1" }),
                SetCamera<UIManagerSample1>()
                );
        }

        public async void OnClickSample2()
        {
            string name = "UIManagerSample2";
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync<UIManagerSample2>(name, new UIManagerSample2.Options() { sumpleText = "UIManagerSample2" }),
                SetCamera<UIManagerSample2>()
                );
        }

        public async void OnClickSample3()
        {
            string viewType = "Hermes.UI.Sample.UIManagerSample3, Assembly-CSharp";
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync(viewType, new UIManagerSample3.Options() { sumpleText = "UIManagerSample3" }),
                SetCamera<UIManagerSample3>()
                );
        }
    }
}
