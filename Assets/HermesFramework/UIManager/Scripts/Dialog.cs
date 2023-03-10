namespace Hermes.UI
{
    /// <summary>
    /// ダイアログ画面
    /// </summary>
    public abstract class Dialog : ViewBase
    {
        // ダイアログは基本Back出来る
        public override bool IsBack { get; protected set; } = true;
    }
}