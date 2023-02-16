namespace Hermes.UI
{
    /// <summary>
    /// 画面の状態
    /// </summary>
    public enum eStatus
    {
        /// <summary>無</summary>
        None,
        /// <summary>画面表示遷移中</summary>
        Enable,
        /// <summary>画面表示中</summary>
        Display,
        /// <summary>画面非表示遷移中</summary>
        Disable,
        /// <summary>終了</summary>
        End
    }
}