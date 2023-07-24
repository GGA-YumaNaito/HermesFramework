using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// デバッグ用の表示オブジェクト
    /// </summary>
    public class DebugDisplay : MonoBehaviour
    {
#if !UNITY_EDITOR && !STG && !DEVELOPMENT_BUILD
        void Awake()
        {
            Destroy(gameObject);
        }
#endif
    }
}