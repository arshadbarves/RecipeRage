// using System.Collections.Generic;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Audio;
//
// namespace GameSystem
// {
//     public class AudioSystem
//     {
//         private AudioMixer _audioMixer;
//         private readonly Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();
//
//         [Inject] private IAudioService _audioService;
//
//         public async Task InitializeAsync()
//         {
//             _audioMixer = await Addressables.LoadAssetAsync<AudioMixer>("AudioMixer").Task;
//             await LoadAudioClips();
//         }
//
//         private async Task LoadAudioClips()
//         {
//             var clipHandles = await Addressables.LoadResourceLocationsAsync("AudioClips").Task;
//             foreach (var handle in clipHandles)
//             {
//                 var clip = await Addressables.LoadAssetAsync<AudioClip>(handle).Task;
//                 _audioClips[clip.name] = clip;
//             }
//         }
//
//         public void PlayMusic(string clipName) => _audioService.PlayMusic(GetClip(clipName));
//         public void PlaySfx(string clipName) => _audioService.PlaySFX(GetClip(clipName));
//
//         private AudioClip GetClip(string clipName) => _audioClips.GetValueOrDefault(clipName);
//
//         public void Update() { }
//
//         public async Task CleanupAsync()
//         {
//             await Addressables.ReleaseAsync(_audioMixer);
//             foreach (var clip in _audioClips.Values)
//             {
//                 await Addressables.ReleaseAsync(clip);
//             }
//             _audioClips.Clear();
//         }
//     }
// }