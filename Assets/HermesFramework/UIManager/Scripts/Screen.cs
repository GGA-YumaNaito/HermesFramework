using DG.Tweening;
using System;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// �X�N���[�����
    /// </summary>
    public abstract class Screen : ViewBase
    {
        /// <summary>���[�h���̃A�j���[�V�����p�����[�^</summary>
        [SerializeField] AnimationParam loadAnimParam = new AnimationParam();
        /// <summary>�A�����[�h���̃A�j���[�V�����p�����[�^</summary>
        [SerializeField] AnimationParam unloadAnimParam = new AnimationParam();
        /// <summary>
        /// �A�j���[�V�����p�����[�^
        /// </summary>
        [Serializable] struct AnimationParam
        {
            /// <summary>�C�[�W���O</summary>
            public Ease ease;
            /// <summary>�A�j���[�V�����J�[�u</summary>
            public AnimationCurve animationCurve;
            /// <summary>�A�j���[�V��������</summary>
            public float moveTime;
            /// <summary>�^�[�Q�b�g�|�W�V����</summary>
            public Vector3 targetPos;
        }

        /// <summary>
        /// Status��Display�ɕύX
        /// </summary>
        void DoStatusDisplay()
        {
            SetStatus(eStatus.Display);
        }

        /// <summary>
        /// Status��End�ɕύX
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