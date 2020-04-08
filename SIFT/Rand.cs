using System;
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
}
