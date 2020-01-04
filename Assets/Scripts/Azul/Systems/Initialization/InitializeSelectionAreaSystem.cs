using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;
using Utilities.Systems;

namespace Azul.Systems.Initialization {
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializeSelectionAreaSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<SelectionArea>()
                         .ForEach((ref RequiresInitialization requiresInitialization, ref Entity entity) => {
                entityCommandBuffer.AddBuffer<SelectionAreaContentsElement>(entity);
                requiresInitialization.Value = false;
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
