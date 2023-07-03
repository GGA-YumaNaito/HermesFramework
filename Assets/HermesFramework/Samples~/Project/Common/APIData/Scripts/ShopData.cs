using System;
using Hermes.API;
using UnityEngine;

namespace API
{
    /// <summary>
    /// ショップデータ
    /// </summary>
    [Serializable]
    public class ShopData : APIData<ShopData>, ISerializationCallbackReceiver
    {
        protected override string api => "shop/list";

        protected override bool isPost => true;

        /// <summary>
        /// リクエスト
        /// </summary>
        [Serializable]
        public class Request
        {
            public int shopId;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Request()
            {
            }
        }

        [SerializeField] string name;
        /// <summary>ショップ名</summary>
        public string Name { get => name; }

        [SerializeField] string open_date_time;
        DateTime _open_date_time;
        /// <summary>開始時間</summary>
        public DateTime OpenDateTime { get => _open_date_time; }

        [SerializeField] string close_date_time;
        DateTime _close_date_time;
        /// <summary>終了時間</summary>
        public DateTime CloseDateTime { get => _close_date_time; }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(open_date_time) || string.IsNullOrEmpty(close_date_time))
                return;
            DateTime date;
            if (DateTime.TryParse(open_date_time, out date))
                _open_date_time = date;
            if (DateTime.TryParse(close_date_time, out date))
                _close_date_time = date;
        }
    }
}