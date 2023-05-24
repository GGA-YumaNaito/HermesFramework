using Cysharp.Threading.Tasks;
using Hermes.Asset;
using Hermes.Localize;
using Hermes.UI;
using Master;
using Mobcast.Coffee;
using TMPro;
using UnityEngine;

namespace Home
{
    /// <summary>
    /// マップ画像、名前、消費ライフが含まれるセルビュー.
    /// </summary>
    public class MapScrollCellView : ScrollCellView
    {
        /// <summary>マップ画像.</summary>
        [Header("マップ画像")][SerializeField] UIImage mapImage;
        /// <summary>名前テキスト.</summary>
        [Header("名前テキスト")][SerializeField] TextMeshProUGUI nameText;
        /// <summary>使用ライフ数</summary>
        [Header("使用ライフ数")][SerializeField] TextMeshProUGUI useLifeNumText = null;

        /// <summary>セルビューに表示させるデータ.</summary>
        public StageMasterData data { get; set; }
        /// <summary>Index</summary>
        public int index { get; set; }
        /// <summary>現在のIndex</summary>
        int mainIndex = -1;

        /// <summary>
        /// データリロードや明示的なリフレッシュがされたときにコールされます.
        /// </summary>
        public override void OnRefresh()
        {
        }

        /// <summary>
        /// 表示状態が変化したときにコールされます.
        /// </summary>
        /// <param name="visible">表示状態.</param>
        public override void OnChangedVisibility(bool visible)
        {
        }

        /// <summary>
        /// セルビューがオブジェクトプールに返却される前にコールされます.
        /// </summary>
        public override void OnBeforePool()
        {
        }

        /// <summary>
        /// 座標が変更されたときに、をコールされます.
        /// </summary>
        /// <param name="normalizedPosition">スクロール領域における正規化された位置.</param>
        public override void OnPositionChanged(float normalizedPosition)
        {
        }

        /// <summary>
        /// メインのアクティブが変更されていたら.
        /// </summary>
        public void OnChangeActiveIndex(int index)
        {
            mapImage.enabled = false;
            mainIndex = index;
            if (this.index != index)
                return;

            // 画像のロード.
            AssetManager.Load<Sprite>(
                data.ImageKey,
                (x) =>
                {
                    mapImage.enabled = index == mainIndex;
                    mapImage.sprite = x;
                },
                mapImage.gameObject,
                this.GetCancellationTokenOnDestroy()
                );
            nameText.text = LocalizeManager.Instance.GetValue(data.StageName);
            useLifeNumText.text = data.UseLifeNum.ToString();
        }

        /// <summary>
        /// マップをクリックした時
        /// </summary>
        public async void OnClickMap()
        {
            //await UIManager.Instance.LoadAsync<MapDialog>(new MapDialog.Options() { StageId = data.Id }, this.GetCancellationTokenOnDestroy());
        }
    }
}