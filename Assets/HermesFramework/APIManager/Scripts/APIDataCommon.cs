using System;
using UnityEngine;

namespace Hermes.API
{
    /// <summary>
    /// 共通クラス
    /// </summary>
    [Serializable]
    public class APIDataCommon
    {
        [SerializeField] string master_version;
        /// <summary>Master Version</summary>
        public string MasterVersion { get => master_version; }

        [SerializeField] string app_version;
        /// <summary>App Version (最新)</summary>
        public string AppVersion { get => app_version; }

        [SerializeField] string app_version_force;
        /// <summary>App Version Force (強制変更)</summary>
        public string AppVersionForce { get => app_version_force; }
    }
}