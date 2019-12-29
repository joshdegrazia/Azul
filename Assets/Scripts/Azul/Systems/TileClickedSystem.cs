using Activities.Components;
using Activities.Systems;
using Azul.Components;
using Input.Components;
using Input.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Components;

namespace Azul.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateAfter(typeof(MouseClickSystem)),
     UpdateBefore(typeof(MoveFromFactoryTileToSelectedAreaSystem)),
     UpdateBefore(typeof(MouseClickEndFrameSystem))]
    public class TileClickedSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer buffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<MouseClick>()
                         .WithoutBurst()
                         .ForEach((in ParentFactoryTile parent, in TileTypeComponent type) => {
                             Entity activity = buffer.CreateEntity();
                             buffer.AddComponent(activity, typeof(DestroyEntityAfterUpdate));
                             buffer.AddComponent(activity, new MoveFromFactoryTileToSelectedAreaProps {
                                 FactoryTile = parent.Value,
                                 TileType = type.Value
                             });
                         }).Run();

            buffer.Playback(base.EntityManager);
            buffer.Dispose();

            return inputDeps;
        }
    }
}
