using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Hermes.Sound
{
    /// <summary>
    /// AudioPlayer
    /// </summary>
    public abstract class AudioPlayer : MonoBehaviour
    {
        /// <summary>ループさせるか</summary>
        protected abstract bool isLoop { get; }

        // オブジェクト要素
        /// <summary>同時再生数</summary>
        [SerializeField, Tooltip("同時再生数")] protected int max = 10;
        /// <summary>同じ音の同時再生数</summary>
        [SerializeField, Tooltip("同じ音の同時再生数")] protected int sameMax = 3;
        /// <summary>初期音量</summary>
        [SerializeField, Tooltip("初期音量"), Range(0f, 1f)] protected float initialVolume = 0.5f;
        /// <summary>再生しているクリップリスト</summary>
        [SerializeField, Tooltip("再生しているクリップリスト")] protected List<AudioClip> clipList = new List<AudioClip>();

        // メンバー要素
        /// <summary>ミュート</summary>
        protected bool soundMute = false;
        /// <summary>発生体</summary>
        protected AudioSource[] source;
        /// <summary>UseList</summary>
        protected Dictionary<string, List<KeyValuePair<int, bool>>> useList = new Dictionary<string, List<KeyValuePair<int, bool>>>();
        /// <summary>基準音量</summary>
        protected float volume;
        /// <summary>音量係数</summary>
        protected float coefficient = 1f;

        /// <summary>
        /// 初期化
        /// </summary>
        protected virtual void Awake()
        {
            volume = initialVolume;
            source = new AudioSource[max];
            for (var i = 0; i < source.Length; i++)
            {
                clipList.Add(null);
                source[i] = gameObject.AddComponent<AudioSource>();
                source[i].loop = isLoop;
                source[i].playOnAwake = false;
                source[i].Stop();
            }
        }

        /// <summary>
        /// 再生Async
        /// </summary>
        /// <param name="key"></param>
        /// <param name="playType"></param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsync(string key, ePlayType playType = ePlayType.Play)
        {
            var i = clipList.IndexOf(null);
            // 同時再生数を超えていたら処理をしない
            if (i < 0)
            {
                Debug.LogError("The number of simultaneous playback of sound is exceeded.");
                return;
            }

            // 存在していなかったら追加
            if (!useList.ContainsKey(key))
            {
                useList.Add(key, new List<KeyValuePair<int, bool>>());
                for (var l = 0; l < sameMax; l++)
                    useList[key].Add(new KeyValuePair<int, bool>(-1, false));
            }

            switch (playType)
            {
                // 同じ音を制限数まで鳴らす
                case ePlayType.Play:
                    await PlayAsyncTypePlay(key, i);
                    break;
                // 同じ音が鳴っていたら止めてから鳴らす
                case ePlayType.StopPlus:
                    await PlayAsyncTypeStopPlus(key, i);
                    break;
                // 同じ音が鳴っていない場合だけ鳴らす
                case ePlayType.IfNot:
                    await PlayAsyncTypeIfNot(key, i);
                    break;
            }
        }

        /// <summary>
        /// 同じ音を制限数まで鳴らす
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsyncTypePlay(string key, int i)
        {
            // 超えていたら古い音を最初から再生する
            if (useList[key].Count(x => x.Value) >= sameMax)
            {
                var time = 0f;
                var l = 0;
                foreach (var x in useList[key])
                {
                    if (source[x.Key].time > time)
                    {
                        l = x.Key;
                        time = source[x.Key].time;
                    }
                }
                source[l].Play();
            }
            // 新たに追加する
            else
            {
                await PlayAsyncCreate(key, i);
            }
        }

        /// <summary>
        /// 同じ音が鳴っていたら止めてから鳴らす
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsyncTypeStopPlus(string key, int i)
        {
            var time = 0f;
            var l = -1;
            foreach (var x in useList[key])
            {
                if (x.Key < 0)
                    continue;
                if (source[x.Key].time > time)
                {
                    l = x.Key;
                    source[l].Stop();
                    time = source[x.Key].time;
                }
            }
            if (l < 0)
                await PlayAsyncCreate(key, i);
            else
                source[l].Play();
        }

        /// <summary>
        /// 同じ音が鳴っていない場合だけ鳴らす
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsyncTypeIfNot(string key, int i)
        {
            // 鳴っていたら鳴らさない
            foreach (var use in useList[key])
                if (use.Value)
                    return;

            await PlayAsyncCreate(key, i);
        }

        /// <summary>
        /// 作成
        /// </summary>
        /// <param name="key"></param>
        /// <param name="i"></param>
        /// <returns>UniTask</returns>
        protected async UniTask PlayAsyncCreate(string key, int i)
        {
            var handle = Addressables.LoadAssetAsync<AudioClip>(key);
            await handle.ToUniTask();
            var result = handle.Result;

            clipList[i] = result;
            source[i].clip = result;
            source[i].Play();

            var l = useList[key].FindIndex(x => !x.Value);
            useList[key][l] = new KeyValuePair<int, bool>(i, true);
            await UniTask.WaitUntil(() => !source[i].isPlaying);

            Addressables.Release(handle);
            clipList[i] = null;
            source[i].clip = null;
            useList[key][l] = new KeyValuePair<int, bool>(-1, false);
            if (useList[key].Count(x => !x.Value) >= sameMax)
                useList.Remove(key);
        }

        /// <summary>
        /// 全体を止める(keyが指定された時はそのkeyの音を止める)
        /// </summary>
        /// <param name="key"></param>
        protected virtual void StopTargetKey(string key = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                foreach (var list in useList)
                {
                    foreach (var pair in list.Value)
                    {
                        if (pair.Key < 0)
                            continue;
                        source[pair.Key].Stop();
                    }
                }
                return;
            }

            // 鳴っていたら止める
            if (!useList.ContainsKey(key))
                return;

            foreach (var pair in useList[key])
            {
                if (pair.Key < 0)
                    continue;
                source[pair.Key].Stop();
            }
        }

        /// <summary>
        /// 音量設定
        /// </summary>
        /// <param name="value"></param>
        protected void SetVolume(float value)
        {
            if (value >= 0f && value <= 1f)
            {
                volume = value;
            }
            for (var i = 0; i < source.Length; i++)
            {
                source[i].volume = coefficient * volume;
            }
        }

        /// <summary>
        /// 一時的音量設定
        /// </summary>
        /// <param name="value"></param>
        protected void SetTmpVolume(float value)
        {
            if (value >= 0f && value <= 1f)
            {
                coefficient = value;
            }
            SetVolume(volume);
        }

        /// <summary>消音</summary>
        protected bool mute
        {
            get { return soundMute; }
            set
            {
                soundMute = value;
                for (var i = 0; i < source.Length; i++)
                {
                    source[i].mute = soundMute;
                }
                for (var i = 0; i < source.Length; i++)
                {
                    source[i].mute = soundMute;
                }
            }
        }

        /// <summary>効果音の音量</summary>
        public float Volume
        {
            get { return volume; }
            set { SetVolume(value); }
        }

        /// <summary>効果音の一時的音量</summary>
        public float TmpVolume
        {
            get { return coefficient; }
            set { SetTmpVolume(value); }
        }

        /// <summary>消音</summary>
        public bool Mute
        {
            get { return mute; }
            set { mute = value; }
        }
    }
}