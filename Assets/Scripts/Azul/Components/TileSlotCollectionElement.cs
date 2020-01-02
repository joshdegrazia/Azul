using Unity.Entities;

namespace Azul.Components {
    public struct TileSlotCollectionElement : IBufferElementData {
        public Entity TileSlot;
        public Entity Contents;
    }
}
