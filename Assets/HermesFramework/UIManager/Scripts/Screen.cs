using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Hermes.UI
{
    /// <summary>
    /// スクリーン画面
    /// </summary>
    public abstract class Screen : ViewBase
    {
        /// <summary>退出アニメーションを行うかフラグ</summary>
        protected virtual bool IsDisableTransition { get; set; } = false;

        /// <summary>Camera</summary>
        [SerializeField] new Camera camera = null;

        /// <summary>CameraData</summary>
        UniversalAdditionalCameraData cameraData = null;

        /// <summary>
        /// カメラのStackに追加する
        /// </summary>
        /// <param name="camera">Camera</param>
        public void AddCameraStack(Camera camera)
        {
            // カメラがnullだったらreturn
            if (camera == null)
                return;
            if (!cameraData)
                cameraData = this.camera.GetUniversalAdditionalCameraData();
            // カメラが存在していたら、除外してから追加
            if (cameraData.cameraStack.Contains(camera))
                cameraData.cameraStack.Remove(camera);
            cameraData.cameraStack.Add(camera);
        }

        /// <summary>
        /// カメラのStackから除外する
        /// </summary>
        /// <param name="camera">Camera</param>
        public void RemoveCameraStack(Camera camera)
        {
            // カメラがnullだったらreturn
            if (camera == null)
                return;
            if (!cameraData)
                cameraData = this.camera.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Remove(camera);
        }

        protected override async UniTask OnDisableAnimation()
        {
            if (IsDisableTransition)
                await base.OnDisableAnimation();
        }
    }
}