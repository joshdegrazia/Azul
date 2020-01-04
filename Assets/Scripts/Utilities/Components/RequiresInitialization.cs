using System.Collections.Generic;
using Unity.Entities;

namespace Utilities.Components {
    [GenerateAuthoringComponent]
    public struct RequiresInitialization : IComponentData {
        public bool Value;
    }
}
