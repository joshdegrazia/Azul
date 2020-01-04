using System;

namespace Utilities {
    public static class RandomSeed {
        public static uint GetRandomSeed() {
            return (uint)((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) & uint.MaxValue);
        }
    }
}
