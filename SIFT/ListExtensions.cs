using System.Collections.Generic;

namespace SIFT
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Rand.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void Random(this IList<int> list)
        {
            for (int i = 0; i < list.Count; i++) list[i] = Rand.Next();
        }
        public static bool IsSorted(this IList<int> list)
        {
            for (int i = 1; i < list.Count; i++) if (list[i - 1] > list[i]) return false;
            return true;
        }
    }
}
