using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// WorldMasterData
    /// </summary>
    [System.Serializable]
    public class WorldMasterData : MasterDataBase
    {
        [SerializeField] string world_name;
        /// <summary>名前</summary>
        public string WorldName { get => world_name; }
    }
}
