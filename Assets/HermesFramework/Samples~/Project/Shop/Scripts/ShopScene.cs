using Cysharp.Threading.Tasks;

namespace Shop
{
    /// <summary>
    /// Shop Scene
    /// </summary>
    public class ShopScene : Hermes.UI.Screen
    {
        public override bool IsBack { get; protected set; } = true;

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
            return UniTask.CompletedTask;
        }
    }
}