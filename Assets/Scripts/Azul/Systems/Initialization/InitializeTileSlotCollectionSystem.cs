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
            
            base.Entities.WithAll<RequiresInitialization>()
                         .WithAll<TileSlotCollection>()
                         .ForEach((Entity entity) => {
                             entityCommandBuffer.AddBuffer<TileSlotCollectionElement>(entity);
                         }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
