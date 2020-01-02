using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct ParentTileSlotCollection : IComponentData{
        public Entity PatternLine;
    }
}
