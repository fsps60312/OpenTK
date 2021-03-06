﻿using System;
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
            new Shader2D("SIFT.shaders.plain_color.glsl").Run(Window.Width, Window.Height, Window.Canvas);
            //object a = new int();
            //Console.WriteLine(a.GetType());
            ////return;
            int n = 100000000;
            int loop_n = 1; // 60 * 32;
            GPUIntArray s_gpu = new GPUIntArray(n);
            List<int> s_cpu = new List<int>(n);for (int i = 0; i < n; i++) s_cpu.Add(0);

            Print(s_gpu.IsValue(7122));
            s_gpu.Value(7122);
            Print(s_gpu.IsValue(7122));

            GL.Finish();
            //Print("GPU time:", a=Timing(() =>
            //{
            //    for (int i = 0; i < n * power * 11; i++)
            //    {
            //        s_gpu[Rand.Next(n)] = Rand.Next();
            //    }
            //    GL.Finish();
            //}));
            TimeSpan a, b;
            Print("GPU time:",a= Timing(() =>
            {
                for (int i = 0; i < loop_n; i++)
                {
                    //Print("i =", i);
                    s_gpu.Random();
                    //s_gpu.Data(Shuffled(Range(n)).ToArray());
                    s_gpu.Sort();
                    //s_gpu.Value(0);
                    //if (!s_gpu.IsSorted())
                    //{
                    //    Print(s_gpu);
                    //}
                    Assert(s_gpu.IsSorted());
                }
                GL.Finish();
            }));
            Print("CPU time:",b= Timing(() =>
            {
                for (int i = 0; i < loop_n; i++)
                {
                    //Print("i =", i);
                    s_cpu.Random();
                    s_cpu.Sort();
                    Assert(s_cpu.IsSorted());
                }
            }));
            Print("speedup =", b.TotalMilliseconds / a.TotalMilliseconds);
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
