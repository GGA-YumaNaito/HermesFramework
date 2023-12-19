using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
        /// <param name="key">key</param>
        /// <param name="playType">playType</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public void Play(string key, ePlayType playType = ePlayType.Play, CancellationToken cancellationToken = default)
            => PlayAsync(key, null, playType, cancellationToken).Forget();

        /// <summary>
        /// 再生Async
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="playType">playType</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayAsync(string key, ePlayType playType = ePlayType.Play, CancellationToken cancellationToken = default)
            => await PlayAsync(key, null, playType, cancellationToken);

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="playType">playType</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public void Play(AudioClip clip, ePlayType playType = ePlayType.Play, CancellationToken cancellationToken = default)
            => PlayAsync(null, clip, playType, cancellationToken).Forget();

        /// <summary>
        /// 再生Async
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="playType">playType</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayAsync(AudioClip clip, ePlayType playType = ePlayType.Play, CancellationToken cancellationToken = default)
            => await PlayAsync(null, clip, playType, cancellationToken);

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="key">key</param>
        public void Stop(string key = null) => StopTargetKey(key);

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="clip">clip</param>
        public void Stop(AudioClip clip) => StopTargetClip(clip);
    }
}