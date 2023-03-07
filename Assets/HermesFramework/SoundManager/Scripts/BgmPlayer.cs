using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DateTime = System.DateTime;

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
        [SerializeField, Tooltip("フェードイン時間")] protected float fadeInTime = 0f;
        /// <summary>フェードアウト時間</summary>
        [SerializeField, Tooltip("フェードアウト時間")] protected float fadeOutTime = 3f;
        /// <summary>インターバル時間 (フェードアウトありで負数ならクロスフェード)</summary>
        [SerializeField, Tooltip("インターバル時間 (フェードアウトありで負数ならクロスフェード)")] protected float intervalTime = 0f;
        
        // メンバー要素
        /// <summary>再生状態</summary>
        protected eStatus[] state;
        /// <summary>直前の再生状態</summary>
        protected eStatus[] lastState;
        /// <summary>状態残存時間</summary>
        protected float[] remainTime;
        /// <summary>発生チャネル</summary>
        protected int playChannel;

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
            if (fadeInTime <= 0) { fadeInTime = 0; }
            if (fadeOutTime <= 0) { fadeOutTime = 0; }
            state = new eStatus[max];
            lastState = new eStatus[max];
            remainTime = new float[max];
            playChannel = 1;
        }

        /// <summary>
        /// 駆動
        /// </summary>
        protected virtual void Update()
        {
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
                            remainTime[i] = ((state[smsc] == eStatus.FADEOUT) ? fadeOutTime : 0) + intervalTime;
                            source[i].volume = 0f;
                            break;
                        case eStatus.FADEIN:
                            remainTime[i] = fadeInTime * (Volume - source[i].volume) / Volume;
                            if (!source[i].isPlaying)
                            {
                                source[i].Play();
                            }
                            break;
                        case eStatus.FADEOUT:
                            if (lastState[i] == eStatus.FADEIN)
                            {
                                remainTime[i] = fadeOutTime * source[i].volume / Volume;
                            }
                            else
                            {
                                remainTime[i] = fadeOutTime;
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
                                source[i].volume = coefficient * volume * (1f - remainTime[i] / fadeInTime);
                            }
                            else
                            {
                                state[i] = eStatus.PLAYING;
                            }
                            break;
                        case eStatus.FADEOUT:
                            if (remainTime[i] >= 0f)
                            {
                                source[i].volume = coefficient * volume * remainTime[i] / fadeOutTime;
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

        /// <summary>
        /// 全体を止める(keyが指定された時はそのkeyの音を止める)
        /// </summary>
        /// <param name="key"></param>
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

        protected async UniTask PlayAsync(string key)
        {
            var smsc = subChannel(playChannel); // 裏チャネル

            // 再生中の裏と一致
            if (useList.ContainsKey(key))
            {
                var keyIndex = useList[key][0].Key;
                if (keyIndex < 0)
                    return;
                if (source[smsc].isPlaying && source[smsc].clip == source[keyIndex].clip)
                {
                Debug.Log($"裏と一致 : musicActiveChannel = {musicActiveChannel} : playChannel = {playChannel}");
                    state[playChannel] = source[playChannel].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // 表を停止
                    state[smsc] = eStatus.FADEIN; // 裏を開始
                    playChannel = smsc; // 表裏入れ替え
                }
            }
            // どちらとも一致しない
            else
            {
                var i = clipList.IndexOf(null);
                // 同時再生数を超えていたら処理をしない
                if (i < 0)
                {
                    Debug.LogError("The number of simultaneous playback of sound is exceeded.");
                    return;
                }
                Debug.Log($"一致しない : musicActiveChannel = {musicActiveChannel} : playChannel = {playChannel}");
                playChannel = musicActiveChannel; // アクティブな方を表に
                smsc = subChannel(playChannel);
                state[playChannel] = source[playChannel].isPlaying ? eStatus.FADEOUT : eStatus.STOP; // 表をフェードアウト
                lastState[smsc] = eStatus.STOP; // 裏を即時停止
                source[smsc].Stop();
                source[smsc].volume = 0f;
                state[smsc] = eStatus.WAIT_INTERVAL; // 裏を開始
                playChannel = smsc; // 表裏入れ替え
                await PlayAsync(key, ePlayType.IfNot);
                // BGMが全て消えていたら
                if (clipList.Count(x => x == null) >= max)
                    playChannel = 1; // 初期化
            }
        }

        /// <summary>
        /// 再生
        /// </summary>
        /// <param name="key"></param>
        public void Play(string key) => PlayAsync(key).Forget();

        /// <summary>
		/// 停止
        /// </summary>
        /// <param name="key"></param>
        public void Stop(string key = null) => StopTargetKey(key);
    }

}