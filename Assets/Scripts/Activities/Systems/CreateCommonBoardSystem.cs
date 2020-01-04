using System.Collections.Generic;
using Activities.Components;
using Azul.Behaviours;
using Azul.Components;
using Azul.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace Activities.Systems {
    [AlwaysSynchronizeSystem]
    public class CreateCommonBoardSystem : JobComponentSystem {
        private const float factoryTileRadius = 8.5f;
        
        private static readonly Dictionary<int, int> FactoryTilesForPlayerCount = new Dictionary<int, int>() {
            { 2, 5 },
            { 3, 7 },
            { 4, 9 }
        };

        private EntityQuery PropsQuery;
        private EntityQuery GameBoardQuery;
        
        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(CreateCommonBoardProps));
            this.GameBoardQuery = base.GetEntityQuery(typeof(GameBoard));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            GameBoard gameBoard = this.GameBoardQuery.GetSingleton<GameBoard>();
            int factoryTileCount = CreateCommonBoardSystem.FactoryTilesForPlayerCount[gameBoard.PlayerCount];
            
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithoutBurst()
                         .WithAll<CreateCommonBoardProps>()
                         .ForEach((in Entity entity) => {
                for (int i = 0; i < factoryTileCount; i++) {
                    Entity factoryTileEntity = ecb.Instantiate(PrefabStore.Instance.FactoryTilePrefab);

                    ecb.SetComponent(factoryTileEntity, new Translation {
                        Value = PositionUtils.PositionAroundCircle(factoryTileRadius, i, factoryTileCount)
                    });
                }

                ecb.Instantiate(PrefabStore.Instance.CenterFactoryTilePrefab);
            }).Run();

            ecb.Playback(base.EntityManager);
            ecb.Dispose();

            return default;
        }
    }
}
