using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct TileSlotCollection : IComponentData {
        public int NumSlots;
        public int NumFilledSlots;
    }
}
