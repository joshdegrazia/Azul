using Azul.Components;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Utilities.Components;

namespace Azul.Systems.Initialization {
    [UpdateAfter(typeof(InitializeTileSlotCollectionSystem))]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class InitializePatternLineSlotSystem : JobComponentSystem {
        protected override JobHandle OnUpdate(JobHandle inputDeps) {
            BufferFromEntity<TileSlotCollectionElement> tileSlotBuffers = base.GetBufferFromEntity<TileSlotCollectionElement>();
            ComponentDataFromEntity<TileSlotCollectionIndex> tileSlotIndices = base.GetComponentDataFromEntity<TileSlotCollectionIndex>();

            base.Entities.WithAll<TileSlot>()
                         .WithoutBurst()
                         .ForEach((ref RequiresInitialization requiresInitialization, in Entity tileSlot, in ParentTileSlotCollection parent, in TileSlotCollectionIndex index) => {
                             DynamicBuffer<TileSlotCollectionElement> tileSlotCollectionBuffer = tileSlotBuffers[parent.PatternLine];
                             
                             if (index.Value >= tileSlotCollectionBuffer.Length) {
                                 tileSlotCollectionBuffer.Add(new TileSlotCollectionElement {
                                     TileSlot = tileSlot,
                                     Contents = Entity.Null
                                 });
                             } else {
                                 // we assume here that the list is sorted
                                 for (int i = tileSlotCollectionBuffer.Length - 1; i >= 0; i -= 1) {
                                     TileSlotCollectionElement element = tileSlotCollectionBuffer[i];
                                     TileSlotCollectionIndex elementIndex = tileSlotIndices[element.TileSlot];

                                     if (elementIndex.Value <= tileSlotIndices[tileSlot].Value) {
                                         tileSlotCollectionBuffer.Insert(i + 1, new TileSlotCollectionElement {
                                             TileSlot = tileSlot,
                                             Contents = Entity.Null
                                         });

                                         return;
                                     }
                                 }

                                 Debug.LogError("Element was not inserted");
                             }

                             requiresInitialization.Value = false;
                         }).Run();

            base.Entities.WithAll<TileSlot>()
                         .WithNone<TileSlotCollectionIndex>()
                         .WithoutBurst()
                         .ForEach((ref RequiresInitialization requiresInitialization, in Entity tileSlot, in ParentTileSlotCollection parent) => {
                DynamicBuffer<TileSlotCollectionElement> tileSlotCollectionBuffer = tileSlotBuffers[parent.PatternLine];

                tileSlotCollectionBuffer.Add(new TileSlotCollectionElement {
                    TileSlot = tileSlot,
                    Contents = Entity.Null
                });

                requiresInitialization.Value = false;
            }).Run();

            return default;
        }
    }
}
