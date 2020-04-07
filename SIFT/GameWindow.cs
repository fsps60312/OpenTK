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
        public GameWindow()
        {
            GPUArray<int> h = new GPUArray<int>(100000000);
            Param.Image(Canvas); new Shader2D("SIFT.shaders.plain_color.glsl").Run(this.Width, this.Height);
            for (int i = 0; i < 100; i++)
            {
                h.Value(0);
                MyGL.AssertError();
                Console.WriteLine(h.Contains(1));
                MyGL.AssertError();
                h[99999999] = 1;
                MyGL.AssertError();
                Console.WriteLine(h.Contains(1));
                MyGL.AssertError();
                h[99999999] = 0;
                MyGL.AssertError();
                Console.WriteLine(h.Contains(1));
                MyGL.AssertError();
                Console.WriteLine(h.Contains(0));
                MyGL.AssertError();
            }
        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
