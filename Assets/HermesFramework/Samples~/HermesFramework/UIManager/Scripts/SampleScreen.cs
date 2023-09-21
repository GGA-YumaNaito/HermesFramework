using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Hermes.UI.Sample
{
    public class SampleScreen : MonoBehaviour
    {
        [SerializeField] string otherScreenName;

        /// <summary>Camera</summary>
        new Camera camera = null;

        /// <summary>CameraData</summary>
        UniversalAdditionalCameraData cameraData = null;

        void Start()
        {
            // 無理やりカメラを追加している
            if (!camera)
                camera = Camera.main;
            if (!cameraData)
                cameraData = camera.GetUniversalAdditionalCameraData();
            if (cameraData.renderType == CameraRenderType.Overlay)
                cameraData.renderType = CameraRenderType.Base;
        }

        async UniTask SetCamera<T>() where T : Screen
        {
            T screenType = null;
            while (true)
            {
                if (screenType == null)
                    screenType = FindObjectOfType<T>();
                if (screenType == null)
                {
                    await UniTask.NextFrame();
                    continue;
                }
                if (screenType.Status.Value == eStatus.None || screenType.Status.Value == eStatus.Enable || screenType.Status.Value == eStatus.Display)
                    break;
            }
            if (cameraData.renderType == CameraRenderType.Base)
                cameraData.renderType = CameraRenderType.Overlay;
            screenType.AddCameraStack(camera);
        }

        public async void OnClickSample1()
        {
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync<UIManagerSample1>(new UIManagerSample1.Options() { sumpleText = "UIManagerSample1" }, this.GetCancellationTokenOnDestroy()),
                SetCamera<UIManagerSample1>()
                );
        }

        public async void OnClickSample2()
        {
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync<UIManagerSample2>(new UIManagerSample2.Options() { sumpleText = "UIManagerSample2" }, this.GetCancellationTokenOnDestroy()),
                SetCamera<UIManagerSample2>()
                );
        }

        public async void OnClickSample3()
        {
            await UniTask.WhenAll(
                UIManager.Instance.LoadAsync<UIManagerSample3>(new UIManagerSample3.Options() { sumpleText = "UIManagerSample3" }, this.GetCancellationTokenOnDestroy()),
                SetCamera<UIManagerSample3>()
                );
        }

        public async void OnClickSampleDialog1()
        {
            await UIManager.Instance.LoadAsync<UIManagerSampleDialog1>(new UIManagerSampleDialog1.Options() { sumpleText = "UIManagerSampleDialog1" }, this.GetCancellationTokenOnDestroy());
        }

        public async void OnClickBackButton()
        {
            await UniTask.WhenAll(
                UniTask.Create(async () =>
                {
                    if (UIManager.Instance.GetStackCount() == 1 || !UIManager.Instance.CurrentView.IsBack)
                        return UniTask.CompletedTask;

                    await UniTask.NextFrame();

                    await UniTask.WaitWhile(() => UIManager.Instance.CurrentScene != null);

                    Screen screenType = null;
                    await UniTask.WaitWhile(() =>
                    {
                        screenType = FindObjectOfType<UIManagerSample1>();
                        if (screenType != null) return false;
                        screenType = FindObjectOfType<UIManagerSample2>();
                        if (screenType != null) return false;
                        screenType = FindObjectOfType<UIManagerSample3>();
                        if (screenType != null) return false;
                        screenType = FindObjectOfType<UIManagerSampleOther>();
                        if (screenType != null) return false;
                        return true;
                    });
                    if (screenType.GetType() == typeof(UIManagerSample1))
                        return SetCamera<UIManagerSample1>();
                    else if (screenType.GetType() == typeof(UIManagerSample2))
                        return SetCamera<UIManagerSample2>();
                    else if (screenType.GetType() == typeof(UIManagerSample3))
                        return SetCamera<UIManagerSample3>();
                    else if (screenType.GetType() == typeof(UIManagerSampleOther))
                        return SetCamera<UIManagerSampleOther>();
                    else
                        return UniTask.CompletedTask;
                }),
                UIManager.Instance.BackAsync(this.GetCancellationTokenOnDestroy())
                );
        }

        public void OnClickClearStackLeaveOneButton()
        {
            UIManager.Instance.ClearStackLeaveOne();
        }

        public async void OnClickReloadSceneButton()
        {
            await UIManager.Instance.ReloadSceneAsync();
        }

        public async void OnClickSampleOther()
        {
            if (otherScreenName.IsNullOrEmpty())
                await UniTask.WhenAll(
                    UIManager.Instance.LoadAsync<UIManagerSampleOther>(new UIManagerSampleOther.Options() { sumpleText = "UIManagerSampleOther" }, this.GetCancellationTokenOnDestroy()),
                    SetCamera<UIManagerSampleOther>()
                    );
            else
                await UniTask.WhenAll(
                    UIManager.Instance.LoadAsync<UIManagerSampleOther>(otherScreenName, new UIManagerSampleOther.Options() { sumpleText = $"UIManagerSampleOther : {otherScreenName}" }, this.GetCancellationTokenOnDestroy()),
                    SetCamera<UIManagerSampleOther>()
                    );
        }
    }
}
