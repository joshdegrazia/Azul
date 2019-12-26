using Unity.Entities;

namespace Azul.Components {
    [GenerateAuthoringComponent]
    public struct Player : IComponentData {
        public int PlayerId;
        public int Score;
    }
}
