using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct FactoryTileCount : IComponentData {
        public int Value;
    }
}
