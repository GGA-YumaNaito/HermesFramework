using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Hermes.Sound
{
    /// <summary>
    /// BgmPlayer
    /// <para>音のMAXは最大2にする</para>
    /// </summary>
    public class BgmPlayer : AudioPlayer
    {
        protected override bool isLoop => true;

        /// <summary>フェードイン時間</summary>
        protected float FadeInTime { get => fadeInTime; set { if (value <= 0f) { fadeInTime = 0f; } else fadeInTime = value; } }
        [SerializeField, Tooltip("フェードイン時間")] float fadeInTime = 1f;
        /// <summary>フェードアウト時間</summary>
        protected float FadeOutTime { get => fadeOutTime; set { if (value <= 0f) { fadeOutTime = 0f; } else fadeInTime = value; } }
        [SerializeField, Tooltip("フェードアウト時間")] float fadeOutTime = 2f;
        /// <summary>インターバル時間 (フェードアウトありで負数ならクロスフェード)</summary>
        protected float IntervalTime { get => intervalTime; set => intervalTime = value; }
        [SerializeField, Tooltip("インターバル時間 (フェードアウトありで負数ならクロスフェード)")] float intervalTime = -1f;
        /// <summary>クロスフェードフラグ (フェードアウトありでインターバル時間が負数ならクロスフェード)</summary>
        protected bool IsCrossfade { get => isCrossfade; set => isCrossfade = value; }
        [SerializeField, Tooltip("クロスフェードフラグ (フェードアウトありでインターバル時間が負数ならクロスフェード)")] protected bool isCrossfade = false;

        // メンバー要素
        /// <summary>再生状態</summary>
        protected eStatus[] state;
        /// <summary>直前の再生状態</summary>
        protected eStatus[] lastState;
        /// <summary>状態残存時間</summary>
        protected float[] remainTime;
        /// <summary>発生チャネル</summary>
        protected int playChannel;
        /// <summary>待機番号</summary>
        protected int waitNum;
        /// <summary>一時的なノーマルフェードフラグ</summary>
        protected bool TempIsNormalfade { get => !(IsCrossfade || tempIsCrossfade) || tempIsNormalfade; set => tempIsNormalfade = value; }
        protected bool tempIsNormalfade { get => tempIsNormalfadeCount > 0; set { if (value) tempIsNormalfadeCount++; else tempIsNormalfadeCount--; } }
        protected int tempIsNormalfadeCount;
        /// <summary>一時的なクロスフェードフラグ</summary>
        protected bool TempIsCrossfade { get => (IsCrossfade || tempIsCrossfade) && !tempIsNormalfade; set => tempIsCrossfade = value; }
        protected bool tempIsCrossfade { get => tempIsCrossfadeCount > 0; set { if (value) tempIsCrossfadeCount++; else tempIsCrossfadeCount--; } }
        protected int tempIsCrossfadeCount;

        /// <summary>サブチャネル</summary>
        protected int subChannel(int main) => (main == 0) ? 1 : 0;

        /// <summary>アクティブなチャネル</summary>
        protected int musicActiveChannel
        {
            get
            {
                var smsc = subChannel(playChannel);
                if (source[smsc].isPlaying)
                {
                    if (source[playChannel].isPlaying && source[playChannel].volume >= source[smsc].volume)
                    {
                        return playChannel;
                    }
                    return smsc;
                }
                return playChannel;
            }
        }

        /// <summary>再生状態</summary>
        protected enum eStatus
        {
            /// <summary>停止</summary>
            STOP = 0,
            /// <summary>再生中</summary>
            PLAYING,
            /// <summary>インターバル待ち</summary>
            WAIT_INTERVAL,
            /// <summary>フェードイン中</summary>
            FADEIN,
            /// <summary>フェードアウト中</summary>
            FADEOUT,
        }

        /// <summary>
        /// 初期化
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            if (FadeInTime <= 0f) { FadeInTime = 0f; }
            if (FadeOutTime <= 0f) { FadeOutTime = 0f; }
            state = new eStatus[max];
            lastState = new eStatus[max];
            remainTime = new float[max];
            playChannel = 1;
            waitNum = 0;
            tempIsNormalfadeCount = 0;
            tempIsCrossfadeCount = 0;
        }

        /// <summary>
        /// 駆動
        /// </summary>
        protected override void Update()
        {
            base.Update();
            for (var i = 0; i < source.Length; i++)
            {
                var smsc = subChannel(i);
                if (lastState[i] != state[i])
                {
                    switch (state[i])
                    {
                        case eStatus.STOP:
                            remainTime[i] = 0f;
                            source[i].volume = 0f;
                            source[i].Stop();
                            break;
                        case eStatus.PLAYING:
                            remainTime[i] = 0f;
                            source[i].volume = coefficient * volume;
                            if (lastState[i] != eStatus.FADEIN)
                            {
                                source[i].Play();
                            }
                            break;
                        case eStatus.WAIT_INTERVAL:
                            remainTime[i] = ((state[smsc] == eStatus.FADEOUT) ? FadeOutTime : 0f) +
                                (TempIsCrossfade ? intervalTime : intervalTime > 0f ? intervalTime : 0f);
                            source[i].volume = 0f;
                            break;
                        case eStatus.FADEIN:
                            remainTime[i] = FadeInTime * (Volume - source[i].volume) / Volume;
                            if (!source[i].isPlaying)
                            {
                                source[i].Play();
                            }
                            break;
                        case eStatus.FADEOUT:
                            if (lastState[i] == eStatus.FADEIN)
                            {
                                remainTime[i] = FadeOutTime * source[i].volume / Volume;
                            }
                            else if (lastState[i] == eStatus.WAIT_INTERVAL)
                            {
                                remainTime[i] = 0f;
                            }
                            else
                            {
                                remainTime[i] = FadeOutTime;
                            }
                            break;
                    }
                    lastState[i] = state[i];
                }
                else
                {
                    remainTime[i] -= Time.deltaTime; // 経過時間セット
                    switch (state[i])
                    {
                        case eStatus.WAIT_INTERVAL:
                            if (remainTime[i] <= 0f)
                            {
                                state[i] = eStatus.FADEIN;
                            }
                            break;
                        case eStatus.FADEIN:
                            if (remainTime[i] >= 0f)
                            {
                                source[i].volume = coefficient * volume * (1f - remainTime[i] / FadeInTime);
                            }
                            else
                            {
                                state[i] = eStatus.PLAYING;
                            }
                            break;
                        case eStatus.FADEOUT:
                            if (remainTime[i] >= 0f)
                            {
                                source[i].volume = coefficient * volume * remainTime[i] / FadeOutTime;
                            }
                            else
                            {
                                state[i] = eStatus.STOP;
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 再生Async
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">cancellationToken</param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsync(string key, AudioClip clip, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(key))
            {
                if (clip == null)
                    return;
                else
                    key = clip.name;
            }

            // クロスフェードではなかった場合の処理
            if (TempIsNormalfade)
            {
                if (useList.ContainsKey(key))
                {
                    if (waitNum == 0)
                        return;
                    if (state[playChannel] == eStatus.FADEOUT)
                    {
                        state[playChannel] = eStatus.FADEIN;
                        return;
                    }
                }
                waitNum++;
                var num = waitNum;
                if (useList.Count > 0 && !source[playChannel].isPlaying)
                    await UniTask.WaitUntil(() => source[playChannel].isPlaying);
                state[playChannel] = source[playChannel].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // 表を停止
                await UniTask.WaitUntil(() => useList.Count == 0 || num != waitNum);
                // 番号が違ったら処理終了
                if (num != waitNum)
                    return;
                waitNum = 0;
            }

            var smsc = subChannel(playChannel); // 裏チャネル

            // 再生中の裏と一致
            if (useList.ContainsKey(key))
            {
                var keyIndex = useList[key][0].Key;
                if (keyIndex < 0)
                    return;
                if (source[smsc].isPlaying && source[smsc].clip == source[keyIndex].clip)
                {
                    state[playChannel] = source[playChannel].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // 表を停止
                    state[smsc] = eStatus.FADEIN; // 裏を開始
                    playChannel = smsc; // 表裏入れ替え
                }
            }
            // どちらとも一致しない
            else
            {
                var i = clipList.IndexOf(null);
                // 同時再生数を超えている
                if (i < 0)
                {
                    Debug.LogError("The number of simultaneous playback of sound is exceeded.");
                    return;
                }
                playChannel = musicActiveChannel; // アクティブな方を表に
                smsc = subChannel(playChannel);
                state[playChannel] = source[playChannel].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // 表をフェードアウト
                lastState[smsc] = eStatus.STOP; // 裏を即時停止
                source[smsc].Stop();
                source[smsc].volume = 0f;
                state[smsc] = eStatus.WAIT_INTERVAL; // 裏を開始
                playChannel = smsc; // 表裏入れ替え
                await PlayAsync(key, clip, ePlayType.IfNot, cancellationToken);
                // BGMが全て消えていたら
                if (clipList.Count(x => x == null) >= max)
                    playChannel = 1; // 初期化
            }
        }

        /// <summary>
        /// 全体を止める(keyが指定された時はそのkeyの音を止める)
        /// </summary>
        /// <param name="key">key</param>
        protected override void StopTargetKey(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                foreach (var list in useList)
                {
                    foreach (var pair in list.Value)
                    {
                        state[pair.Key] = source[pair.Key].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // フェードアウト or ストップ
                    }
                }
                return;
            }

            // keyがなかったら中断
            if (!useList.ContainsKey(key))
                return;

            foreach (var pair in useList[key])
            {
                state[pair.Key] = source[pair.Key].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // フェードアウト or ストップ
            }
        }

        /// <summary>
        /// 全体を止める(clipが指定された時はそのclipの音を止める)
        /// </summary>
        /// <param name="clip">clip</param>
        protected override void StopTargetClip(AudioClip clip = null)
        {
            var key = clip != null ? clip.name : null;
            StopTargetKey(key);
        }

        #region Play key

        /// <summary>
        /// <para>再生</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がtrueでノーマル再生中は再生出来ない</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がfalseでクロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public void Play(string key, CancellationToken cancellationToken = default)
            => PlayAsync(key, null, false, false, cancellationToken).Forget();

        /// <summary>
        /// <para>再生Async</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がtrueでノーマル再生中は再生出来ない</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がfalseでクロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayAsync(string key, CancellationToken cancellationToken = default)
            => await PlayAsync(key, null, false, false, cancellationToken);

        /// <summary>
        /// <para>ノーマルフェード再生</para>
        /// <para>クロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public void PlayNormalfade(string key, CancellationToken cancellationToken = default)
            => PlayAsync(key, null, true, false, cancellationToken).Forget();

        /// <summary>
        /// <para>ノーマルフェード再生Async</para>
        /// <para>クロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayNormalfadeAsync(string key, CancellationToken cancellationToken = default)
            => await PlayAsync(key, null, true, false, cancellationToken);

        /// <summary>
        /// <para>クロスフェード再生</para>
        /// <para>ノーマル再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public void PlayCrossfade(string key, CancellationToken cancellationToken = default)
            => PlayAsync(key, null, false, true, cancellationToken).Forget();

        /// <summary>
        /// <para>クロスフェード再生Async</para>
        /// <para>ノーマル再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayCrossfadeAsync(string key, CancellationToken cancellationToken = default)
            => await PlayAsync(key, null, false, true, cancellationToken);

        #endregion
        #region Play clip

        /// <summary>
        /// <para>再生</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がtrueでノーマル再生中は再生出来ない</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がfalseでクロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public void Play(AudioClip clip, CancellationToken cancellationToken = default)
            => PlayAsync(null, clip, false, false, cancellationToken).Forget();

        /// <summary>
        /// <para>再生Async</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がtrueでノーマル再生中は再生出来ない</para>
        /// <para>IsCrossfade(クロスフェードフラグ)がfalseでクロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayAsync(AudioClip clip, CancellationToken cancellationToken = default)
            => await PlayAsync(null, clip, false, false, cancellationToken);

        /// <summary>
        /// <para>ノーマルフェード再生</para>
        /// <para>クロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public void PlayNormalfade(AudioClip clip, CancellationToken cancellationToken = default)
            => PlayAsync(null, clip, true, false, cancellationToken).Forget();

        /// <summary>
        /// <para>ノーマルフェード再生Async</para>
        /// <para>クロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayNormalfadeAsync(AudioClip clip, CancellationToken cancellationToken = default)
            => await PlayAsync(null, clip, true, false, cancellationToken);

        /// <summary>
        /// <para>クロスフェード再生</para>
        /// <para>ノーマル再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public void PlayCrossfade(AudioClip clip, CancellationToken cancellationToken = default)
            => PlayAsync(null, clip, false, true, cancellationToken).Forget();

        /// <summary>
        /// <para>クロスフェード再生Async</para>
        /// <para>ノーマル再生中は再生出来ない</para>
        /// </summary>
        /// <param name="clip">clip</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        public async UniTask PlayCrossfadeAsync(AudioClip clip, CancellationToken cancellationToken = default)
            => await PlayAsync(null, clip, false, true, cancellationToken);

        #endregion

        /// <summary>
        /// <para>再生Async</para>
        /// <para>isNormalfade=tureの時、クロス再生中は再生出来ない</para>
        /// <para>isCrossfade=tureの時、ノーマル再生中は再生出来ない</para>
        /// <para>isNormalfadeとisCrossfadeがfalseの時、
        /// IsCrossfade(クロスフェードフラグ)がtrueでノーマル再生中は再生出来ない</para>
        /// <para>isNormalfadeとisCrossfadeがfalseの時、
        /// IsCrossfade(クロスフェードフラグ)がfalseでクロス再生中は再生出来ない</para>
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="clip">clip</param>
        /// <param name="isNormalfade">isNormalfade</param>
        /// <param name="isCrossfade">isCrossfade</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsync(string key, AudioClip clip, bool isNormalfade = false, bool isCrossfade = false, CancellationToken cancellationToken = default)
        {
            // BGMが流れている時
            if (clipList.Count(x => x == null) < max)
            {
                if (isNormalfade)
                {
                    if (tempIsCrossfade)
                    {
                        Debug.LogError("It is not possible to normalfade during a crossfade.");
                        return;
                    }
                    else
                        tempIsNormalfade = true;
                }
                else if (isCrossfade)
                {
                    if (tempIsNormalfade)
                    {
                        Debug.LogError("It is not possible to crossfade during a normalfade.");
                        return;
                    }
                    else
                        tempIsCrossfade = true;
                }
                else
                {
                    if (IsCrossfade)
                    {
                        if (tempIsNormalfade)
                        {
                            Debug.LogError("It is not possible to crossfade during a normalfade.");
                            return;
                        }
                        else
                            tempIsCrossfade = true;
                    }
                    else
                    {
                        if (tempIsCrossfade)
                        {
                            Debug.LogError("It is not possible to normalfade during a crossfade.");
                            return;
                        }
                        else
                            tempIsNormalfade = true;
                    }
                }
            }
            await PlayAsync(key, clip, cancellationToken);
            if (tempIsCrossfade)
                tempIsCrossfade = false;
            else
                tempIsNormalfade = false;
        }

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

        /// <summary>
        /// フェードイン時間変更
        /// </summary>
        /// <param name="fadeInTime">fadeInTime</param>
        public void SetFadeInTime(float fadeInTime) => FadeInTime = fadeInTime;

        /// <summary>
        /// フェードアウト時間変更
        /// </summary>
        /// <param name="fadeOutTime">fadeOutTime</param>
        public void SetFadeOutTime(float fadeOutTime) => FadeOutTime = fadeOutTime;

        /// <summary>
        /// インターバル時間変更 (フェードアウトありで負数ならクロスフェード)
        /// </summary>
        /// <param name="intervalTime">intervalTime</param>
        public void SetIntervalTime(float intervalTime) => IntervalTime = intervalTime;

        /// <summary>
        /// クロスフェードフラグ変更 (フェードアウトありでインターバル時間が負数ならクロスフェード)
        /// </summary>
        /// <param name="isCrossfade">isCrossfade</param>
        public void SetIsCrossfade(bool isCrossfade) => IsCrossfade = isCrossfade;
    }
}