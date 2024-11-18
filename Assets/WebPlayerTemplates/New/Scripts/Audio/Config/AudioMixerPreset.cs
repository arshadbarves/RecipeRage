using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio.Config
{
    [CreateAssetMenu(fileName = "AudioMixerPreset", menuName = "RecipeRage/Audio/MixerPreset")]
    public class AudioMixerPreset : ScriptableObject
    {

        public AudioMixer mixer;
        public MixerGroupSettings[] groups;
        public SnapshotSettings[] snapshots;

        [Header("Mobile Optimization")]
        public SnapshotSettings mobileSnapshot;
        public bool useMobileOptimization = true;

        public AudioMixerGroup GetGroup(string groupName)
        {
            foreach (MixerGroupSettings group in groups)
            {
                if (group.name == groupName)
                    return group.group;
            }
            return null;
        }

        public SnapshotSettings GetSnapshot(string snapshotName)
        {
            foreach (SnapshotSettings snapshot in snapshots)
            {
                if (snapshot.name == snapshotName)
                    return snapshot;
            }
            return null;
        }
        [Serializable]
        public class MixerGroupSettings
        {
            public string name;
            public AudioMixerGroup group;
            public float defaultVolume;
            public float minVolume = -80f;
            public float maxVolume;
        }

        [Serializable]
        public class SnapshotSettings
        {
            public string name;
            public AudioMixerSnapshot snapshot;
            public float[] parameters;
            [Range(0f, 2f)]
            public float defaultTransitionTime = 0.5f;
        }
    }
}