using Unity.Entities;

namespace Azul.Groups {
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public sealed class EndFrameSystemGroup : ComponentSystemGroup { }
}
