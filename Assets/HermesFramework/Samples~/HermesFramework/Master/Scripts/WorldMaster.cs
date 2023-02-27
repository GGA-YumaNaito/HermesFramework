using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes.Master
{
    [CreateAssetMenu]
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
