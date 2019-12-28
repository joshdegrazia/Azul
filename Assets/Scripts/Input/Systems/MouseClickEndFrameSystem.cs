using Input.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Input.Systems {
    [AlwaysSynchronizeSystem]
    public class MouseClickEndFrameSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<MouseClick>()
                         .WithoutBurst()
                         .ForEach((ref Entity entity) => {
                             buffer.RemoveComponent(entity, typeof(MouseClick));
                         }).Run();

            buffer.Playback(base.EntityManager);
            buffer.Dispose();
            
            return inputDeps;
        }
    }
}
