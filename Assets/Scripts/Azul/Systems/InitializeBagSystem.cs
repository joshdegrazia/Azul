using Azul.Behaviours;
using Azul.Components;
using Azul.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

namespace Azul.Systems.Commands {
    [AlwaysSynchronizeSystem]
    [UpdateBefore(typeof(RequiresInitializationEndFrameSystem))]
    public class InitializeBagSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            
            base.Entities.WithAll<Bag>()
                    .WithAll<RequiresInitialization>()
                    .WithoutBurst()
                    .ForEach((Entity bagEntity, in Translation translation) => {
                        DynamicBuffer<BagContentsElement> buffer = entityCommandBuffer.AddBuffer<BagContentsElement>(bagEntity);

                        List<Entity> list = new List<Entity>(AzulConstants.TilesPerColor * 5);
                        for (int i = 0; i < AzulConstants.TilesPerColor; i++) {
                            foreach (Entity tilePrefab in PrefabStore.Instance.TilePrefabs) {
                                Entity newTileEntity = entityCommandBuffer.Instantiate(tilePrefab);

                                entityCommandBuffer.SetComponent(newTileEntity, new Translation {
                                    Value = translation.Value
                                });

                                buffer.Add(new BagContentsElement {
                                    TileEntity = newTileEntity
                                });
                            }
                        }
                    }).Run();

            entityCommandBuffer.Playback(base.EntityManager);
            entityCommandBuffer.Dispose();

            return inputDeps;
        }
    }
}
