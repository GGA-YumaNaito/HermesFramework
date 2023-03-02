using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hermes
{
    /// <summary>
    /// フレームレートの設定クラス
    /// </summary>
    public class FrameRate : SingletonMonoBehaviour<FrameRate>
    {
        protected override bool isDontDestroyOnLoad => false;

        /// <summary>FrameRate</summary>
        [SerializeField] int frameRate = 60;

        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            SetTargetFrameRate(frameRate);
        }

        /// <summary>
        /// SetTargetFrameRate
        /// </summary>
        /// <param name="frameRate"></param>
        public void SetTargetFrameRate(int frameRate)
        {
            Application.targetFrameRate = frameRate;
        }
    }
}