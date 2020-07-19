using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Arc.ECSAudio.Components
{
    public struct EcsAudioSource : IComponentData
    {
        public bool      Playing;
        public bool      OneShot;
        public AudioData AudioData;

        internal float PlayStartTimestamp;
        internal float PlayLength;

        public void Play()
        {
            Playing = true;
        }

        public void PlayOneShot()
        {
            OneShot = true;
            Playing = true;
        }
    }

    [RequireComponent(typeof(AudioSource))]
    public class EcsAudioSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                throw new Exception(
                    "You can only use EcsAudioSource bridge on components having AudioSource component");

            var audioData = new AudioData();
            var audioClipGuid = EcsAudioStorage.Register(audioSource.clip, EcsAudioStorage.RegisteredAudioClips,
                                                         (item1, item2) =>
                                                             item1.GetInstanceID() == item2.GetInstanceID());
            var audioMixerGroupGuid = EcsAudioStorage.Register(audioSource.outputAudioMixerGroup,
                                                               EcsAudioStorage.RegisteredAudioMixerGroups,
                                                               (item1, item2) =>
                                                                   item1.GetInstanceID() == item2.GetInstanceID());
            audioData.RegisteredAudioClipGuid       = audioClipGuid;
            audioData.RegisteredAudioMixerGroupGuid = audioMixerGroupGuid;
            audioData.Volume                        = audioSource.volume;
            audioData.Pitch                         = audioSource.pitch;
            audioData.Priority                      = audioSource.priority;
            audioData.Spatialize                    = audioSource.spatialize;
            audioData.BypassEffects                 = audioSource.bypassEffects;
            audioData.BypassListenerEffects         = audioSource.bypassListenerEffects;
            audioData.BypassReverbZones             = audioSource.bypassReverbZones;
            audioData.PanStereo                     = audioSource.panStereo;
            audioData.SpatialBlend                  = audioSource.spatialBlend;
            audioData.ReverbZoneMix                 = audioSource.reverbZoneMix;
            audioData.DopplerLevel                  = audioSource.dopplerLevel;
            audioData.Spread                        = audioSource.spread;
            audioData.RolloffMode                   = audioSource.rolloffMode;
            if (audioData.RolloffMode == AudioRolloffMode.Custom)
            {
                audioData.MinDistance = 0f;
            }
            else
            {
                audioData.MinDistance = audioSource.minDistance;
            }

            audioData.MaxDistance = audioSource.maxDistance;

            dstManager.AddComponentData(entity, new EcsAudioSource
            {
                Playing   = audioSource.playOnAwake,
                OneShot   = !audioSource.loop,
                AudioData = audioData,
            });
        }
    }
}