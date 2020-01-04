using Azul.Groups;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Utilities.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(EndFrameSystemGroup))]
    public class RequiresInitializationEndFrameSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            //? could make RequiresInitialization a shared component and only perform this job on false
            base.Entities.ForEach((ref Entity entity, ref RequiresInitialization requiresInitialization) => {
                if (!requiresInitialization.Value) {
                    entityCommandBuffer.RemoveComponent<RequiresInitialization>(entity);
                }
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return inputDeps;
        }
    }
}
