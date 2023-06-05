using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// SubScene画面
    /// </summary>
    public abstract class SubScene : ViewBase
    {
        // SubSceneに戻りはない
        public override bool IsBack { get; protected set; } = false;

        /// <summary>Camera</summary>
        [SerializeField] new Camera camera = null;
        /// <summary>Camera</summary>
        public Camera Camera { get => camera; }
    }
}