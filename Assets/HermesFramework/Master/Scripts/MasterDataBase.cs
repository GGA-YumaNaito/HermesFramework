/*
 * マスターデータの基底クラス群 
 * MasterDataBase：実際のデータを登録するクラス（名前とかコストとか）
*/
namespace Hermes.Master
{
    /// <summary>
    /// データを登録するクラス
    /// </summary>
    [System.Serializable]
    public abstract class MasterDataBase
    {
        /// <summary>ID.</summary>
        public int Id = 0;

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