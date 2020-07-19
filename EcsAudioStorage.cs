using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace Arc.ECSAudio
{
    public static class EcsAudioStorage
    {
        public static Dictionary<Guid, AudioClip> RegisteredAudioClips = new Dictionary<Guid, AudioClip>();

        public static Dictionary<Guid, AudioMixerGroup> RegisteredAudioMixerGroups =
            new Dictionary<Guid, AudioMixerGroup>();

        public static Dictionary<Guid, AnimationCurve> RegisteredCustomCurves =
            new Dictionary<Guid, AnimationCurve>();

        public static Guid Register<T>(T objectToRegister, Dictionary<Guid, T> register, Func<T, T, bool> areEqual)
        {
            var registeredObject = register.Where(pair =>
            {
                if (objectToRegister == null && pair.Value == null) return true;
                if (objectToRegister == null || pair.Value == null) return false;
                return areEqual(pair.Value, objectToRegister);
            }).ToList();
            if (registeredObject.Any())
            {
                return registeredObject.First().Key;
            }
            else
            {
                var guid = Guid.NewGuid();
                register.Add(guid, objectToRegister);
                return guid;
            }
        }
    }
}