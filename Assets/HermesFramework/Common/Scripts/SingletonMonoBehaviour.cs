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
        /// <summary>DontDestroyOnLoadにするかのフラグ</summary>
        protected abstract bool isDontDestroyOnLoad { get; }
        /// <summary>Instance</summary>
        static T instance;
        /// <summary>Instance</summary>
        public static T Instance
        {
            get
            {
                if (!HasInstance())
                    Debug.LogError($"{typeof(T)} is nothing.");
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

        /// <summary>
        /// Instanceが存在するか
        /// </summary>
        /// <returns></returns>
        public static bool HasInstance()
        {
            instance ??= (T)FindObjectOfType(typeof(T));
            return instance is not null;
        }

        void OnDestroy()
        {
            instance = null;
        }
    }
}