using Unity.Entities;

namespace Arc.ECSAudio.Components
{
    [GenerateAuthoringComponent]
    public struct PlayEcsAudioSourceOnProximityToListener : IComponentData
    {
        public   float MinProximity;
        public   bool  OneShot;
        internal bool  Triggered;
    }
}