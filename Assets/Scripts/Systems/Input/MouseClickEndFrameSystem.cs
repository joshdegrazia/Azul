using Unity.Entities;
using Unity.Jobs;

namespace Azul.Systems.Input {
    [AlwaysSynchronizeSystem]
    public class MouseClickEndFrameSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            return inputDeps;
        }
    }
}
