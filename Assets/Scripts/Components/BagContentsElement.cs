using Unity.Collections;
using Unity.Entities;

namespace Azul.Components {
    [InternalBufferCapacity(100)]
    public struct BagContentsElement : IBufferElementData {
        public Entity TileEntity;
    }
}
