using UnityEngine;

namespace Hermes.Sound
{
    /// <summary>
    /// Voice, SE, BGMを管理するクラス
    /// </summary>
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
	{
		protected override bool isDontDestroyOnLoad => false;

        /// <summary>Voice</summary>
        [SerializeField] VoicePlayer voice;
        /// <summary>SE</summary>
		[SerializeField] SePlayer se;
        /// <summary>BGM</summary>
        [SerializeField] BgmPlayer bgm;

        /// <summary>Voice</summary>
        public static VoicePlayer Voice => Instance.voice;
        /// <summary>SE</summary>
        public static SePlayer SE => Instance.se;
        /// <summary>BGM</summary>
        public static BgmPlayer BGM => Instance.bgm;
    }
}