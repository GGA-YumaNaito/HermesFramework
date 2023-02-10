namespace Hermes.UI
{
    /// <summary>
    /// �o�b�N�{�^��
    /// </summary>
    public class UIBackButton : UIButton
    {
        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(() => UIManager.Instance.BackAsync().NoAwait());
        }
    }
}