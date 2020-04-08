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
            int n = 1<<27;
            GPUArray<int> s_gpu = new GPUArray<int>(n);
            List<int> s_cpu = new List<int>(n);for (int i = 0; i < n; i++) s_cpu.Add(0);
            Print("GPU time:", Timing(() =>
            {
                for (int i = 0; i < 1; i++)
                {
                    Print("i =", i);
                    s_gpu.Random();
                    s_gpu.Sort();
                }
            }));
            Print("CPU time:", Timing(() =>
            {
                for (int i = 0; i < 1; i++)
                {
                    Print("i =", i);
                    s_cpu.Random();
                    s_cpu.Sort();
                }
            }));
            Print("finish");
        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
