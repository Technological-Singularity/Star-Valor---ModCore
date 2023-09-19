using System;

namespace Charon.StarValor.ModCore {
    public static class Random {
        static System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        public static int Next(int min, int max) => random.Next(min, max);
    }
}
