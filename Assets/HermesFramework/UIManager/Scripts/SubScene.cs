namespace Hermes.UI
{
    /// <summary>
    /// SubScene画面
    /// </summary>
    public abstract class SubScene : ViewBase
    {
        // SubSceneに戻りはない
        public override bool IsBack { get; protected set; } = false;
    }
}