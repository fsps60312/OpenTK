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
        protected TimeSpan Timing(Action action)
        {
            var start_time = DateTime.Now;
            action.Invoke();
            return DateTime.Now - start_time;
        }
        protected void Assert(bool condition) { if (!condition) throw new Exception("assertion failed."); }
        protected void Print(object o)
        {
            Console.WriteLine(o);
        }
        protected void Print()
        {
            Console.WriteLine();
        }
    }
}
