using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct PatternLine : IComponentData {
        public Entity WallRowEntity;
    }
}
