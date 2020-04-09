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
            int power = 3;
            int n = 1 << power;
            GPUIntArray s_gpu = new GPUIntArray(n);
            List<int> s_cpu = new List<int>(n);for (int i = 0; i < n; i++) s_cpu.Add(0);
            GL.Finish();
            TimeSpan a, b;
            Print("GPU time:", a=Timing(() =>
            {
                for (int i = 0; i < n * power * 11; i++)
                {
                    s_gpu[Rand.Next(n)] = Rand.Next();
                }
                GL.Finish();
            }));
            Print("GPU time:", b=Timing(() =>
            {
                for (int i = 0; i < 1; i++)
                {
                    //Print("i =", i);
                    s_gpu.Data(Shuffled(Range(n)).ToArray());
                    s_gpu.Sort();
                    Assert(s_gpu.IsSorted());
                }
                GL.Finish();
            }));
            Print(b.TotalSeconds / a.TotalSeconds);
            Print("CPU time:", Timing(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    //Print("i =", i);
                    s_cpu.Random();
                    s_cpu.Sort();
                    Assert(s_cpu.IsSorted());
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
