using System.Threading.Tasks;
using GameSystem.Audio;

namespace Core.Interfaces
{
    public interface IAudioService : IGameService
    {
        Task InitializeAsync();
        void PlayMusic(string clipName, float fadeInDuration = 1f, bool loop = true);
        void PlaySfx(string clipName, Vector3? position = null);
        void SetVolume(AudioChannelType channelType, float volume);
        float GetVolume(AudioChannelType channelType);
    }
}
