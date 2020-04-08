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
            for (int n = 1; n < 100; n++)
            {
                GPUArray<int>
                    s = new GPUArray<int>(n) { Name = "s" },
                    _ = new GPUArray<int>(n) { Name = "_" },
                    l = new GPUArray<int>(n) { Name = "l" },
                    r = new GPUArray<int>(n) { Name = "r" },
                    t = new GPUArray<int>(n) { Name = "t" },
                    shift = new GPUArray<int>(n) { Name = "shift" };
                t.Value(0); l.Value(0); r.Value(n - 1); shift.Value(0);
                int cur_depth = 0;
                var tree_push = new Action(() =>
                {
                    Param.Array(t, l, r, shift); new Shader("SIFT.shaders.bitonic_tree_push.glsl").QueueForRun(n);
                    cur_depth++;
                });
                var tree_pull = new Action(() =>
                {
                    Param.Array(t, l, r); new Shader("SIFT.shaders.bitonic_tree_pull.glsl").QueueForRun(n);
                    cur_depth--;
                });
                var bitonic_merge = new Action<int>(level =>
                {
                    Param.Array(s, t, l, r, _, shift); new Shader("SIFT.shaders.bitonic_merge.glsl", p => p.Uniform("level", level)).QueueForRun(n);
                    Param.Array(_, s); new Shader("SIFT.shaders.copy.glsl").QueueForRun(n);
                });
                while (!l.IsRange()) tree_push();
                int max_depth = cur_depth;
                s.Data(Shuffled(Range(n)).ToArray());
                while (cur_depth > 0)
                {
                    tree_pull();
                    int sort_depth = cur_depth;
                    int level = 0;
                    while (cur_depth < max_depth)
                    {
                        bitonic_merge(level);
                        tree_push(); level++;
                    }
                    while (cur_depth > sort_depth)
                    {
                        tree_pull();
                    }
                    Assert(cur_depth == sort_depth);
                }
                if (!s.IsRange()) Print("n=", n, ", s=", s);
            }
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
