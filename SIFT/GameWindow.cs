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
    class GameWindow:GameWindowBase
    {
        Random rand = new Random();
        void TestGPU(bool block=true)
        {
            GPUArray<int> t = new GPUArray<int>(100);
            GPUArray<int> h = new GPUArray<int>(100000000);
            Param.Array(h,t);

            var shader_range = new Shader($"SIFT.shaders.fill_range.glsl");
            var shader_random = new Shader($"SIFT.shaders.fill_random.glsl");

            Console.WriteLine(Timing(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    shader_random.QueueForRun(h.Length);
                    shader_range.QueueForRun(h.Length);
                }
            }));
            if (block)
            {
                //Console.WriteLine(Timing(() =>
                //{
                //    Console.WriteLine($"t = {{{string.Join(", ", t.GetRange(0, 20))}}}");
                //}));

                //Console.WriteLine(Timing(() =>
                //{
                //    Console.WriteLine($"h = {{{string.Join(", ", h.GetRange(0, 20))}}}");
                //}));

                Console.WriteLine(Timing(() =>
                {
                    Console.WriteLine(h[0]);
                }));
            }
        }
        void PerformanceTest()
        {
            Console.WriteLine("CPU time:");
            if (false)
            {
                var h = new int[100000000];
                Console.WriteLine(Timing(() =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < h.Length; j++) h[j] = rand.Next();
                        for (int j = 0; j < h.Length; j++) h[j] = j;
                    }
                }));
            }
            TestGPU();
        }
        public GameWindow()
        {
            Console.WriteLine("A");
            TestGPU(false);
            Console.WriteLine("B");
            TestGPU(true);
            Console.WriteLine("finish");
            Param.Image(Canvas); new Shader2D("SIFT.shaders.plain_color.glsl").Run(this.Width, this.Height);
        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
