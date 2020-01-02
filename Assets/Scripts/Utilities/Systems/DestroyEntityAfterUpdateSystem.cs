using Azul.Groups;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Utilities.Systems {

    // todo: find a way to deprecate end-frame component removal systems
    // RemoveComponentAfterUpdateSystem<T> caused an odd compile time errors, but i hadn't tried adding a subclass.
    [AlwaysSynchronizeSystem]
    [UpdateInGroup(typeof(EndFrameSystemGroup))]
    public class DestroyEntityAfterUpdateSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithAll<DestroyEntityAfterUpdate>()
                         .ForEach((ref Entity entity) => {
                             buffer.DestroyEntity(entity);
                         }).Run();

            buffer.Playback(base.EntityManager);
            buffer.Dispose();

            return inputDeps;
        }
    }
}
