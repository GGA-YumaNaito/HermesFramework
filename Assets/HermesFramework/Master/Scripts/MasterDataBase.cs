/*
 * マスターデータの基底クラス群 
 * MasterDataBase：実際のデータを登録するクラス（名前とかコストとか）
*/
using UnityEngine;

namespace Hermes.Master
{
    /// <summary>
    /// データを登録するクラス
    /// </summary>
    [System.Serializable]
    public abstract class MasterDataBase
    {
        [SerializeField] int id = 0;
        /// <summary>ID</summary>
        public int Id { get => id; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MasterDataBase()
        {
            OnDeserialize();
        }

        /// <summary>デシリアライズ後の処理.</summary>
        public virtual void OnDeserialize()
        {
        }
    }
}