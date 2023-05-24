using Cysharp.Threading.Tasks;
using Hermes.UI;
using Mobcast.Coffee.UI;
using UnityEngine;

namespace Home
{
    /// <summary>
    /// マップコントローラー
    /// </summary>
    public class MapController : MonoBehaviour
    {
        /// <summary>スクロールビュー</summary>
        [SerializeField] ScrollRectEx scrollRectEx;
        /// <summary>マップスクロールコントローラー</summary>
        [SerializeField] MapScrollController scrollController = null;

        /// <summary>
        /// 作戦開始ボタン
        /// </summary>
        public async void OnClickStart()
        {
            //await UIManager.Instance.LoadAsync<GameScene>(new GameScene.Options()
            //{
            //    StageId = ((MapScrollCellView)scrollController.GetCellView(scrollRectEx.activeIndex)).data.Id
            //});
            //await UIManager.Instance.LoadAsync<GameScene>(new GameScene.Options()
            //{
            //    StageId = stageId
            //});
        }

        /// <summary>
        /// マップをクリックした時
        /// </summary>
        public async void OnClickMap()
        {
            //await UIManager.Instance.LoadAsync<MapDialog>(new MapDialog.Options() { StageId = stageId }, this.GetCancellationTokenOnDestroy());
        }
    }
}
