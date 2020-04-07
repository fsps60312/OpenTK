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
        public GameWindow()
        {
            Param.Image(Canvas);
            new Shader2D("SIFT.shaders.plain_color.glsl").Run(this.Width, this.Height);

            int n = 16;
            GPUArray<int> a = new GPUArray<int>(n);
            GPUArray<int> b = new GPUArray<int>(n);
            GPUArray<int> c = new GPUArray<int>(n);
            for (int i = 0; i < n; i++)
            {
                a[i] = rand.Next(10);
                b[i] = rand.Next(10);
                c[i] = rand.Next(10);
            }

            int[] g = new int[n];

            Console.WriteLine("elementwise_sum");
            for (int i = 0; i < g.Length; i++) g[i] = a[i] + b[i];
            Param.Array(a, b, c);
            new Shader("SIFT.shaders.elementwise_sum.glsl").Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");
            Console.WriteLine($"g = {{{string.Join(", ", g.ToArray())}}}");

            Console.WriteLine("reduction_sum");
            Array.Copy(c.ToArray(), g, n);
            for (int i = 1; i < g.Length; i++) g[i] += g[i - 1];
            Param.Array(c);
            new Shader("SIFT.shaders.reduction_sum.glsl").Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");
            Console.WriteLine($"g = {{{string.Join(", ", g.ToArray())}}}");

            Console.WriteLine("shuffle");
            Param.Array(c);
            new Shader("SIFT.shaders.shuffle.glsl", p => p.Uniform("initial_seed", (uint)rand.Next())).Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");

            Console.WriteLine("bitonic_sort");
            Param.Array(c);
            new Shader("SIFT.shaders.bitonic_sort.glsl").Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");

            Console.WriteLine("fill_random");
            Param.Array(c);
            new Shader($"SIFT.shaders.fill_random.glsl", p => p.Uniform("initial_seed", (uint)rand.Next())).Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");

            Console.WriteLine("fill_range");
            Param.Array(c);
            new Shader($"SIFT.shaders.fill_range.glsl").Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");

            Console.WriteLine("fill_number");
            Param.Array(c);
            new Shader($"SIFT.shaders.fill_number.glsl", p => p.Uniform("value", 7122)).Run(c.Length);
            Console.WriteLine($"c = {{{string.Join(", ", c.ToArray())}}}");

            Console.WriteLine("bitonic_sort (heavy)");
            Console.WriteLine("create");
            GPUArray<int> h = new GPUArray<int>(1<<20);
            Console.WriteLine($"h = {{{string.Join(", ", h.GetRange(0, 10))}}}");
            Console.WriteLine("range");
            Param.Array(h); new Shader($"SIFT.shaders.fill_range.glsl").Run(h.Length);
            Console.WriteLine($"h = {{{string.Join(", ", h.GetRange(0, 10))}}}");
            Console.WriteLine("shuffle");
            Param.Array(h); new Shader($"SIFT.shaders.shuffle.glsl", p => p.Uniform("initial_seed", (uint)rand.Next())).Run(h.Length);
            Console.WriteLine($"h = {{{string.Join(", ", h.GetRange(0, 10))}}}");
            Console.WriteLine("bitonic_sort");
            Param.Array(h); new Shader($"SIFT.shaders.bitonic_sort.glsl").Run(h.Length);
            Console.WriteLine($"h = {{{string.Join(", ", h.GetRange(0, 10))}}}");

        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
