using Hermes.UI;
using Mobcast.Coffee.Toggles;
using UnityEngine;

namespace Footer
{
    /// <summary>
    /// Footer Scene
    /// </summary>
    public class FooterScene : SubScene
    {
        /// <summary>CompositeToggle</summary>
        [SerializeField] CompositeToggle compositeToggle = null;

        /// <summary>フッターが選択されているか</summary>
        bool isClickButton = false;

        /// <summary>
        /// Click Number
        /// </summary>
        public enum eClickNumber
        {
            /// <summary>None</summary>
            None = -1,
            /// <summary>Shop</summary>
            Shop,
            /// <summary>Formation</summary>
            Formation,
            /// <summary>Home</summary>
            Home,
            /// <summary>Growth</summary>
            Growth,
            /// <summary>Event</summary>
            Event,
        }

        void Update()
        {
            if (isClickButton)
                return;
            if (UIManager.Instance == null || UIManager.Instance.CurrentView == null)
                return;
            // アイコンがズレていたら修正
            var currentViewType = UIManager.Instance.CurrentView.GetType();
            var buttonType =
                currentViewType == typeof(Shop.ShopScene) ? eClickNumber.Shop :
                currentViewType == typeof(Formation.FormationScene) ? eClickNumber.Formation :
                currentViewType == typeof(Home.HomeScene) ? eClickNumber.Home :
                currentViewType == typeof(Growth.GrowthScene) ? eClickNumber.Growth :
                currentViewType == typeof(Event.EventScene) ? eClickNumber.Event : eClickNumber.None;
            if (buttonType == eClickNumber.None || compositeToggle.indexValue == (int)buttonType)
                return;
            compositeToggle.indexValue = (int)buttonType;
        }

        /// <summary>
        /// ボタンのクリック制御
        /// </summary>
        [EnumAction(typeof(eClickNumber))]
        public async void OnClickButton(int number)
        {
            var clickNumber = (eClickNumber)number;

            var currentViewType = UIManager.Instance.CurrentView.GetType();
            var buttonType =
                clickNumber == eClickNumber.Shop ? typeof(Shop.ShopScene) :
                clickNumber == eClickNumber.Formation ? typeof(Formation.FormationScene) :
                clickNumber == eClickNumber.Home ? typeof(Home.HomeScene) :
                clickNumber == eClickNumber.Growth ? typeof(Growth.GrowthScene) :
                clickNumber == eClickNumber.Event ? typeof(Event.EventScene) : null;
            if (currentViewType == buttonType)
                return;

            isClickButton = true;

            compositeToggle.indexValue = number;

            UIManager.Instance.ClearStackSpecifiedView<Home.HomeScene>();
            switch (clickNumber)
            {
                case eClickNumber.Shop:
                    await UIManager.Instance.LoadAsync<Shop.ShopScene>();
                    break;
                case eClickNumber.Formation:
                    await UIManager.Instance.LoadAsync<Formation.FormationScene>();
                    break;
                case eClickNumber.Home:
                    await UIManager.Instance.LoadAsync<Home.HomeScene>();
                    break;
                case eClickNumber.Growth:
                    await UIManager.Instance.LoadAsync<Growth.GrowthScene>();
                    break;
                case eClickNumber.Event:
                    await UIManager.Instance.LoadAsync<Event.EventScene>();
                    break;
            }

            isClickButton = false;
        }
    }
}