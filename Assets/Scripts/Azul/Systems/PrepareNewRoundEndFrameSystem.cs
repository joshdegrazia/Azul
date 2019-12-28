using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Azul.Systems {
    // [AlwaysSynchronizeSystem]
    // public class PrepareNewRoundEndFrameSystem : JobComponentSystem {
    //     protected override JobHandle OnUpdate(JobHandle inputDeps) {
    //         EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

    //         Entities.WithAll<PrepareNewRound>()
    //                 .ForEach((Entity entity) => {
    //                     entityCommandBuffer.DestroyEntity(entity);
    //                 }).Run();

    //         entityCommandBuffer.Playback(base.EntityManager);
    //         entityCommandBuffer.Dispose();

    //         return inputDeps;
    //     }
    // }
}
