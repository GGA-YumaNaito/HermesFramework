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

        /// <summary>ロード処理</summary>
        public abstract void OnLoad(object options);

        /// <summary>アンロード処理</summary>
        public abstract void OnUnload();

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
        /// 画面の状態更新
        /// </summary>
        /// <param name="status"></param>
        protected void SetStatus(eStatus status)
        {
            this.status.Value = status;
        }
    }
}