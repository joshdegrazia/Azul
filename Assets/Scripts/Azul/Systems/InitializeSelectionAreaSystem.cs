using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;
using Utilities.Systems;

namespace Azul.Systems {
    [UpdateBefore(typeof(RequiresInitializationEndFrameSystem))]
    public class InitializeSelectionAreaSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<RequiresInitialization>()
                         .WithAll<SelectionArea>()
                         .ForEach((Entity entity) => {
                             entityCommandBuffer.AddBuffer<SelectionAreaContentsElement>(entity);
                         }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
