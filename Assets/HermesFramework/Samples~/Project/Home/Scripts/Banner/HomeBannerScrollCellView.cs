using Cysharp.Threading.Tasks;
using Hermes.Asset;
using Hermes.UI;
using Mobcast.Coffee;
using UnityEngine;

namespace Home
{
    /// <summary>
    /// バナー画像が含まれるセルビュー.
    /// </summary>
    public class HomeBannerScrollCellView : ScrollCellView
	{
        /// <summary>バナー画像.</summary>
        [Header("バナー画像")][SerializeField]UIImage m_ImageIcon;

        /// <summary>セルビューに表示させるデータ.</summary>
        public HomeBannerScrollData data { get; set; }

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
			if (visible)
			{
                // アイコンのロード.
                AssetManager.Load<Sprite>(data.imageUrl, (x) => m_ImageIcon.sprite = x, m_ImageIcon.gameObject, this.GetCancellationTokenOnDestroy());
            }
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
    }
}