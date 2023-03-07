namespace Hermes.Sound
{
    /// <summary>
    /// 再生タイプ
    /// </summary>
    public enum ePlayType
    {
        /// <summary>同じSEを制限数まで鳴らす</summary>
        Play,
        /// <summary>同じSEが鳴っていたら止めてから鳴らす</summary>
        StopPlus,
        /// <summary>同じSEが鳴っていない場合だけ鳴らす</summary>
        IfNot
    }
}