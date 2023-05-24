using Hermes.Master;

namespace Master
{
    /// <summary>
    /// StageMasterData
    /// </summary>
    [System.Serializable]
    public class StageMasterData : MasterDataBase
    {
        /// <summary>名前</summary>
        public string StageName;
        /// <summary>エリアID</summary>
        public int AreaId;
        /// <summary>ナンバー</summary>
        public int Number;
        /// <summary>ステージタイプ</summary>
        public eStageType StageType;
        /// <summary>使用ライフ数</summary>
        public int UseLifeNum;
        /// <summary>画像アドレス</summary>
		public string ImageKey;
    }
}
