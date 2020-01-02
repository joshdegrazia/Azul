using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct TileSlotCollectionIndex : IComponentData {
        public int Value;
    }
}
