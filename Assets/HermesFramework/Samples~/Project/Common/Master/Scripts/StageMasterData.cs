using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// StageMasterData
    /// </summary>
    [System.Serializable]
    public class StageMasterData : MasterDataBase
    {
        [SerializeField] string stage_name;
        /// <summary>名前</summary>
        public string StageName { get => stage_name; }

        [SerializeField] int area_id;
        /// <summary>エリアID</summary>
        public int AreaId { get => area_id; }

        [SerializeField] int number;
        /// <summary>ナンバー</summary>
        public int Number { get => number; }

        [SerializeField] eStageType stage_type;
        /// <summary>ステージタイプ</summary>
        public eStageType StageType { get => stage_type; }

        [SerializeField] int use_life_num;
        /// <summary>使用ライフ数</summary>
        public int UseLifeNum { get => use_life_num; }

        [SerializeField] string image_key;
        /// <summary>画像アドレス</summary>
        public string ImageKey { get => image_key; }
    }
}
