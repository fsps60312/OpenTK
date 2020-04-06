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
        Shader2D shader_plain_color;
        public GameWindow()
        {
            shader_plain_color=new Shader2D("SIFT.shaders.plain_color.glsl");
        }
        protected override void Init()
        {
            Param.Image(Canvas);
            shader_plain_color.Run(this.Width, this.Height);
        }
        protected override void Update(double secs)
        {
        }
        protected override void Render(double secs)
        {
        }
    }
}
