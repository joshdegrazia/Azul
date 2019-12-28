using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Utilities.Systems {
    [AlwaysSynchronizeSystem]
    public class RequiresInitializationEndFrameSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            Entities.WithAll<RequiresInitialization>()
                    .ForEach((Entity entity) => {
                        entityCommandBuffer.RemoveComponent<RequiresInitialization>(entity);
                    }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return inputDeps;
        }
    }
}
