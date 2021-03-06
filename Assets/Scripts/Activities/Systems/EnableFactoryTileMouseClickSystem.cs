using Activities.Components;
using Azul.Components;
using Input.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Utilities.Systems;

namespace Activities.Systems {
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(DestroyEntityAfterUpdateSystem))]
    public sealed class EnableFactoryTileMouseClickSystem : JobComponentSystem {
        private EntityQuery PropsQuery;

        private ComponentDataFromEntity<EnableFactoryTileMouseClickProps> PropsData;
        private ComponentDataFromEntity<ListenForMouseClick> ListenForMouseClickData;
        private BufferFromEntity<FactoryTileContentsElement> FactoryTileBufferData;

        protected override void OnCreate() {
            this.PropsQuery = base.GetEntityQuery(typeof(EnableFactoryTileMouseClickProps));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            int propsQueryCount = this.PropsQuery.CalculateEntityCount();
            if (propsQueryCount == 0) {
                return default;
            } else if (propsQueryCount > 1) {
                UnityEngine.Debug.LogWarning("Found two EnableFactoryTileMouseClick props this frame");
            }

            this.PropsData = base.GetComponentDataFromEntity<EnableFactoryTileMouseClickProps>(true);
            this.ListenForMouseClickData = base.GetComponentDataFromEntity<ListenForMouseClick>(false);
            this.FactoryTileBufferData = base.GetBufferFromEntity<FactoryTileContentsElement>(true);

            Entity propsEntity = this.PropsQuery.GetSingletonEntity();
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            base.Entities.WithAll<FactoryTile>()
                         .WithoutBurst()
                         .ForEach((in Entity entity) => {
                EnableFactoryTileMouseClickProps props = this.PropsData[propsEntity];
                DynamicBuffer<FactoryTileContentsElement> factoryTileBuffer = this.FactoryTileBufferData[entity];

                foreach (FactoryTileContentsElement element in factoryTileBuffer) {
                    if (props.Enabled && !this.ListenForMouseClickData.Exists(element.TileEntity)) {
                        commandBuffer.AddComponent<ListenForMouseClick>(element.TileEntity);
                    } else if (!props.Enabled && this.ListenForMouseClickData.Exists(element.TileEntity)) {
                        commandBuffer.RemoveComponent<ListenForMouseClick>(element.TileEntity);
                    }
                }
            }).Run();

            commandBuffer.Playback(base.EntityManager);
            commandBuffer.Dispose();

            return default;
        }
    }

}
