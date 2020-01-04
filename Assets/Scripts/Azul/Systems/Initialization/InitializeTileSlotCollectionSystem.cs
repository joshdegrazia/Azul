using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Azul.Systems.Initialization {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeTileSlotCollectionSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<TileSlotCollection>()
                         .ForEach((ref RequiresInitialization requiresInitialization, ref Entity entity) => {
                entityCommandBuffer.AddBuffer<TileSlotCollectionElement>(entity);
                requiresInitialization.Value = false;
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
