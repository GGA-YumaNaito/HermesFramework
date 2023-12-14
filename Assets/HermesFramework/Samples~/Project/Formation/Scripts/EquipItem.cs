using Cysharp.Threading.Tasks;
using Hermes.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Formation
{
    /// <summary>
    /// Equip Item
    /// </summary>
    public class EquipItem : MonoBehaviour
    {
        /// <summary>UIButton</summary>
        [SerializeField] UIDragButton dragButton;

        /// <summary>DragEndDelegate</summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="eventData">EventData</param>
        public delegate void DragEndDelegate<T>(T eventData);
        /// <summary>Drag終了時のイベント</summary>
        public event DragEndDelegate<PointerEventData> DragEndEvent;

        async void Awake()
        {
            if (dragButton == null)
                return;
            await UniTask.WaitWhile(() => DragEndEvent == null);

            dragButton.DragEndEvent += (x) => DragEndEvent(x);
        }

        /// <summary>
        /// ドラッグの制御
        /// </summary>
        /// <param name="isDrag"></param>
        public void DragControl(bool isDrag)
        {
            if (dragButton == null)
                return;
            dragButton.isDrag = isDrag;
        }

        /// <summary>
        /// ドラッグ前の位置に戻すフラグ設定
        /// <para>true 戻す, false 戻さない</para>
        /// </summary>
        /// <param name="isPrevPosition">isPrevPosition</param>
        public void SetIsPrevPosition(bool isPrevPosition)
        {
            if (dragButton == null)
                return;
            dragButton.SetIsPrevPosition(isPrevPosition);
        }
    }
}