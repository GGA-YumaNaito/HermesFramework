using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Mobcast.Coffee.Transition;
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
        /// <summary>画面の状態</summary>
        ReactiveProperty<eStatus> status = new ReactiveProperty<eStatus>(eStatus.None);
        /// <summary>画面の状態</summary>
        public ReadOnlyReactiveProperty<eStatus> Status;
        /// <summary>アニメーションパラメータ</summary>
        [SerializeField] UITransition transition;

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

            if (transition)
            {
                gameObject.SetActive(false);
                await UniTask.NextFrame();
                gameObject.SetActive(true);
                await UniTask.WaitUntil(() => !transition.isShow);
                transition.Show();
                await UniTask.WaitUntil(() => !transition.isPlaying);
            }

            DoStatusDisplay();
        }

        /// <summary>
        /// 退出アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual async UniTask OnDisableAnimation()
        {
            SetStatus(eStatus.Disable);

            if (transition)
            {
                transition.Hide();
                await UniTask.WaitUntil(() => !transition.isPlaying);
            }

            DoStatusEnd();
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