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
    abstract partial class GameWindowBase : OpenTK.GameWindow
    {
        protected static class Param
        {
            public static void Array(params GPUArray[] arrays)
            {
                for(int i = 0; i < arrays.Length; i++) arrays[i].Bind(i);
            }
            public static void Image(params GPUImage[]images)
            {
                for (int i = 0; i < images.Length; i++) images[i].Bind(i, TextureAccess.ReadWrite);
            }
        }
    }
}
