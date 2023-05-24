using System.Collections;
using System.Collections.Generic;
using Hermes.Master;
using UnityEngine;

namespace Master
{
    /// <summary>
    /// StageMaster
    /// </summary>
    [System.Serializable]
    public class StageMaster : MasterAssetBase<StageMasterData, StageMaster>
    {
        public override int MasterSetNumber
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// AreaIdからデータリストを取得
        /// </summary>
        /// <param name="areaId">Area id</param>
        /// <returns>エリアIDに紐付いたList</returns>
        public List<StageMasterData> GetDatasFromAreaId(int areaId)
        {
            return List.FindAll(x => x.AreaId == areaId);
        }
    }
}
