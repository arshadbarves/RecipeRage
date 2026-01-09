namespace Core.Audio
{
    public interface IAudioVolumeController
    {
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetMute(bool mute);
        float GetMasterVolume();
        float GetMusicVolume();
        float GetSFXVolume();
    }
}
