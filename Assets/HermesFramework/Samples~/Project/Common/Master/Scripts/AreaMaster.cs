using System.Collections;
using System.Collections.Generic;
using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// AreaMaster
    /// </summary>
    [System.Serializable]
    public class AreaMaster : MasterAssetBase<AreaMasterData, AreaMaster>
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
