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
    abstract partial class GameBase
    {
        protected Random Rand { get; private set; } = new Random();
        protected abstract class ShaderBase
        {
            private MyGL.Program program;
            protected ShaderBase(string source_name,int group_size_x,int group_size_y,int group_size_z)
            {
                string source = IO.ReadResource(source_name);
                // inject code
                var lines = source.Split('\n').ToList();
                lines.Insert(1, $"layout(local_size_x = {group_size_x}, local_size_y = {group_size_y}, local_size_z = {group_size_z}) in;");
                source = string.Join("\n", lines);
                program = new MyGL.Program(
                   new MyGL.Shader(ShaderType.ComputeShader, source));
            }
            protected void QueueForRun(int group_count_x, int group_count_y, int group_count_z)
            {
                program.Use();
                MyGL.CheckError(() => GL.DispatchCompute(group_count_x, group_count_y, group_count_z));
                //GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits); // seems not needed
            }
            public void Uniform(string name, uint x) { program.Use(); program.Uniform(program.GetUniformLocation(name), x); }
            public void Uniform(string name, int x) { program.Use(); program.Uniform(program.GetUniformLocation(name), x); }
        }
        protected class Shader:ShaderBase
        {
            const int default_group_size_x = 64;
            public Shader(string source_name, Action<Shader> init_action = null):base(source_name, default_group_size_x, 1, 1)
            {
                init_action?.Invoke(this);
            }
            public void QueueForRun(int n)
            {
                QueueForRun((n + default_group_size_x - 1) / default_group_size_x, 1, 1);
            }
        }
        protected class Shader2D:ShaderBase
        {
            const int default_group_size_x = 8;
            const int default_group_size_y = 8;
            public Shader2D(string source_name, Action<Shader2D> init_action = null) :base(source_name, default_group_size_x, default_group_size_y, 1)
            {
                init_action?.Invoke(this);
            }
            public void Run(int n,int m)
            {
                QueueForRun((n + default_group_size_x - 1) / default_group_size_x,
                    (m + default_group_size_y - 1) / default_group_size_y, 
                    1);
            }
        }
    }
        class GPUImage
        {
            private MyGL.Texture texture = new MyGL.Texture();
            public GPUImage(int width,int height)
            {
                texture.Bind();
                texture.TextureStorage2DVec4(width, height);
            }
            public void Bind(int location,TextureAccess access)
            {
                texture.BindImage(location, access, SizedInternalFormat.Rgba16f);
            }
        }
}
