using UnityEngine;
using Hermes.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Hermes.Asset
{
    public class AssetManagerSample : MonoBehaviour
    {
        [SerializeField] UIImage image;
        [SerializeField] string image_key;
        [SerializeField] Transform partParent;
        [SerializeField] string part_key;
        [SerializeField] List<GameObject> partList;

        void Start()
        {
            AssetManager.Load<Sprite>(image_key, (x) => image.sprite = x, image.gameObject, this.GetCancellationTokenOnDestroy());
        }

        public void OnClickCreateButton()
        {
            AssetManager.Load<GameObject>(part_key, (x) =>
            {
                // Instantiate
                partList.Add(Instantiate(x, partParent));
            }, partParent.gameObject, this.GetCancellationTokenOnDestroy());
        }

        public void OnClickDeleteButton()
        {
            if (partList.Count > 0)
                Destroy(partList.Pop());
        }
    }
}