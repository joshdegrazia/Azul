using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Azul.Systems.Initialization {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeFactoryTileSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<FactoryTile>()
                         .ForEach((ref RequiresInitialization requiresInitialization, in Entity entity) => {
                commandBuffer.AddBuffer<FactoryTileContentsElement>(entity);
                requiresInitialization.Value = false;
            }).Run();

            commandBuffer.Playback(base.EntityManager);
            commandBuffer.Dispose();

            return default;
        }
    }
}
