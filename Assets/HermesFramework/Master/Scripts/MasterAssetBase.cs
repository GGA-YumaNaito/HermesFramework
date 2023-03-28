/*
 * マスターデータの基底クラス群 
 * MasterAssetBase：MasterDataBaseのリストを持っていてを持っていてScriptableObject型で保存されるクラスで、ランタイム時の管理も行うクラス
*/
using System.Collections.Generic;
using UnityEngine;

namespace Hermes.Master
{
    /// <summary>
    /// MasterDataBaseのリストを持っている
    /// <para>ScriptableObject型で保存</para>
    /// <para>ランタイム時の管理も行うクラス</para>
    /// </summary>
    /// <typeparam name="T1">MasterDataBase</typeparam>
    /// <typeparam name="T2">MasterAssetBase</typeparam>
    [System.Serializable]
    public abstract class MasterAssetBase<T1, T2> : ScriptableObject
        where T1 : MasterDataBase, new()
        where T2 : MasterAssetBase<T1, T2>
    {
        /// <summary>スプレッドシートのナンバー</summary>
        public abstract int MasterSetNumber { get; }

        /// <summary>
        /// ランタイム時用のインスタンス
        /// <para>各継承先ではこのインスタンス経由でListにアクセスする</para>
        /// </summary>
        /// <value>The instance.</value>
        public static T2 Instance
        {
            get
            {
                if (m_RuntimeInstance == null)
                {
                    m_RuntimeInstance = ScriptableObject.CreateInstance<T2>();
                }
                return m_RuntimeInstance;
            }
        }
        static T2 m_RuntimeInstance;

        /// <summary>
        /// マスターデータのリスト
        /// </summary>
        [SerializeField]
        public List<T1> List = new List<T1>();

        // ====== ランタイム時用関数 =====

        /// <summary>
        /// マスターデータをリストで追加
        /// </summary>
        /// <param name="datas">マージするマスターデータ.</param>
        public static void AddListToRuntimeMaster(IEnumerable<T1> datas)
        {
            foreach (T1 data in datas)
            {
                if (data != null && data.IsRelease)
                {
                    data.OnDeserialize();
                    AddRuntimeMaster(data);
                }
            }
        }
        /// <summary>
        /// マスターデータを追加
        /// </summary>
        /// <param name="data">Data.</param>
        public static void AddRuntimeMaster(T1 data)
        {
            Instance.List.Add(data);
        }
        /// <summary>
        /// マスターデータを削除
        /// </summary>
        public static void ClearRuntimeMaster()
        {
            Instance.List.Clear();
        }

        // ===== ScriptableObjectへの関数 =====
#if UNITY_EDITOR
        /// <summary>
        /// マスターデータをリストで追加
        /// </summary>
        /// <param name="datas">マージするマスターデータ.</param>
        public void AddList(IEnumerable<T1> datas)
        {
            foreach (T1 d in datas)
            {
                List.Add(d);
            }
        }
        /// <summary>
        /// マスターデータを追加
        /// </summary>
        /// <param name="data">Data.</param>
        public void Add(T1 data)
        {
            if (data != null)
                List.Add(data);
        }
        /// <summary>
        /// マスターデータを追加
        /// </summary>
        /// <param name="data">Data.</param>
        public void Add(object data)
        {
            if (data != null)
                List.Add(data as T1);
        }
        /// <summary>
        /// マスターデータを削除
        /// </summary>
        public void Clear()
        {
            List.Clear();
        }
#endif

    }
}