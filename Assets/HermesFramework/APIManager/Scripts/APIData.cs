using System;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// サーバーデータのベースクラス
    /// </summary>
    [Serializable]
    public abstract class APIData<T> : APIDataBase<T> where T : APIDataBase<T>
    {
        [SerializeField] T data;
        /// <summary>Data</summary>
        public T Data { get => data; }
    }
}