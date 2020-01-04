using Activities.Components;
using Azul.Components;
using Azul.Model;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Activities.Systems {
    [AlwaysSynchronizeSystem]
    public class CalculateEndOfRoundSystem : JobComponentSystem {
        private EntityQuery PropsQuery;
        private EntityQuery BoxQuery;
        
        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(CalculateEndOfRoundProps));
            this.BoxQuery = base.GetEntityQuery(typeof(Box));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.PropsQuery.CalculateEntityCount() == 0) {
                return default;
            }

            ComponentDataFromEntity<TileTypeComponent> tileTypeData = base.GetComponentDataFromEntity<TileTypeComponent>(true);
            ComponentDataFromEntity<LocalToWorld> localToWorldData = base.GetComponentDataFromEntity<LocalToWorld>();
            
            BufferFromEntity<TileSlotCollectionElement> tileSlotCollectionData = base.GetBufferFromEntity<TileSlotCollectionElement>();
            
            Entity boxEntity = this.BoxQuery.GetSingletonEntity();
            BufferFromEntity<BoxContentsElement> boxContentsData = base.GetBufferFromEntity<BoxContentsElement>();
            DynamicBuffer<BoxContentsElement> boxContents = boxContentsData[boxEntity];

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
                LocalToWorld boxLocalToWorld = localToWorldData[boxEntity];
                for (int i = patternLineCollection.Length - 1; i >= 1; i--) {
                    Entity tile = patternLineCollection[i].Contents;

                    boxContents.Add(new BoxContentsElement {
                        TileEntity = tile
                    });

                    entityCommandBuffer.SetComponent(tile, new Translation {
                        Value = boxLocalToWorld.Position
                    });
                }
                
                DynamicBuffer<TileSlotCollectionElement> wallRowCollection = tileSlotCollectionData[patternLine.WallRowEntity];

                for (int i = 0; i < wallRowCollection.Length; i++) {
                    TileSlotCollectionElement element = wallRowCollection[i];
                    TileTypeComponent tileSlotType = tileTypeData[element.TileSlot];

                    // there shouldn't be another tile here, so we won't check to see if the spot isn't occupied
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
