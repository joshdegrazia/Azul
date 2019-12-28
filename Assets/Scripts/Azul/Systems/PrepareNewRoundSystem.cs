using Azul.Components;
using Input.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Utilities.Systems;

namespace Azul.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(DestroyEntityAfterUpdateSystem))]
    public class PrepareNewRoundSystem : JobComponentSystem {
        private EntityQuery BagQuery;
        private EntityQuery PrepareNewRoundQuery;

        private Unity.Mathematics.Random Random;

        private Dictionary<int, float3> TileLocations;

        protected override void OnCreate() {
            this.BagQuery = base.GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { typeof(Bag) }
            });

            this.PrepareNewRoundQuery = base.GetEntityQuery(new EntityQueryDesc {
                All = new ComponentType[] { typeof(PrepareNewRound) }
            });

            this.Random = new Unity.Mathematics.Random();
            this.Random.InitState();

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

            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithAll<FactoryTile>()
                         .WithNone<CenterFactoryTile>()
                         .WithoutBurst()
                         .ForEach((Entity factoryTileEntity, in Translation translation) => {
                             DynamicBuffer<FactoryTileContentsElement> factoryTileContentsBuffer = entityCommandBuffer.AddBuffer<FactoryTileContentsElement>(factoryTileEntity);

                             while (bagTileBuffer.Length > 0 && factoryTileContentsBuffer.Length < 4) {
                                 int bagTileIndex = this.Random.NextInt(0, bagTileBuffer.Length);

                                 Entity tileEntity = bagTileBuffer[bagTileIndex].TileEntity;

                                 entityCommandBuffer.AddComponent(tileEntity, typeof(ListenForMouseClick));
                                 entityCommandBuffer.SetComponent(tileEntity, new Translation {
                                     Value = translation.Value + this.TileLocations[factoryTileContentsBuffer.Length]
                                 });

                                 factoryTileContentsBuffer.Add(new FactoryTileContentsElement {
                                     TileEntity = tileEntity
                                 });

                                 bagTileBuffer.RemoveAt(bagTileIndex);
                             }
                         }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return inputDeps;
        }
    }
}
