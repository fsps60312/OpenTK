using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    abstract class ShaderBase
    {
        private MyGL.Program program;
        protected ShaderBase(string source_name, int group_size_x, int group_size_y, int group_size_z)
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
}