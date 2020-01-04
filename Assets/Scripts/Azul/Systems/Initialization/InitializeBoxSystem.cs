using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Azul.Systems.Initialization {
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeBoxSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithAll<Box>()
                         .WithAll<RequiresInitialization>()
                         .ForEach((in Entity entity) => {
                entityCommandBuffer.AddBuffer<BoxContentsElement>(entity);
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
