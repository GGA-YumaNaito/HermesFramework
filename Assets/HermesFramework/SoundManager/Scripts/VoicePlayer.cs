using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.Sound
{
    /// <summary>
    /// VoicePlayer
    /// </summary>
    public class VoicePlayer : AudioPlayer
    {
        protected override bool isLoop => false;

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="playType">playType</param>
        public void Play(string key, ePlayType playType = ePlayType.Play) => PlayAsync(key, playType, this.GetCancellationTokenOnDestroy()).Forget();

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="playType">playType</param>
        public void Play(AudioClip clip, ePlayType playType = ePlayType.Play) => PlayAsync(clip, playType, this.GetCancellationTokenOnDestroy()).Forget();

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="key">key</param>
        public void Stop(string key = null) => StopTargetKey(key);

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="clip">clip</param>
        public void Stop(AudioClip clip = null) => StopTargetClip(clip);
    }
}