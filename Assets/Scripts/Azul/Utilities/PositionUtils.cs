using Unity.Mathematics;

namespace Azul.Utilities {
    public static class PositionUtils {
        public static float3 PositionAroundCircle(float radius, int index, int max) {
            float angle = (index * 2 * math.PI) / max;
            return new float3(radius * math.cos(angle), 0, radius * math.sin(angle));
        }
    }
}
