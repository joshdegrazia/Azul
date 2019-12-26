using Unity.Collections;
using Unity.Entities;

namespace Azul.Components {

    // worst case for a factory tile is, each player pulls 1 tile off each factory tile,
    // sending 3 to the center factory tile. most factory tiles can be 9, so 3*9=27
    [InternalBufferCapacity(27)]
    public struct FactoryTileContentsElement : IBufferElementData {
        public Entity TileEntity;
    }
}
