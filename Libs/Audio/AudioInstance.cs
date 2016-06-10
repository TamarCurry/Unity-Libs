// Author : Tamar Curry

using UnityEngine;

namespace Libs.Audio
{
	/// <summary>
	/// Helper class that aids in playing AudioClips
	/// </summary>
	internal class AudioInstance {
		public const uint SFX	= 1;
		public const uint VOICE = 2;
		public const uint MUSIC = 4;

		// PRIMITIVES
		private bool _paused;
		
		// NULLABLES
		private AudioSource _source;

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// GETTERS & SETTERS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// The audio source this AudioInstance controls.
		/// </summary>
		public bool isPaused { get { return _paused; } }

		/// <summary>
		/// Returns whether or not audio is playing.
		/// </summary>
		public bool isPlaying { get { return _source.isPlaying; } }

		/// <summary>
		/// Returns the volume the audio is playing at.
		/// </summary>
		public float volume {
			get { return _source.volume; }
			set { _source.volume = value; }
		}
		
		/// <summary>
		/// Returns the current AudioClip
		/// </summary>
		public AudioClip clip {
			get { return _source.clip; }
		}
		
		/// <summary>
		/// Returns the next time sound using the same AudioClip is allowed to play.
		/// </summary>
		public int nextRepeatTime { get; private set; }

		public uint type { get; private set; }

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// METHODS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="audioType"></param>
		public AudioInstance(AudioSource source, uint audioType)
		{
			_source = source;
			type = audioType;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses the audio.
		/// </summary>
		public void Pause() {
			if (_source.isPlaying) {
				_paused = true;
				_source.Pause();
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Unpauses the audio.
		/// </summary>
		public void Unpause() {
			if (_paused) {
				_paused = false;
				_source.UnPause();
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Plays the specified AudioClip at the specified volume.
		/// </summary>
		/// <param name="audioClip"></param>
		/// <param name="audioVolume"></param>
		/// <param name="repeatTime"></param>
		public void Play(AudioClip audioClip, float audioVolume, int repeatTime) {
			Stop();
			nextRepeatTime = repeatTime;
			_source.clip = audioClip;
			_source.volume = audioVolume;
			_source.Play();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops the audio.
		/// </summary>
		public void Stop() {
			_paused = false;
			_source.Stop();
		}
	}
}
