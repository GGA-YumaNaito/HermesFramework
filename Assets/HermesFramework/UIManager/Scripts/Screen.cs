using DG.Tweening;
using System;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// スクリーン画面
    /// </summary>
    public abstract class Screen : ViewBase
    {
        /// <summary>ロード時のアニメーションパラメータ</summary>
        [SerializeField] AnimationParam loadAnimParam = new AnimationParam();
        /// <summary>アンロード時のアニメーションパラメータ</summary>
        [SerializeField] AnimationParam unloadAnimParam = new AnimationParam();
        /// <summary>
        /// アニメーションパラメータ
        /// </summary>
        [Serializable] struct AnimationParam
        {
            /// <summary>イージング</summary>
            public Ease ease;
            /// <summary>アニメーションカーブ</summary>
            public AnimationCurve animationCurve;
            /// <summary>アニメーション時間</summary>
            public float moveTime;
            /// <summary>ターゲットポジション</summary>
            public Vector3 targetPos;
        }

        /// <summary>
        /// StatusをDisplayに変更
        /// </summary>
        void DoStatusDisplay()
        {
            SetStatus(eStatus.Display);
        }

        /// <summary>
        /// StatusをEndに変更
        /// </summary>
        void DoStatusEnd()
        {
            SetStatus(eStatus.End);
        }

        public override void OnLoad(object options)
        {
            SetStatus(eStatus.Enable);

            if (loadAnimParam.ease != Ease.Unset)
            {
                targetTransform.localPosition = loadAnimParam.targetPos;
                targetTransform.DOLocalMove(Vector3.zero, loadAnimParam.moveTime).SetEase(loadAnimParam.ease).OnComplete(DoStatusDisplay);
                return;
            }
            else if (loadAnimParam.animationCurve != null)
            {
                targetTransform.localPosition = loadAnimParam.targetPos;
                targetTransform.DOLocalMove(Vector3.zero, loadAnimParam.moveTime).SetEase(loadAnimParam.animationCurve).OnComplete(DoStatusDisplay);
                return;
            }

            DoStatusDisplay();
        }

        public override void OnUnload()
        {
            SetStatus(eStatus.Disable);

            if (unloadAnimParam.ease != Ease.Unset)
            {
                targetTransform.DOLocalMove(unloadAnimParam.targetPos, unloadAnimParam.moveTime).SetEase(unloadAnimParam.ease).OnComplete(DoStatusEnd);
                return;
            }
            else if (unloadAnimParam.animationCurve != null)
            {
                targetTransform.DOLocalMove(unloadAnimParam.targetPos, unloadAnimParam.moveTime).SetEase(unloadAnimParam.animationCurve).OnComplete(DoStatusEnd);
                return;
            }

            DoStatusEnd();
        }
    }
}