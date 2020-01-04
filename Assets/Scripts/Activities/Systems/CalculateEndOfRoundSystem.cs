using Activities.Components;
using Azul.Components;
using Azul.Model;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utilities.Systems;

namespace Activities.Systems {
    [AlwaysSynchronizeSystem]
    public class CalculateEndOfRoundSystem : JobComponentSystem {
        private EntityQuery PropsQuery;
        
        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(CalculateEndOfRoundProps));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.PropsQuery.CalculateEntityCount() == 0) {
                return default;
            }

            ComponentDataFromEntity<TileTypeComponent> tileTypeData = base.GetComponentDataFromEntity<TileTypeComponent>(true);
            ComponentDataFromEntity<LocalToWorld> localToWorldData = base.GetComponentDataFromEntity<LocalToWorld>();
            
            BufferFromEntity<TileSlotCollectionElement> tileSlotCollectionData = base.GetBufferFromEntity<TileSlotCollectionElement>();

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithoutBurst()
                         .ForEach((in Entity entity, in PatternLine patternLine, in TileTypeComponent patternLineType) => {
                if (patternLineType.Value == TileType.None) {
                    return;
                }

                DynamicBuffer<TileSlotCollectionElement> patternLineCollection = tileSlotCollectionData[entity];

                // don't empty the row if it contains any empty slots
                foreach (TileSlotCollectionElement element in patternLineCollection) {
                    if (element.Contents == Entity.Null) {
                        return;
                    }
                }

                // remove the tiles from the collection and put them in the box
                // for (int i = patternLineCollection.Length - 1; i >= 1; i--) {
                    
                // }
                
                DynamicBuffer<TileSlotCollectionElement> wallRowCollection = tileSlotCollectionData[patternLine.WallRowEntity];

                for (int i = 0; i < wallRowCollection.Length; i++) {
                    TileSlotCollectionElement element = wallRowCollection[i];
                    TileTypeComponent tileSlotType = tileTypeData[element.TileSlot];

                    // there shouldn't be another tile here, so we won't check to see if the spot isn't occupied
                    UnityEngine.Debug.Log(patternLineType.Value + " " + tileSlotType.Value + " " + tileSlotType.Value.Equals(patternLineType.Value));
                    if (tileSlotType.Value.Equals(patternLineType.Value)) {

                        Entity firstTileEntity = patternLineCollection[0].Contents;
                        LocalToWorld tileSlotLocalToWorld = localToWorldData[element.TileSlot];

                        wallRowCollection[i] = new TileSlotCollectionElement {
                            TileSlot = element.TileSlot,
                            Contents = firstTileEntity
                        };

                        entityCommandBuffer.SetComponent(firstTileEntity, new Translation {
                            Value = tileSlotLocalToWorld.Position
                        });

                        break;
                    }
                }
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
