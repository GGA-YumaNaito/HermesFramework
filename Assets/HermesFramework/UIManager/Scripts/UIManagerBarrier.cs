using System;
using UnityEngine;

namespace Hermes.UI.UIManagerParts
{
    /// <summary>
    /// UIManager Barrier
    /// </summary>
    [Serializable]
    public class UIManagerBarrier
    {
        /// <summary>バリア</summary>
        [SerializeField] GameObject barrier = null;

        /// <summary>オブジェクトが非表示になっているか</summary>
        public bool ActiveSelf => barrier.activeSelf;

        /// <summary>Count</summary>
        int count = 0;

        /// <summary>
        /// バリアをアクティブ、非アクティブ化する
        /// </summary>
        /// <param name="value">Value</param>
        public void SetActive(bool value)
        {
            if (value)
            {
                count++;
                if (count == 1)
                    barrier.SetActive(true);
            }
            else
            {
                count--;
                if (count == 0)
                    barrier.SetActive(false);
            }
        }

        /// <summary>
        /// 強制的にバリアをアクティブ、非アクティブ化する
        /// </summary>
        /// <param name="value">Value</param>
        public void SetActiveForce(bool value)
        {
            barrier.SetActive(value);
        }
    }
}