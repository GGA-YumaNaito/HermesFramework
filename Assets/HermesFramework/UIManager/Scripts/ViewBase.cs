using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UniRx;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// 画面の基底クラス
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        /// <summary>前の画面に戻れるかフラグ</summary>
        public abstract bool IsBack { get; protected set; }
        /// <summary>画面対象</summary>
        [SerializeField] protected RectTransform targetTransform;
        /// <summary>画面の状態</summary>
        ReactiveProperty<eStatus> status = new ReactiveProperty<eStatus>(eStatus.None);
        /// <summary>画面の状態</summary>
        public ReadOnlyReactiveProperty<eStatus> Status;
        // TODO: 今は仮でこのアニメーションを使ってるけど、そのうちちゃんとしたのを作る
        /// <summary>ロード時のアニメーションパラメータ</summary>
        [SerializeField] AnimationParam loadAnimParam = new AnimationParam();
        /// <summary>アンロード時のアニメーションパラメータ</summary>
        [SerializeField] AnimationParam unloadAnimParam = new AnimationParam();
        /// <summary>
        /// アニメーションパラメータ
        /// </summary>
        [Serializable]
        struct AnimationParam
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
        /// コンストラクタ
        /// </summary>
        public ViewBase()
        {
            Status = status.ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void Initialize()
        {
            Status
            .Subscribe(value =>
            {
                Debug.Log($"{base.name} : status = {status.Value}");
            })
            .AddTo(gameObject);
        }

        /// <summary>
        /// ロード処理
        /// </summary>
        /// <param name="options"></param>
        /// <returns>UniTask</returns>
        public virtual async UniTask OnLoad(object options) { await UniTask.CompletedTask; }

        /// <summary>
        /// アンロード処理
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual async UniTask OnUnload() { await UniTask.CompletedTask; }

        /// <summary>
        /// StatusをDisplayに変更
        /// </summary>
        void DoStatusDisplay() => SetStatus(eStatus.Display);

        /// <summary>
        /// StatusをEndに変更
        /// </summary>
        void DoStatusEnd() => SetStatus(eStatus.End);

        /// <summary>
        /// 出現アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual async UniTask OnEnableAnimation()
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
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 退出アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual async UniTask OnDisableAnimation()
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
            await UniTask.CompletedTask;
        }

        /// <summary>
        /// 画面の状態更新
        /// </summary>
        /// <param name="status"></param>
        protected void SetStatus(eStatus status)
        {
            this.status.Value = status;
        }
    }
}