using System.Collections.Generic;
using Activities.Components;
using Azul.Components;
using Input.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Utilities.Components;
using Utilities.Systems;

namespace Activities.Systems {
    //? could make this job more flexible by abstracting out the positioning logic and
    //? adding Translations which were the origin + destinations
    // problem is adding parent components for each potential type of parent

    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(EnableFactoryTileMouseClickSystem))]
    [UpdateBefore(typeof(DestroyEntityAfterUpdateSystem))]
    public class MoveFromFactoryTileToSelectedAreaSystem : JobComponentSystem {
        private EntityQuery PropsQuery;
        private EntityQuery GameBoardQuery;
        private EntityQuery SelectionAreaQuery;
        private EntityQuery CenterTileQuery;
        private EntityQuery FactoryTileQuery;

        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(MoveFromFactoryTileToSelectedAreaProps));
            this.GameBoardQuery = base.GetEntityQuery(typeof(GameBoard));
            this.SelectionAreaQuery = base.GetEntityQuery(typeof(SelectionArea));
            this.CenterTileQuery = base.GetEntityQuery(typeof(CenterFactoryTile));
            this.FactoryTileQuery = base.GetEntityQuery(typeof(FactoryTile));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.PropsQuery.CalculateEntityCount() == 0) {
                return default;
            }

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            //? is there a way to cache this in OnCreate() with a guarantee that the entity will be created?
            // could have a private get method that checks for null and then fills
            Entity selectionAreaEntity = this.SelectionAreaQuery.GetSingletonEntity();
            Entity centerTileEntity = this.CenterTileQuery.GetSingletonEntity();
            Entity gameBoardEntity = this.GameBoardQuery.GetSingletonEntity();

            int factoryTileCount = this.FactoryTileQuery.CalculateEntityCount();

            BufferFromEntity<FactoryTileContentsElement> factoryTileBuffers = base.GetBufferFromEntity<FactoryTileContentsElement>();
            BufferFromEntity<SelectionAreaContentsElement> selectionAreaBuffers = base.GetBufferFromEntity<SelectionAreaContentsElement>();
            
            ComponentDataFromEntity<TileTypeComponent> tileTypeData = base.GetComponentDataFromEntity<TileTypeComponent>();
            ComponentDataFromEntity<Translation> translationData = base.GetComponentDataFromEntity<Translation>();
            
            base.Entities.WithoutBurst()
                         .ForEach((in MoveFromFactoryTileToSelectedAreaProps props) => {
                DynamicBuffer<FactoryTileContentsElement> factoryTileBuffer = factoryTileBuffers[props.FactoryTile];
                DynamicBuffer<SelectionAreaContentsElement> selectionAreaBuffer = selectionAreaBuffers[selectionAreaEntity];
                DynamicBuffer<FactoryTileContentsElement> centerTileBuffer = factoryTileBuffers[centerTileEntity];

                for (int i = factoryTileBuffer.Length - 1; i >= 0; i--) {
                    Entity tileEntity = factoryTileBuffer[i].TileEntity;
                    TileTypeComponent tileType = tileTypeData[tileEntity];

                    if (tileType.Value == props.TileType) {
                        selectionAreaBuffer.Add(new SelectionAreaContentsElement {
                            TileEntity = tileEntity
                        });

                        Translation translation = translationData[selectionAreaEntity];

                        entityCommandBuffer.SetComponent(tileEntity, new Translation {
                            Value = translation.Value
                        });

                        entityCommandBuffer.RemoveComponent<ParentFactoryTile>(tileEntity);
                        entityCommandBuffer.RemoveComponent<ListenForMouseClick>(tileEntity);

                        factoryTileBuffer.RemoveAt(i);
                    } else if (props.FactoryTile != centerTileEntity) {
                        Translation translation = translationData[centerTileEntity];
                    
                        entityCommandBuffer.SetComponent(tileEntity, new Translation {
                            Value = translation.Value
                        });

                        entityCommandBuffer.SetComponent(tileEntity, new ParentFactoryTile {
                            Value = centerTileEntity
                        });

                        centerTileBuffer.Add(new FactoryTileContentsElement {
                            TileEntity = tileEntity,
                        });

                        factoryTileBuffer.RemoveAt(i);
                    }
                }
                if (props.FactoryTile != centerTileEntity 
                    || (factoryTileCount == 1 && factoryTileBuffer.Length == 0)) {
                    entityCommandBuffer.SetComponent(gameBoardEntity, new FactoryTileCount {
                        Value = factoryTileCount - 1
                    });

                    entityCommandBuffer.DestroyEntity(props.FactoryTile);
                }

                if (selectionAreaBuffer.Length > 0) {
                    entityCommandBuffer.SetComponent(selectionAreaEntity, new TileTypeComponent {
                        Value = props.TileType
                    });
                }

                Entity disableFactoryTiles = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent<DestroyEntityAfterUpdate>(disableFactoryTiles);
                entityCommandBuffer.AddComponent(disableFactoryTiles, new EnableFactoryTileMouseClickProps {
                    Enabled = false
                });

                Entity repositionTiles = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent<DestroyEntityAfterUpdate>(repositionTiles);
                entityCommandBuffer.AddComponent<RepositionTileElementsProps>(repositionTiles);
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }
    }
}
