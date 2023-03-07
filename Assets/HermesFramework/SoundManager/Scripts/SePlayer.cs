using Cysharp.Threading.Tasks;

namespace Hermes.Sound
{
    /// <summary>
    /// SePlayer
    /// </summary>
    public class SePlayer : AudioPlayer
    {
        protected override bool isLoop => false;

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playType"></param>
        public void Play(string key, ePlayType playType = ePlayType.Play) => PlayAsync(key, playType).Forget();

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="key"></param>
        public void Stop(string key = null) => StopTargetKey(key);
    }
}