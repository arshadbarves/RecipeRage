namespace Playcenter.Services
{
    public interface IAudioService
    {
        void Play(string sfxId);
        void PlayMusic(string musicId);
        void StopMusic();
        void SetMasterVolume(float volume01);
        void SetMusicVolume(float volume01);
        void SetSfxVolume(float volume01);
    }
}
