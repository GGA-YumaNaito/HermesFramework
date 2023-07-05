using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace Hermes.Cinemachine
{
    /// <summary>
    /// CinemachineManager
    /// </summary>
    public class CinemachineManager : SingletonMonoBehaviour<CinemachineManager>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>Cinemachine Brain</summary>
        [SerializeField] protected CinemachineBrain cinemachineBrain = new();
        /// <summary>Cinemachine Virtual Camera List</summary>
        [SerializeField] protected List<CinemachineVirtualCameraBase> cinemachineVirtualCameraList = new();
        /// <summary>最初に選択するCinemachineのID</summary>
        [SerializeField] protected int firstId = 0;
        /// <summary>Priorityにプラスする値</summary>
        [SerializeField] protected int plusPriority = 100;

        /// <summary>現在のID</summary>
        int currentId = 0;

        protected override void Awake()
        {
            base.Awake();

            // Listが0ではなかったら
            if (cinemachineVirtualCameraList.Count > 0)
            {
                if (cinemachineVirtualCameraList.Count > firstId && firstId >= 0)
                {
                    cinemachineVirtualCameraList[firstId].Priority += plusPriority;
                    currentId = firstId;
                }
                else
                {
                    Debug.LogWarning("Warning!!! firstId is not set correctly.");
                    cinemachineVirtualCameraList[currentId].Priority += plusPriority;
                }
            }
            // Listが0なら
            else
            {
                Debug.LogError("Error!!! cinemachineVirtualCameraList is not set.");
            }
        }

        /// <summary>
        /// Switch to id
        /// </summary>
        /// <param name="id">ID</param>
        public virtual void SwitchTo(int id)
        {
            SwitchTo(id, null);
        }

        /// <summary>
        /// Switch to id
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="style">Style</param>
        public virtual void SwitchTo(int id, CinemachineBlendDefinition.Style? style = null)
        {
            if (id == currentId)
                return;
            if (style.HasValue)
                cinemachineBrain.m_DefaultBlend.m_Style = style.Value;
            cinemachineVirtualCameraList[id].Priority += plusPriority;
            cinemachineVirtualCameraList[currentId].Priority -= plusPriority;
            currentId = id;
        }
    }
}