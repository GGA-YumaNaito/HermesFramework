using Cysharp.Threading.Tasks;

namespace Hermes.UI
{
    /// <summary>
    /// バックボタン
    /// </summary>
    public class UIBackButton : UIButton
    {
        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(() => UIManager.Instance.BackAsync(this.GetCancellationTokenOnDestroy()).Forget());
        }
    }
}