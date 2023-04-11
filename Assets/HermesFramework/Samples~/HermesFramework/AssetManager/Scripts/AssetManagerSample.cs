using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Hermes.UI;
using UnityEngine;
using UnityEngine.U2D;

namespace Hermes.Asset
{
    public class AssetManagerSample : MonoBehaviour
    {
        [SerializeField] string atlas_key;
        [SerializeField] UIImage image;
        [SerializeField] string image_key;
        [SerializeField] Transform partParent;
        [SerializeField] string part_key;
        [SerializeField] string part_image_key;
        [SerializeField] List<GameObject> partList;

        SpriteAtlas spriteAtlas;

        void Start()
        {
            AssetManager.Load<SpriteAtlas>(atlas_key, (x) =>
                {
                    spriteAtlas = x;
                    image.sprite = spriteAtlas.GetSprite(image_key);
                }, gameObject);
        }

        public void OnClickCreateButton()
        {
            AssetManager.Load<GameObject>(part_key, (x) =>
            {
                // Instantiate
                var part = Instantiate(x, partParent);
                part.GetComponent<UIImage>().sprite = spriteAtlas.GetSprite(part_image_key);
                partList.Add(part);
            }, partParent.gameObject, this.GetCancellationTokenOnDestroy());
        }

        public void OnClickDeleteButton()
        {
            if (partList.Count > 0)
                Destroy(partList.Pop());
        }
    }
}