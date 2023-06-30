using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// AreaMasterData
    /// </summary>
    [System.Serializable]
    public class AreaMasterData : MasterDataBase
    {
        [SerializeField] string area_name;
        /// <summary>名前</summary>
        public string AreaName { get => area_name; }

        [SerializeField] int world_id;
        /// <summary>ワールドID</summary>
        public int WorldId { get => world_id; }
    }
}
