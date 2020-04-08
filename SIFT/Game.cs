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
    class Game:GameBase
    {
        //GPUArray<int> h = new GPUArray<int>(100000000);
        public Game(GameWindow window):base(window)
        {
            Param.Image(Window.Canvas); new Shader2D("SIFT.shaders.plain_color.glsl").Run(Window.Width, Window.Height);
            //object a = new int();
            //Console.WriteLine(a.GetType());
            ////return;
            int n = 16;
            GPUArray<int>
                s = new GPUArray<int>(n) { Name = "s" },
                d = new GPUArray<int>(n) { Name = "d" },
                l = new GPUArray<int>(n) { Name = "l" },
                r = new GPUArray<int>(n) { Name = "r" },
                t = new GPUArray<int>(n) { Name = "t" };
            t.Value(0); l.Value(0);r.Value(n - 1);
            Print(l);
            Print(r);
            Print();
            while (!l.IsRange())
            {
                Param.Array(t, l, r); new Shader("SIFT.shaders.tree_push.glsl").QueueForRun(n);
                Print(t);
                Print(l);
                Print(r);
                Print();
            }
            //for (int i = 0; i < 100; i++)
            //{
            //    //h.Value(0);
            //    //Console.WriteLine(h.Contains(1));
            //    //h[99999999] = 1;
            //    //Console.WriteLine(h.Contains(1));
            //    //h[99999999] = 0;
            //    //Console.WriteLine(h.Contains(1));
            //    //Console.WriteLine(h.Contains(0));
            //}
        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
