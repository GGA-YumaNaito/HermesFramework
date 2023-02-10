using UniRx;
using UnityEngine;

namespace Hermes.UI
{
    /// <summary>
    /// ��ʂ̊��N���X
    /// </summary>
    public abstract class ViewBase : MonoBehaviour
    {
        /// <summary>�O�̉�ʂɖ߂�邩�t���O</summary>
        public abstract bool IsBack { get; protected set; }
        /// <summary>��ʑΏ�</summary>
        [SerializeField] protected RectTransform targetTransform;
        /// <summary>��ʂ̏��</summary>
        ReactiveProperty<eStatus> status = new ReactiveProperty<eStatus>(eStatus.None);
        /// <summary>��ʂ̏��</summary>
        public ReadOnlyReactiveProperty<eStatus> Status;

        /// <summary>���[�h����</summary>
        public abstract void OnLoad(object options);

        /// <summary>�A�����[�h����</summary>
        public abstract void OnUnload();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public ViewBase()
        {
            Status = status.ToReadOnlyReactiveProperty();
        }

        /// <summary>
        /// ����������
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
        /// ��ʂ̏�ԍX�V
        /// </summary>
        /// <param name="status"></param>
        protected void SetStatus(eStatus status)
        {
            this.status.Value = status;
        }
    }
}