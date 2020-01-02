using Azul.Components;
using Azul.Model;
using Input.Components;
using Input.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Azul.Systems {

    // todo: enable so that only the current player's board can be clicked this turn.
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(MouseClickEndFrameSystem))]
    public class TileSlotCollectionClickedSystem : JobComponentSystem {
        private EntityQuery SelectionAreaQuery;
        
        protected override void OnCreate() {
            this.SelectionAreaQuery = base.GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { typeof(SelectionArea) }
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            Entity selectionAreaEntity = this.SelectionAreaQuery.GetSingletonEntity();

            ComponentDataFromEntity<SelectionArea> selectionAreaData = base.GetComponentDataFromEntity<SelectionArea>(true);
            ComponentDataFromEntity<TileSlotCollection> tileSlotCollectionData = base.GetComponentDataFromEntity<TileSlotCollection>();
            ComponentDataFromEntity<TileTypeComponent> tileTypeData = base.GetComponentDataFromEntity<TileTypeComponent>();
            
            ComponentDataFromEntity<LocalToWorld> localToWorldData = base.GetComponentDataFromEntity<LocalToWorld>();
            
            BufferFromEntity<SelectionAreaContentsElement> selectionAreaBuffers = base.GetBufferFromEntity<SelectionAreaContentsElement>();
            BufferFromEntity<TileSlotCollectionElement> tileSlotBuffers = base.GetBufferFromEntity<TileSlotCollectionElement>();

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            //? it's super weird that this runs "on" the clickable area - it should probably raise an "activity"
            // todo: this works for standard play, but you'll have to select the rows for variant mode, so
            // todo: pattern lines won't be the only things being clicked.
            base.Entities.WithAll<MouseClick>()
                         .WithoutBurst()
                         .ForEach((in ParentTileSlotCollection parent) => {
                TileSlotCollection tileSlotCollection = tileSlotCollectionData[parent.PatternLine];

                TileTypeComponent selectionAreaType = tileTypeData[selectionAreaEntity];
                TileTypeComponent patternLineType = tileTypeData[parent.PatternLine];

                DynamicBuffer<SelectionAreaContentsElement> selectionAreaBuffer = selectionAreaBuffers[selectionAreaEntity];
                DynamicBuffer<TileSlotCollectionElement> tileSlots = tileSlotBuffers[parent.PatternLine];

                // still need to check if you haven't placed a tile on any other pattern lines.
                // though, after the first tile it may be better to just remove the click listeners
                // from other pattern lines.
                if (tileSlotCollection.NumSlots == tileSlotCollection.NumFilledSlots) {
                    return;
                } else if (patternLineType.Value != TileType.None 
                            && patternLineType.Value != selectionAreaType.Value) {
                    return;
                } else if (selectionAreaBuffer.Length == 0) {
                    return;
                }
                
                // move the tile into the open spot.
                Entity tile = selectionAreaBuffer[selectionAreaBuffer.Length - 1].TileEntity;
                selectionAreaBuffer.RemoveAt(selectionAreaBuffer.Length - 1);
                
                // todo: update tile slot collection so it tells you the next available index
                for (int i = 0; i < tileSlots.Length; i++) {
                    if (tileSlots[i].Contents == Entity.Null) {
                        tileSlots[i] = new TileSlotCollectionElement {
                            TileSlot = tileSlots[i].TileSlot,
                            Contents = tile
                        };

                        LocalToWorld tileSlotLocality = localToWorldData[tileSlots[i].TileSlot];
                        entityCommandBuffer.SetComponent(tile, new Translation {
                            Value = tileSlotLocality.Position
                        });

                        return;
                    }
                }
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
