using System.Collections.Generic;
using Mobcast.Coffee;
using Mobcast.Coffee.UI;
using UniRx;
using UnityEngine;
using Master;

namespace Home
{
    /// <summary>
    /// ホームバナー用スクロールコントローラー
    /// </summary>
    public class MapScrollController : MonoBehaviour, IScrollViewController
    {
        /// <summary>テンプレート</summary>
        [Header("テンプレート")]
        [SerializeField] MapScrollCellView m_CellViewTemplate;

        /// <summary>スクロールビュー</summary>
        [Header("スクロールビュー")]
        [SerializeField] ScrollRectEx m_ScrollRectEx;

        /// <summary>ステージマスターデータリスト</summary>
        List<StageMasterData> stageMasterDatas = new List<StageMasterData>();

        /// <summary>セルビューのサイズ</summary>
        float _cellSize;

        /// <summary>
        /// データの要素数を取得します.
        /// </summary>
        /// <returns>データの要素数.</returns>
        public int GetDataCount()
        {
            return stageMasterDatas.Count;
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
            MapScrollCellView cellView = m_ScrollRectEx.GetCellView(m_CellViewTemplate) as MapScrollCellView;
            cellView.gameObject.SetActive(true);

            // データをセルビューに渡します.
            cellView.data = stageMasterDatas[dataIndex];
            cellView.index = dataIndex;

            return cellView;
        }

        /// <summary>
        /// ステージデータを格納
        /// </summary>
        /// <param name="stageId"></param>
        public void SetStageData(int stageId)
        {
            var data = StageMaster.Instance.GetDataFromId(stageId);
            stageMasterDatas = StageMaster.Instance.GetDatasFromAreaId(data.AreaId);
        }

        void Start()
        {
            // テンプレートを非表示
            m_CellViewTemplate.gameObject.SetActive(false);

            // セルサイズをテンプレートから取得
            var size = (m_CellViewTemplate.transform as RectTransform).sizeDelta;
            _cellSize = m_ScrollRectEx.scrollRect.vertical ? size.y : size.x;

            // コントローラを割り当て、リロード実行.
            m_ScrollRectEx.controller = this;

            // アクティブIndexが変更したら
            m_ScrollRectEx
                .ObserveEveryValueChanged(x => x.activeIndex)
                .Where(x => x > -1)
                .Subscribe(i =>
                {
                    foreach(var cellView in m_ScrollRectEx.activeCellViews)
                    {
                        (cellView as MapScrollCellView)?.OnChangeActiveIndex(i);
                    }
                })
                .AddTo(this);
        }
    }
}
