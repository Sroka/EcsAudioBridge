using System;
using System.Collections.Generic;
using Arc.ECSAudio.Components;
using Arc.SystemGroups;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Arc.ECSAudio
{
    internal struct PlayingAudioSource
    {
        public AudioSource AudioSource;
    }

    [UpdateInGroup(typeof(GameplaySimulationGroup))]
    public class PlayAudioSystem : SystemBase
    {
        private Dictionary<Guid, PlayingAudioSource> PlayingSources = new Dictionary<Guid, PlayingAudioSource>();
        private Queue<AudioSource>                   UnusedSources  = new Queue<AudioSource>();

        private GameplaySimulationCommandBufferSystem _gameplaySimulationCommandBufferSystem;
        private Camera                                _camera;

        protected override void OnCreate()
        {
            _gameplaySimulationCommandBufferSystem = World.GetOrCreateSystem<GameplaySimulationCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null) return;
            var playingSources        = PlayingSources;
            var unusedSources         = UnusedSources;
            var audioListenerPosition = _camera.transform.position;
            var commandBuffer         = _gameplaySimulationCommandBufferSystem.CreateCommandBuffer();
            var time                  = (float) Time.ElapsedTime;
            Entities
               .WithName("audio_process_proximity_to_listener_entities")
               .ForEach((ref EcsAudioSource                          playAudioContinuously,
                         ref PlayEcsAudioSourceOnProximityToListener playAudioOnProximityToListener,
                         in  LocalToWorld                            localToWorld) =>
                {
                    if (playAudioOnProximityToListener.OneShot)
                    {
                        if (playAudioOnProximityToListener.Triggered) return;
                        if (!(distance(localToWorld.Position, audioListenerPosition) <
                              playAudioOnProximityToListener.MinProximity)) return;
                        playAudioContinuously.Playing            = true;
                        playAudioContinuously.OneShot            = true;
                        playAudioOnProximityToListener.Triggered = true;
                    }
                    else
                    {
                        playAudioContinuously.Playing = distance(localToWorld.Position, audioListenerPosition) <
                                                        playAudioOnProximityToListener.MinProximity;
                        playAudioContinuously.OneShot = false;
                    }
                })
               .Run();
            Entities
               .WithName("audio_process_entities_with_playing_state")
               .WithoutBurst()
               .ForEach((Entity                entity,
                         ref EcsAudioSource    playAudioContinuously,
                         in  PlayingAudioState playAudioContinuouslyState,
                         in  LocalToWorld      localToWorld) =>
                {
                    var audioSource = PlayingSources[playAudioContinuouslyState.AudioSourceId].AudioSource;
                    audioSource.transform.position = localToWorld.Position;

                    bool stopPlaying;
                    if (playAudioContinuously.OneShot)
                    {
                        if (playAudioContinuously.Playing)
                        {
                            var clip = EcsAudioStorage.RegisteredAudioClips[
                                playAudioContinuously.AudioData.RegisteredAudioClipGuid];
                            audioSource.PlayOneShot(clip);
                            playAudioContinuously.Playing            = false;
                            playAudioContinuously.PlayStartTimestamp = time;
                        }

                        stopPlaying = time > playAudioContinuously.PlayStartTimestamp +
                            playAudioContinuously.PlayLength;
                    }
                    else
                    {
                        stopPlaying = !playAudioContinuously.Playing;
                    }

                    if (stopPlaying)
                    {
                        ReturnSourceToPool(playAudioContinuouslyState.AudioSourceId, playingSources, unusedSources);
                        commandBuffer.RemoveComponent<PlayingAudioState>(entity);
                    }
                })
               .Run();
            Entities
               .WithName("audio_process_entities_without_playing_state")
               .WithoutBurst()
               .WithNone<PlayingAudioState>()
               .ForEach(
                    (Entity entity, ref EcsAudioSource playAudioContinuously, in LocalToWorld localToWorld) =>
                    {
                        if (playAudioContinuously.Playing)
                        {
                            var clip = EcsAudioStorage.RegisteredAudioClips[
                                playAudioContinuously.AudioData.RegisteredAudioClipGuid];
                            if (clip == null)
                            {
                                Debug.Log("Cannot play - null clip");
                                return;
                            }
                            var audioMixerGroup =
                                EcsAudioStorage.RegisteredAudioMixerGroups[
                                    playAudioContinuously.AudioData.RegisteredAudioMixerGroupGuid];
                            var playingAudioSource  = new PlayingAudioSource();
                            var pooledAudioSourceId = PoolSource(playingSources, unusedSources, ref playingAudioSource);
                            playAudioContinuously.PlayStartTimestamp = time;
                            playAudioContinuously.PlayLength         = clip.length;
                            commandBuffer.AddComponent(entity, new PlayingAudioState
                            {
                                AudioSourceId = pooledAudioSourceId,
                            });
                            playingAudioSource.AudioSource.transform.position = localToWorld.Position;
                            playingAudioSource.AudioSource.clip = clip;
                            playingAudioSource.AudioSource.outputAudioMixerGroup = audioMixerGroup;
                            playingAudioSource.AudioSource.loop = !playAudioContinuously.OneShot;
                            playingAudioSource.AudioSource.pitch = playAudioContinuously.AudioData.Pitch;
                            playingAudioSource.AudioSource.volume = playAudioContinuously.AudioData.Volume;
                            playingAudioSource.AudioSource.priority = playAudioContinuously.AudioData.Priority;
                            playingAudioSource.AudioSource.spatialize = playAudioContinuously.AudioData.Spatialize;
                            playingAudioSource.AudioSource.bypassEffects =
                                playAudioContinuously.AudioData.BypassEffects;
                            playingAudioSource.AudioSource.bypassListenerEffects =
                                playAudioContinuously.AudioData.BypassListenerEffects;
                            playingAudioSource.AudioSource.bypassReverbZones =
                                playAudioContinuously.AudioData.BypassReverbZones;
                            playingAudioSource.AudioSource.panStereo    = playAudioContinuously.AudioData.PanStereo;
                            playingAudioSource.AudioSource.spatialBlend = playAudioContinuously.AudioData.SpatialBlend;
                            playingAudioSource.AudioSource.reverbZoneMix =
                                playAudioContinuously.AudioData.ReverbZoneMix;
                            playingAudioSource.AudioSource.dopplerLevel = playAudioContinuously.AudioData.DopplerLevel;
                            playingAudioSource.AudioSource.spread       = playAudioContinuously.AudioData.Spread;
                            playingAudioSource.AudioSource.rolloffMode  = playAudioContinuously.AudioData.RolloffMode;
                            playingAudioSource.AudioSource.minDistance  = playAudioContinuously.AudioData.MinDistance;
                            playingAudioSource.AudioSource.maxDistance  = playAudioContinuously.AudioData.MaxDistance;
                            if (playAudioContinuously.OneShot)
                            {
                                playingAudioSource.AudioSource.PlayOneShot(clip);
                                playAudioContinuously.Playing = false;
                            }
                            else
                            {
                                playingAudioSource.AudioSource.Play();
                            }
                        }
                    })
               .Run();
            Entities
               .WithoutBurst()
               .WithName("audio_process_destroyed_entity")
               .WithNone<EcsAudioSource>()
               .ForEach((Entity entity, in PlayingAudioState playAudioContinuouslyState) =>
                {
                    ReturnSourceToPool(playAudioContinuouslyState.AudioSourceId, playingSources, unusedSources);
                    commandBuffer.RemoveComponent<PlayingAudioState>(entity);
                })
               .Run();
        }

        private static Guid PoolSource(
            Dictionary<Guid, PlayingAudioSource> playingSources,
            Queue<AudioSource>                   unusedSources,
            ref PlayingAudioSource               playingAudioSource
        )
        {
            var audioSourceId = Guid.NewGuid();
            if (unusedSources.Count == 0)
            {
                playingAudioSource.AudioSource = new GameObject("EcsAudioSource").AddComponent<AudioSource>();
            }
            else
            {
                playingAudioSource.AudioSource = unusedSources.Dequeue();
                playingAudioSource.AudioSource.gameObject.SetActive(true);
            }

            playingSources.Add(audioSourceId, playingAudioSource);
            return audioSourceId;
        }

        private static void ReturnSourceToPool(Guid                                 audioSourceId,
                                               Dictionary<Guid, PlayingAudioSource> playingSources,
                                               Queue<AudioSource>                   unusedSources)
        {
            var pooledAudioSource = playingSources[audioSourceId];
            pooledAudioSource.AudioSource.gameObject.SetActive(false);
            pooledAudioSource.AudioSource.Stop();
            playingSources.Remove(audioSourceId);
            unusedSources.Enqueue(pooledAudioSource.AudioSource);
        }
    }
}