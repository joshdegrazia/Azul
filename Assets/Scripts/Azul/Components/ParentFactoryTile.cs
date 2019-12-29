using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct ParentFactoryTile : IComponentData {
        public Entity Value;
    }
}
