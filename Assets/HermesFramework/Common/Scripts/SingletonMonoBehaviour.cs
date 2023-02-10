using System;
using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// SingletonMonoBehaviour
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        /// <summary>DontDestroyOnLoad‚É‚·‚é‚©‚Ìƒtƒ‰ƒO</summary>
        protected abstract bool isDontDestroyOnLoad { get; }
        /// <summary>Instance</summary>
        static T instance;
        /// <summary>Instance</summary>
        public static T Instance
        {
            get
            {
                if (!instance)
                {
                    Type t = typeof(T);
                    instance = (T)FindObjectOfType(t);
                    if (!instance)
                    {
                        Debug.LogError($"{t} is nothing.");
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Awake
        /// </summary>
        protected virtual void Awake()
        {
            if (this != Instance)
            {
                Destroy(this);
                return;
            }
            if (isDontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}