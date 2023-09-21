using System;
using System.Collections.Generic;

namespace Hermes.API
{
    /// <summary>
    /// サーバーデータのベースクラス
    /// </summary>
    [Serializable]
    public abstract class APIDataBase
    {
        /// <summary>API</summary>
        protected abstract string api { get; }
        /// <summary>Postで通信するならtrue</summary>
        protected abstract bool isPost { get; }

        /// <summary>
        /// API
        /// </summary>
        public string API
        {
            get
            {
                return api + QueryParams;
            }
        }

        /// <summary>
        /// is post
        /// </summary>
        public bool IsPost
        {
            get
            {
                return isPost;
            }
        }

        /// <summary>
        /// ヘッダー
        /// </summary>
        Dictionary<string, string> Headers
        {
            get; set;
        } = new Dictionary<string, string>();

        /// <summary>
        /// クエリパラメータ
        /// </summary>
        string QueryParams
        {
            get; set;
        } = null;

        /// <summary>
        /// ヘッダーセット
        /// </summary>
        /// <param name="headers"></param>
        public void SetHeaders(Dictionary<string, string> headers)
        {
            Headers = headers;
        }

        /// <summary>
        /// ヘッダーのDateを取得
        /// </summary>
        /// <returns></returns>
        public DateTime GetHeaderDateTime()
        {
            string value;
            if (Headers.TryGetValue("Date", out value))
            {
                return DateTime.Parse(value);
            }
            else
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// クエリパラメータセット
        /// </summary>
        /// <param name="queryParam"></param>
        public void SetQueryParam(string queryParam)
        {
            QueryParams = queryParam;
        }
    }
}