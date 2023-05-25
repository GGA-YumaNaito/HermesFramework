using Cysharp.Threading.Tasks;

namespace Header
{
    /// <summary>
    /// Header Scene
    /// </summary>
    public class HeaderScene : Hermes.UI.SubScene
    {
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