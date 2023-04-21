using Cysharp.Threading.Tasks;

namespace Hermes.UI
{
    /// <summary>
    /// スクリーン画面
    /// </summary>
    public abstract class Screen : ViewBase
    {
        /// <summary>退出アニメーションを行うかフラグ</summary>
        protected virtual bool IsDisableTransition { get; set; } = false;

        protected override async UniTask OnDisableAnimation()
        {
            if (IsDisableTransition)
                await base.OnDisableAnimation();
        }
    }
}