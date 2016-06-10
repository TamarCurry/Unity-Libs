using UnityEngine;

namespace Libs.Audio
{
    /// <summary>
    /// AudioClipExtensions
    /// </summary>
    public static class AudioClipExtensions
    {
        public static void PlaySfx(this AudioClip clip)
        {
            AudioManager.PlaySfx(clip);
        }
        public static void PlayVoice(this AudioClip clip)
        {
            AudioManager.PlayVoice(clip);
        }
        public static void PlayMusic(this AudioClip clip)
        {
            AudioManager.PlayMusic(clip);
        }
    }
}
