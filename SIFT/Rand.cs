using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIFT
{
    static class Rand
    {
        static Random rand = new Random();
        public static int Next() { return rand.Next(); }
        public static int Next(int maxValue) { return rand.Next(maxValue); }
    }
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
    }
}
