using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    abstract partial class GameBase
    {
        protected GameWindow Window { get; private set; }
        protected GameBase(GameWindow window)
        {
            Window = window;
            window.Render += s => Render(s);
            window.Update += s => Update(s);
        }
        protected abstract void Update(double secs);
        protected abstract void Render(double secs);
        protected static TimeSpan Timing(Action action)
        {
            var start_time = DateTime.Now;
            action.Invoke();
            return DateTime.Now - start_time;
        }
        protected static void Assert(bool condition = false)
        {
            if (!condition)
            {
                Print("assertion failed.");
                Print(string.Join("", new System.Diagnostics.StackTrace(true).GetFrames().Select(f => f.ToString())));
                Console.ReadLine();
                throw new Exception("assertion failed.");
            }
        }
        protected static void PrintN(params object[] o)
        {
            Console.Write(string.Join(" ", o));
        }
        protected static void Print(params object[] o)
        {
            Console.WriteLine(string.Join(" ", o));
        }
        protected static void Print()
        {
            Console.WriteLine();
        }
        protected static List<int>Range(int n)
        {
            List<int> ret = new List<int>();
            for (int i = 0; i < n; i++) ret.Add(i);
            return ret;
        }
        protected static List<T> Shuffled<T>(List<T>l)
        {
            List<T> ret = new List<T>(l);
            ret.Shuffle();
            return ret;
        }
        protected static List<T>Sorted<T>(List<T>l)
        {
            List<T> ret = new List<T>(l);
            ret.Sort();
            return ret;
        }
        protected static bool IsRange(List<int> l)
        {
            for (int i = 0; i < l.Count; i++) if (l[i] != i) return false;
            return true;
        }
        protected static int __builtin_clz(int _v)
        {
            uint v = (uint)_v;
            if (v == 0) return 32;
            int ret = 1;
            while ((v << ret >> ret) == v) ret++;
            return ret - 1;
        }
        protected static int __builtin_ctz(int _v)
        {
            uint v = (uint)_v;
            if (v == 0) return 32;
            int ret = 1;
            while ((v >> ret << ret) == v) ret++;
            return ret - 1;
        }
        protected static int __builtin_popcount(int v)
        {
            unchecked
            {
                v = ((v & (int)0xaaaaaaaa) >> 1) + (v & 0x55555555);
                v = ((v & (int)0xcccccccc) >> 2) + (v & 0x33333333);
                v = ((v & (int)0xf0f0f0f0) >> 4) + (v & 0x0f0f0f0f);
                v = ((v & (int)0xff00ff00) >> 8) + (v & 0x00ff00ff);
                v = ((v & (int)0xffff0000) >> 16) + (v & 0x0000ffff);
                return v;
            }
        }
    }
}
