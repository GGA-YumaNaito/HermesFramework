using System.Collections.Generic;
using System.Reflection;

namespace Hermes
{
    /// <summary>
    /// Cache
    /// </summary>
    public class Cache
    {
        /// <summary>The member info caches.</summary>
        public static Dictionary<string, MemberInfo> MemberInfoCaches = new Dictionary<string, MemberInfo>();

        /// <summary>The members info caches.</summary>
        public static Dictionary<string, MemberInfo[]> MembersInfoCaches = new Dictionary<string, MemberInfo[]>();

        /// <summary>
        /// GetMembersInfo
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static MemberInfo[] GetMembersInfo(string key, object data, BindingFlags bindingAttr)
        {
            MemberInfo[] membersInfo;
            // キャッシュしていたらキャッシュから取得.
            if (Cache.MembersInfoCaches.ContainsKey(key))
            {
                membersInfo = Cache.MembersInfoCaches[key];
            }
            else
            {
                membersInfo = data.GetType().GetMembers(bindingAttr);
            }
            return membersInfo;
        }
        /// <summary>
        /// SetMembersInfo
        /// </summary>
        /// <param name="key"></param>
        /// <param name="membersInfo"></param>
        public static void SetMembersInfo(string key, MemberInfo[] membersInfo)
        {
            Cache.MembersInfoCaches[key] = membersInfo;
        }
    }
}