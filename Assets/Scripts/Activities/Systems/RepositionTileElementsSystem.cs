using Activities.Components;
using Azul.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Activities.Systems {
    [UpdateAfter(typeof(MoveFromFactoryTileToSelectedAreaSystem))]
    public class RepositionTileElementsSystem : JobComponentSystem {
        private EntityQuery PropsQuery;
        private EntityQuery CenterTileQuery;
        private EntityQuery SelectionAreaQuery;
        private EntityQuery CenterTileContentsQuery;
        private EntityQuery SelectionAreaContentsQuery;

        private const float CenterTileContentsRadius = 3;
        private const float SelectionAreaContentsRadius = 1;

        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(RepositionTileElementsProps));
            this.CenterTileQuery = base.GetEntityQuery(typeof(CenterFactoryTile));
            this.SelectionAreaQuery = base.GetEntityQuery(typeof(SelectionArea));
        }
        
        // todo: this can likely be refactored into a single system (something like "RadiallyPositionedElements" with a Recalculate trigger)
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            if (this.PropsQuery.CalculateEntityCount() == 0) {
                return default;
            }

            UnityEngine.Debug.Log("running system");
            
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            BufferFromEntity<FactoryTileContentsElement> factoryTileContentsData = base.GetBufferFromEntity<FactoryTileContentsElement>();
            base.Entities.WithAll<CenterFactoryTile>()
                         .WithoutBurst()
                         .ForEach((in Entity entity, in LocalToWorld localToWorld) => {
                UnityEngine.Debug.Log("run center tiles");
                DynamicBuffer<FactoryTileContentsElement> tileContents = factoryTileContentsData[entity];

                for (int i = 0; i < tileContents.Length; i++) {
                    entityCommandBuffer.SetComponent(tileContents[i].TileEntity, new Translation {
                        Value = localToWorld.Position + this.PositionAroundCircle(CenterTileContentsRadius, i, tileContents.Length)
                    });
                }
            }).Run();

            BufferFromEntity<SelectionAreaContentsElement> selectionAreaContentsData = base.GetBufferFromEntity<SelectionAreaContentsElement>();
            base.Entities.WithAll<SelectionArea>()
                         .WithoutBurst()
                         .ForEach((in Entity entity, in LocalToWorld localToWorld) => {
                UnityEngine.Debug.Log("run selected area");
                DynamicBuffer<SelectionAreaContentsElement> tileContents = selectionAreaContentsData[entity];

                for (int i = 0; i < tileContents.Length; i++) {
                    entityCommandBuffer.SetComponent(tileContents[i].TileEntity, new Translation {
                        Value = localToWorld.Position + this.PositionAroundCircle(SelectionAreaContentsRadius, i, tileContents.Length)
                    });
                }
            }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return default;
        }

        private float3 PositionAroundCircle(float radius, int index, int max) {
            float angle = (index * 2 * math.PI) / max;
            return new float3(radius * math.cos(angle), 0, radius * math.sin(angle));
        }
    }
}
