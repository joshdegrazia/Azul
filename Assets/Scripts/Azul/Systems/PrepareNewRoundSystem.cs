using Azul.Components;
using Input.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Utilities;
using Utilities.Systems;

namespace Azul.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(DestroyEntityAfterUpdateSystem))]
    public class PrepareNewRoundSystem : JobComponentSystem {
        private EntityQuery BagQuery;
        private EntityQuery BoxQuery;
        private EntityQuery PrepareNewRoundQuery;

        private Unity.Mathematics.Random Random;

        private Dictionary<int, float3> TileLocations;

        protected override void OnCreate() {
            this.BagQuery = base.GetEntityQuery(typeof(Bag));
            this.BoxQuery = base.GetEntityQuery(typeof(Box));
            this.PrepareNewRoundQuery = base.GetEntityQuery(typeof(PrepareNewRound));

            this.Random = new Unity.Mathematics.Random();
            this.Random.InitState(RandomSeed.GetRandomSeed());

            this.TileLocations = new Dictionary<int, float3>();
            this.TileLocations.Add(0, new float3(1f, 0, 1f));
            this.TileLocations.Add(1, new float3(1f, 0, -1f));
            this.TileLocations.Add(2, new float3(-1f, 0, 1f));
            this.TileLocations.Add(3, new float3(-1f, 0, -1f));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.PrepareNewRoundQuery.CalculateEntityCount() <= 0) {
                return inputDeps;
            }

            Entity bagEntity = this.BagQuery.GetSingletonEntity();
            DynamicBuffer<BagContentsElement> bagTileBuffer = base.EntityManager.GetBuffer<BagContentsElement>(bagEntity);
            
            Entity boxEntity = this.BoxQuery.GetSingletonEntity();
            DynamicBuffer<BoxContentsElement> boxTileBuffer = base.EntityManager.GetBuffer<BoxContentsElement>(boxEntity);
            
            BufferFromEntity<FactoryTileContentsElement> factoryTileContentsBufferData = base.GetBufferFromEntity<FactoryTileContentsElement>();

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithAll<FactoryTile>()
                         .WithNone<CenterFactoryTile>()
                         .WithoutBurst()
                         .ForEach((Entity factoryTileEntity, in Translation translation) => {
                DynamicBuffer<FactoryTileContentsElement> factoryTileContentsBuffer = factoryTileContentsBufferData[factoryTileEntity];
                
                ParentFactoryTile parentComponent = new ParentFactoryTile {
                    Value = factoryTileEntity
                };

                while (bagTileBuffer.Length > 0 && factoryTileContentsBuffer.Length < 4) {
                    int bagTileIndex = this.Random.NextInt(0, bagTileBuffer.Length);

                    Entity tileEntity = bagTileBuffer[bagTileIndex].TileEntity;

                    entityCommandBuffer.AddComponent(tileEntity, typeof(ListenForMouseClick));
                    entityCommandBuffer.SetComponent(tileEntity, new Translation {
                        Value = translation.Value + this.TileLocations[factoryTileContentsBuffer.Length]
                    });
                    
                    entityCommandBuffer.AddComponent(tileEntity, parentComponent);

                    factoryTileContentsBuffer.Add(new FactoryTileContentsElement {
                        TileEntity = tileEntity
                    });

                    bagTileBuffer.RemoveAt(bagTileIndex);

                    if (bagTileBuffer.Length == 0) {
                        if (boxTileBuffer.Length == 0) {
                            return; // no more tiles 4 u :shrug:
                        }

                        for (int i = boxTileBuffer.Length - 1; i >= 0; i--) {
                            Entity tile = boxTileBuffer[i].TileEntity;
                            
                            bagTileBuffer.Add(new BagContentsElement {
                                TileEntity = tile
                            });
                        }
                    }
                }
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return inputDeps;
        }
    }
}
