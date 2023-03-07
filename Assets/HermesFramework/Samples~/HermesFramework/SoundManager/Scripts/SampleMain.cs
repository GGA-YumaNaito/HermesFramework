using UnityEngine;
using UnityEngine.UI;

namespace Hermes.Sound.Sample
{
    /// <summary>メイン (参考)</summary>
    public class SampleMain : MonoBehaviour
	{
		// オブジェクト要素
		/// <summary>音量スライダー</summary>
		[SerializeField] private Slider effectVolumeSlider = default;
		[SerializeField] private Slider musicVolumeSlider = default;
		[SerializeField] private Toggle soundMuteToggle = default;

		// 効果音番号 ©効果音ラボ https://soundeffect-lab.info/
		public const string Voice_Started = "voice_info-lady1-kidoushimashita1";
		public const int SE_SwordGesture1 = 1;
		public const int SE_SwordSlash1 = 2;
		public const int SE_MagicFlame2 = 3;
		public const string SE_Thunderstorm1 = "se_thunderstorm1";
		// 楽曲音番号 ©魔王魂 https://maoudamashii.jokersounds.com/
		public const int BGM_Fantasy01 = 0;
		public const int BGM_Fantasy15 = 1;
		public const string BGM_Neorock83 = "bgm_maoudamashii_neorock83";
		public const int BGM_Orchestra16 = 3;

		/// <summary>初期化</summary>
		private void Start()
		{
			musicVolumeSlider.normalizedValue = SoundManager.BGM.Volume;
			effectVolumeSlider.normalizedValue = SoundManager.SE.Volume;
			soundMuteToggle.isOn = SoundManager.BGM.Mute;
			Debug.Log("Started");
            SoundManager.Voice.Play(Voice_Started); // 起動しました。
            SoundManager.SE.Play(SE_Thunderstorm1); // 雷雨
            SoundManager.BGM.Play(BGM_Neorock83); // ロックなBGM
        }

        #region voice

        /// <summary>ボイス再生</summary>
        public void OnPressVoiceButton(string key)
        {
            Debug.Log($"Play Voice {key}");
            SoundManager.Voice.Play(key);
		}

        /// <summary>ボイスボタン 同音が再生中なら止めてから再生</summary>
        public void OnPressVoiceStopPlusButton(string key)
        {
            Debug.Log($"Stop & Play Voice {key}");
            SoundManager.Voice.Play(key, ePlayType.StopPlus);
        }

        /// <summary>ボイスボタン 同音が再生中でなければ再生</summary>
        public void OnPressVoiceIfNotButton(string key)
        {
            Debug.Log($"Play Voice {key} If not playing");
            SoundManager.Voice.Play(key, ePlayType.IfNot);
        }

        /// <summary>ボイスボタン 停止</summary>
        public void OnPressVoiceStopButton(string key)
        {
            Debug.Log($"Stop Voice {key}");
            SoundManager.Voice.Stop(key);
        }

        #endregion
        #region se

        /// <summary>効果音ボタン</summary>
        public void OnPressSEButton(string key)
        {
            Debug.Log($"Play SE {key}");
            SoundManager.SE.Play(key);
        }

        /// <summary>効果音ボタン 同音が再生中なら止めてから再生</summary>
        public void OnPressSEStopPlusButton(string key)
        {
            Debug.Log($"Stop & Play SE {key}");
            SoundManager.SE.Play(key, ePlayType.StopPlus);
        }

        /// <summary>効果音ボタン 同音が再生中でなければ再生</summary>
        public void OnPressSEIfNotButton(string key)
        {
            Debug.Log($"Play SE {key} If not playing");
            SoundManager.SE.Play(key, ePlayType.IfNot);
        }

		/// <summary>効果音ボタン 停止</summary>
		public void OnPressSEStopButton(string key)
		{
			Debug.Log($"Stop SE {key}");
            SoundManager.SE.Stop(key);
		}

		/// <summary>効果音 音量設定</summary>
		public void OnChangeSEVolumeSlider()
		{
			Debug.Log($"SE Volue {effectVolumeSlider.normalizedValue}");
            SoundManager.SE.Volume = effectVolumeSlider.normalizedValue;
        }

        #endregion
        #region bgm

        /// <summary>楽曲音ボタン 切り替え</summary>
        public void OnPressBGMStopButton(string key)
        {
            Debug.Log($"Stop BGM {key}");
            SoundManager.BGM.Stop(key);
        }

        /// <summary>楽曲音ボタン 切り替え</summary>
        public void OnPressBGMButton(string key)
		{
			Debug.Log($"Play BGM {key}");
            SoundManager.BGM.Play(key);
		}

		/// <summary>楽曲音 音量設定</summary>
		public void OnChangeBGMVolumeSlider()
		{
			Debug.Log($"BGM Volue {musicVolumeSlider.normalizedValue}");
            SoundManager.BGM.Volume = musicVolumeSlider.normalizedValue;
		}

        #endregion

        /// <summary>消音トグル</summary>
        public void OnChangeMuteToggle()
		{
			Debug.Log($"Mute {soundMuteToggle.isOn}");
			soundMuteToggle.isOn = SoundManager.SE.Mute;
            SoundManager.Voice.Mute = soundMuteToggle.isOn;
            SoundManager.SE.Mute = soundMuteToggle.isOn;
            SoundManager.BGM.Mute = soundMuteToggle.isOn;
        }
	}
}