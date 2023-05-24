using System.Collections.Generic;
using Mobcast.Coffee;
using Mobcast.Coffee.UI;
using UnityEngine;

namespace Home
{
    /// <summary>
    /// ホームバナー用スクロールコントローラー
    /// </summary>
    public class HomeBannerScrollController : MonoBehaviour, IScrollViewController
	{
        /// <summary>テンプレート</summary>
		[Header("テンプレート")]
        [SerializeField] HomeBannerScrollCellView m_CellViewTemplate;

        /// <summary>スクロールビュー</summary>
        [Header("スクロールビュー")]
        [SerializeField] ScrollRectEx m_ScrollRectEx;

		/// <summary>セルビューのサイズ</summary>
		float _cellSize;

        /// <summary>
        /// データの要素数を取得します.
        /// </summary>
        /// <returns>データの要素数.</returns>
        public int GetDataCount()
		{
			return _dummyData.Count;
		}

		/// <summary>
		/// データインデックスに対するセルビューのサイズを取得します.
		/// セルビューサイズをデータに基づいて可変させたい場合、このメソッドを利用して調整できます.
		/// </summary>
		/// <returns>セルビューサイズ.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		public float GetCellViewSize(int dataIndex)
		{
			return _cellSize;
		}

		/// <summary>
		/// データインデックスに対するセルビューを取得します.
		/// オブジェクトプールを利用して取得/新規作成する場合、GetCellView([TEMPLATE_CELLVIEW_PREFAB]) でセルビューを取得できます.
		/// 取得したセルビューに対し、データを引き渡してください.
		/// </summary>
		/// <returns>セルビュー.</returns>
		/// <param name="dataIndex">データインデックス.</param>
		public ScrollCellView GetCellView(int dataIndex)
		{
			// セルビューを取得. オブジェクトプールに存在する場合、優先的に利用します.
			HomeBannerScrollCellView cellView = m_ScrollRectEx.GetCellView(m_CellViewTemplate) as HomeBannerScrollCellView;
			cellView.gameObject.SetActive(true);

			// データをセルビューに渡します.
			cellView.data = _dummyData[dataIndex];

			return cellView;
		}

		/// <summary>
		/// ダミーデータ.
		/// </summary>
		// TODO: ダミーデータ
		List<HomeBannerScrollData> _dummyData = new List<HomeBannerScrollData>()
		{
			new HomeBannerScrollData(){ imageUrl = "Banner_1" },
			new HomeBannerScrollData(){ imageUrl = "Banner_2" },
			new HomeBannerScrollData(){ imageUrl = "Banner_3" },
			new HomeBannerScrollData(){ imageUrl = "Banner_4" },
			new HomeBannerScrollData(){ imageUrl = "Banner_5" },
		};

		void Start()
		{
			// テンプレートを非表示
			m_CellViewTemplate.gameObject.SetActive(false);

			// セルサイズをテンプレートから取得
			var size = (m_CellViewTemplate.transform as RectTransform).sizeDelta;
			_cellSize = m_ScrollRectEx.scrollRect.vertical ? size.y : size.x;

			// コントローラを割り当て、リロード実行.
			m_ScrollRectEx.controller = this;
			m_ScrollRectEx.ReloadData();
		}
	}
}
