using Azul.Model;
using Unity.Entities;

namespace Azul.Components {
    //? should this be an ISharedComponentData
    [GenerateAuthoringComponent]
    public struct TileTypeComponent : IComponentData {
        public TileType Value;
    }
}
