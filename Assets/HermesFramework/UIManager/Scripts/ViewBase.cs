using Cysharp.Threading.Tasks;
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
#if UNITY_EDITOR || STG || DEVELOPMENT_BUILD
            Status
            .Subscribe(value =>
            {
                Debug.Log($"{base.name} : status = {status.Value}");
            })
            .AddTo(gameObject);
#endif
        }

        /// <summary>
        /// ロード処理
        /// </summary>
        /// <param name="options">オプション</param>
        /// <returns>UniTask</returns>
        public virtual UniTask OnLoad(object options) { return UniTask.CompletedTask; }

        /// <summary>
        /// アンロード処理
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual UniTask OnUnload() { return UniTask.CompletedTask; }

        /// <summary>
        /// 表示
        /// </summary>
        /// <returns>UniTask</returns>
        public async UniTask OnDisplay()
        {
            SetStatus(eStatus.Enable);

            await OnEnableAnimation();

            SetStatus(eStatus.Display);
        }

        /// <summary>
        /// 出現アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        protected virtual async UniTask OnEnableAnimation()
        {
            if (transition)
            {
                gameObject.SetActive(false);
                await UniTask.NextFrame();
                gameObject.SetActive(true);
                await UniTask.WaitUntil(() => !transition.isShow);
                transition.Show();
                await UniTask.WaitUntil(() => !transition.isPlaying);
            }
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <returns>UniTask</returns>
        public async UniTask OnEnd()
        {
            SetStatus(eStatus.Disable);

            await OnDisableAnimation();

            SetStatus(eStatus.End);
        }

        /// <summary>
        /// 退出アニメーション
        /// </summary>
        /// <returns>UniTask</returns>
        protected virtual async UniTask OnDisableAnimation()
        {
            if (transition)
            {
                transition.Hide();
                await UniTask.WaitUntil(() => !transition.isPlaying);
            }
        }

        /// <summary>
        /// 画面の状態更新
        /// </summary>
        /// <param name="status"></param>
        void SetStatus(eStatus status)
        {
            this.status.Value = status;
        }

        /// <summary>
        /// IsBackがfalseの時、UIManagerのBackAsyncが呼ばれた時に代わりに行う処理
        /// </summary>
        /// <returns>UniTask</returns>
        public virtual UniTask ActionInsteadOfBack() { return UniTask.CompletedTask; }
    }
}