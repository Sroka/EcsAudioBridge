using System;
using UnityEngine;
using UnityEngine.Internal;

namespace Arc.ECSAudio.Components
{
    [System.Serializable]
    public struct AudioData
    {
        public Guid             RegisteredAudioClipGuid;
        public Guid             RegisteredAudioMixerGroupGuid;
        public float            Pitch;
        public float            Volume;
        public int              Priority;
        public bool             Spatialize;
        public bool             BypassEffects;
        public bool             BypassListenerEffects;
        public bool             BypassReverbZones;
        public float            PanStereo;
        public float            SpatialBlend;
        public float            ReverbZoneMix;
        public float            DopplerLevel;
        public float            Spread;
        public AudioRolloffMode RolloffMode;
        public float            MinDistance;
        public float            MaxDistance;
    }
}