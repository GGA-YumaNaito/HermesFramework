using System;
using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// DestroyEventListener
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DestroyEventListener : MonoBehaviour
    {
        /// <summary>OnDestroyed</summary>
        public event Action OnDestroyed;

        /// <summary>
        /// OnDestroy
        /// </summary>
        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}