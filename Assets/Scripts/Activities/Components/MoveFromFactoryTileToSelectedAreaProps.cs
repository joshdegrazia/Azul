using Azul.Model;
using Unity.Entities;

namespace Activities.Components {
    public struct MoveFromFactoryTileToSelectedAreaProps : IComponentData {
        public Entity FactoryTile;
        public TileType TileType;
    }
}
