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
        public override bool IsBack { get; protected set; } = true;

        /// <summary>
        /// Click Number
        /// </summary>
        public enum eClickNumber
        {
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
        
        /// <summary>
        /// ボタンのクリック制御
        /// </summary>
        [EnumAction(typeof(eClickNumber))]
        public async void OnClickButton(int state)
        {
            UIManager.Instance.ClearStackSpecifiedView<Home.HomeScene>();
            switch ((eClickNumber)state)
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
        }
    }
}