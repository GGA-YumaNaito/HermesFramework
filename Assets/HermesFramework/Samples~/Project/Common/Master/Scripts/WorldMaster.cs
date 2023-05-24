using System.Collections;
using System.Collections.Generic;
using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// WorldMaster
    /// </summary>
    [System.Serializable]
    public class WorldMaster : MasterAssetBase<WorldMasterData, WorldMaster>
    {
        public override int MasterSetNumber
        {
            get
            {
                return 0;
            }
        }
    }
}
