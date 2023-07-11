using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Formation
{
    /// <summary>
    /// Formation Scene
    /// </summary>
    public class FormationScene : Hermes.UI.Screen
    {
        public override bool IsBack { get; protected set; } = true;

        [SerializeField] RectTransform equipParent = new();
        [SerializeField] List<RectTransform> equipList = new();
        [SerializeField] RectTransform itemParent = new();
        [SerializeField] List<EquipItem> itemList = new();

        /// <summary>オプション</summary>
        public class Options
        {
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="options"></param>
        public override UniTask OnLoad(object options)
        {
            if (itemParent.childCount > 0)
            {
                foreach (RectTransform item in itemParent)
                {
                    itemList.Add(item.GetComponent<EquipItem>());
                }
            }
            foreach (var item in itemList)
            {
                item.DragEndEvent += (eventData) => {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(equipParent, eventData.position, Camera, out var result);

                    item.SetIsPrevPosition(true);

                    foreach(var equip in equipList)
                    {
                        // セット
                        if (equip.localPosition.x + equip.sizeDelta.x / 2f >= result.x &&
                            equip.localPosition.x - equip.sizeDelta.x / 2f <= result.x &&
                            equip.localPosition.y + equip.sizeDelta.y / 2f >= result.y &&
                            equip.localPosition.y - equip.sizeDelta.y / 2f <= result.y)
                        {
                            var rect = item.GetComponent<RectTransform>();
                            rect.SetParent(equip);
                            rect.position = equip.position;
                            item.SetIsPrevPosition(false);
                            item.DragControl(false);

                            itemList.Remove(item);
                            break;
                        }
                    };
                };
            };
            return UniTask.CompletedTask;
        }
    }
}