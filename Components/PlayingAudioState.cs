using System;
using Unity.Entities;

namespace Arc.ECSAudio.Components
{
    public struct PlayingAudioState : ISystemStateComponentData
    {
        internal Guid AudioSourceId;
    }
}