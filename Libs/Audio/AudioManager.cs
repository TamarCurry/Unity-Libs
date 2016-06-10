// Author : Tamar Curry

using UnityEngine;
using System.Collections.Generic;

namespace Libs.Audio
{ 
	/// <summary>
	/// Manages all audio played throughout the game.
	/// There are three kinds of audio the game plays: music, sound effects, and voice audio.
	/// We allocate one music instance, two voice audio instances, and the rest are sound effects.
	/// </summary>
	public class AudioManager {

		// CONSTANTS
		/// <summary>Maximum number of sounds to play at any one time.</summary>
		private const int MAX_SOUND_INSTANCES = 10;

		/// <summary>
		/// Key in PlayerPrefs for the audio enabling
		/// </summary>
		private const string KEY_ENABLED_AUDIO = "enabledAudio";

		/// <summary>
		/// Default audio flags
		/// </summary>
		private const uint DEFAULT_ENABLED_AUDIO = AudioInstance.SFX | AudioInstance.MUSIC | AudioInstance.VOICE;

		/// Actions
		private const uint PAUSE	= 1;
		private const uint RESUME	= 2;
		private const uint STOP		= 3;

		// PRIMITIVES
		private static uint _enabledAudio;

		// NULLABLES
		private static GameObject _gameObject;
		private static AudioInstance _music;
		private static List<AudioInstance> _audioInstances = new List<AudioInstance>();

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// GETTERS & SETTERS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------

		public static bool initialized { get; private set; }

		/// <summary>
		/// Gets or sets the enabled audio. Setting the value saves it to PlayerPrefs
		/// </summary>
		/// <value>The enabled audio flags.</value>
		private static uint enabledAudio {
			get { return _enabledAudio; }
			set {
				if (_enabledAudio != value) {
					_enabledAudio = value;
					PlayerPrefs.SetInt (KEY_ENABLED_AUDIO, (int)_enabledAudio);
				}
			}
		}

		/// <summary>Sets the volume for the music.</summary>
		public static float musicVolume {
			get { return _music.volume; }
			set { _music.volume = value; }
		}

		/// <summary>
		/// Returns whether or not the music is paused.
		/// </summary>
		public static bool isMusicPaused
		{
			get { return _music.isPaused; }
		}

		/// <summary>
		/// Returns whether or not the music is enabled.
		/// </summary>
		public static bool musicEnabled {
			get { return (_enabledAudio & AudioInstance.MUSIC) > 0; }
			set {
				if (value) {
					enabledAudio |= AudioInstance.MUSIC;
				}
				else {
					enabledAudio &= ~AudioInstance.MUSIC;
					StopMusic();
				}
			}
		}

		/// <summary>
		/// Returns whether or not sound effects are enabled.
		/// </summary>
		public static bool sfxEnabled {
			get { return (_enabledAudio & AudioInstance.SFX) > 0; }
			set
			{
				if (value)
				{
					enabledAudio |= AudioInstance.SFX;
				}
				else
				{
					enabledAudio &= ~AudioInstance.SFX;
					StopAllSfx();
				}
			}
		}

		/// <summary>
		/// Returns whether or not voice audio is enabled.
		/// </summary>
		public static bool voiceEnabled
		{
			get { return (_enabledAudio & AudioInstance.VOICE) > 0; }
			set
			{
				if (value)
				{
					enabledAudio |= AudioInstance.VOICE;
				}
				else
				{
					enabledAudio &= ~AudioInstance.VOICE;
					StopAllVoice();
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// METHODS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes the AudioManager.
		/// </summary>
		/// <param name="c"></param>
		public static void Init(Camera c)
		{
			if ( initialized ) { return; }
			initialized = true;

			_enabledAudio = PlayerPrefs.HasKey (KEY_ENABLED_AUDIO) ? (uint)PlayerPrefs.GetInt (KEY_ENABLED_AUDIO) : DEFAULT_ENABLED_AUDIO;
			_gameObject = new GameObject {name = "AudioEmitter"};
			_audioInstances = new List<AudioInstance>();

			for ( int i = 0; i < MAX_SOUND_INSTANCES; ++i )
			{
				AudioSource source = _gameObject.AddComponent<AudioSource>();

                // let's disable settings we don't need right now
                source.playOnAwake = false;
                source.bypassEffects = true;
                source.bypassListenerEffects = true;
                source.bypassReverbZones = true;

                uint audioType;
	            if (i == 0) { // one music instance
		            audioType = AudioInstance.MUSIC;
					source.priority = 0; // highest priority;
					source.loop = true; // looping;
				}
				else if (i < 3) { // two voice instances
					audioType = AudioInstance.VOICE;
					source.priority = i; // second highest priority
				}
				else { // sound effect instances
					audioType = AudioInstance.SFX;
				}
	            var instance = new AudioInstance(source, audioType);

	            if (audioType == AudioInstance.MUSIC) {
					// quick reference handler.
		            _music = instance;
	            }

                _audioInstances.Add(instance);
			}
			
			if ( c.gameObject.GetComponent<AudioListener>() == null)
			{
				c.gameObject.AddComponent<AudioListener>();
			}
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Performs the specified action for the specified audio types.
		/// If an AudioClip is provided, we'll only perform the action on AudioInstances that are
		/// playing that AudioClip.
		/// </summary>
		/// <param name="audioTypes"></param>
		/// <param name="action"></param>
		/// <param name="clip"></param>
		private static void HandleAudioInstancesOfType(uint audioTypes, uint action, AudioClip clip=null)
		{
			foreach (var audioInstance in _audioInstances)
			{
				if(clip != null && audioInstance.clip != clip) { continue; }
				if ((audioInstance.type & audioTypes) > 0)
				{
					if ( action == PAUSE ) { audioInstance.Pause(); }
					else if ( action == RESUME ) { audioInstance.Unpause(); }
					else if ( action == STOP ) { audioInstance.Stop(); }
				}
			}
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns an audio clip of the specified resource.
		/// </summary>
		/// <param name="audioId"></param>
		/// <returns></returns>
		public static AudioClip GetClip(string audioId)
		{
			return Resources.Load<AudioClip>(audioId);
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays an AudioClip for the specified type of audio.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="audioType"></param>
		/// <param name="volume"></param>
		/// <param name="minTimeBeforeRepeat"></param>
		private static void PlayAudio(AudioClip clip, uint audioType, float volume, int minTimeBeforeRepeat)
		{
			if (clip == null || (_enabledAudio & audioType) == 0) { return; }

			// if we're playing music, let's skip all the other bullsh*t
			if (audioType == AudioInstance.MUSIC) {
				_music.Play(clip, volume, 0);
				return;
			}

			int time = (int)(Time.realtimeSinceStartup * 1000);
			
			// first, check for repeats and if we can repeat
			foreach (AudioInstance i in _audioInstances)
			{
				if(i.type != audioType) { continue; }
				if (i.clip == clip && i.nextRepeatTime > time)
				{
					// can't play this sound effect again right now
					return;
				}
			}

			// play this clip in the first available audio source
			foreach (AudioInstance i in _audioInstances)
			{
				if (!i.isPlaying && i.type == audioType)
				{
					i.Play(clip, volume, time + minTimeBeforeRepeat);
					break;
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays a sound effect.
		/// </summary>
		/// <param name="sfxId"></param>
		/// <param name="volume"></param>
		/// <param name="minTimeBeforeRepeat"></param>
		public static void PlaySfx(string sfxId, float volume = 1, int minTimeBeforeRepeat = 0)
		{
			PlaySfx(GetClip(sfxId), volume, minTimeBeforeRepeat);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays a sound effect.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volume"></param>
		/// <param name="minTimeBeforeRepeat"></param>
		public static void PlaySfx(AudioClip clip, float volume = 1, int minTimeBeforeRepeat = 0)
		{
			PlayAudio(clip, AudioInstance.SFX, volume, minTimeBeforeRepeat);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays a voice sound effect.
		/// </summary>
		/// <param name="sfxId"></param>
		/// <param name="volume"></param>
		/// <param name="minTimeBeforeRepeat"></param>
		public static void PlayVoice(string sfxId, float volume = 1, int minTimeBeforeRepeat = 0)
		{
			PlayVoice(GetClip(sfxId), volume, minTimeBeforeRepeat);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays a voice sound effect.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volume"></param>
		/// <param name="minTimeBeforeRepeat"></param>
		public static void PlayVoice(AudioClip clip, float volume = 1, int minTimeBeforeRepeat = 0)
		{
			PlayAudio(clip, AudioInstance.VOICE, volume, minTimeBeforeRepeat);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays the specified music.
		/// </summary>
		/// <param name="musicId"></param>
		/// <param name="volume"></param>
		public static void PlayMusic(string musicId, float volume = 1)
		{
			PlayMusic(GetClip(musicId), volume);
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays the specified music.
		/// </summary>
		/// <param name="clip"></param>
		/// <param name="volume"></param>
		public static void PlayMusic(AudioClip clip, float volume = 1)
		{
			PlayAudio(clip, AudioInstance.MUSIC, volume, 0);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops a sound effect if it is playing.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void StopSfx(string sfxId)
		{
			StopSfx(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops a sound effect if it is playing.
		/// </summary>
		/// <param name="clip"></param>
		public static void StopSfx(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, STOP, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops a voice sound effect if it is playing.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void StopVoice(string sfxId)
		{
			StopVoice(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops a voice sound effect if it is playing.
		/// </summary>
		/// <param name="clip"></param>
		public static void StopVoice(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, STOP, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops the music.
		/// </summary>
		public static void StopMusic()
		{
			HandleAudioInstancesOfType(AudioInstance.MUSIC, STOP);
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops all sound effects.
		/// </summary>
		public static void StopAllSfx()
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, STOP);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops all voice sound effects.
		/// </summary>
		public static void StopAllVoice()
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, STOP);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops music and all sound effects.
		/// </summary>
		public static void StopAll()
		{
			HandleAudioInstancesOfType(0xffffffff, STOP);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses a sound effect if it is playing.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void PauseSfx(string sfxId)
		{
			PauseSfx(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses a sound effect if it is playing.
		/// </summary>
		/// <param name="clip"></param>
		public static void PauseSfx(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, PAUSE, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses a voice sound effect if it is playing.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void PauseVoice(string sfxId)
		{
			PauseVoice(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses a voice sound effect if it is playing.
		/// </summary>
		/// <param name="clip"></param>
		public static void PauseVoice(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, PAUSE, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses the music if it is playing.
		/// </summary>
		public static void PauseMusic()
		{
			HandleAudioInstancesOfType(AudioInstance.MUSIC, PAUSE);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses all sound effects.
		/// </summary>
		public static void PauseAllSfx()
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, PAUSE);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses all voice sound effects.
		/// </summary>
		public static void PauseAllVoice()
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, PAUSE);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses music and all sound effects.
		/// </summary>
		public static void PauseAll()
		{
			HandleAudioInstancesOfType(0xffffffff, PAUSE);
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses a sound effect if it was previously paused.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void UnpauseSfx(string sfxId)
		{
			UnpauseSfx(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses a sound effect if it was previously paused.
		/// </summary>
		/// <param name="clip"></param>
		public static void UnpauseSfx(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, STOP, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses a sound effect if it was previously paused.
		/// </summary>
		/// <param name="sfxId"></param>
		public static void UnpauseVoice(string sfxId)
		{
			UnpauseVoice(GetClip(sfxId));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses a sound effect if it was previously paused.
		/// </summary>
		/// <param name="clip"></param>
		public static void UnpauseVoice(AudioClip clip)
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, STOP, clip);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses the music if it was previously paused.
		/// </summary>
		public static void UnpauseMusic()
		{
			HandleAudioInstancesOfType(AudioInstance.MUSIC, RESUME);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes all paused sound effects.
		/// </summary>
		public static void UnpauseAllSfx()
		{
			HandleAudioInstancesOfType(AudioInstance.SFX, RESUME);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes all paused voice sound effects.
		/// </summary>
		public static void UnpauseAllVoice()
		{
			HandleAudioInstancesOfType(AudioInstance.VOICE, RESUME);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes music and all paused sound effects.
		/// </summary>
		public static void UnpauseAll()
		{
			HandleAudioInstancesOfType(0xffffffff, RESUME);
		}

	}
}
