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
        static Dictionary<(string, int, int, int), MyGL.Program> program_pool = new Dictionary<(string, int, int, int), MyGL.Program>();
        protected ShaderBase(string source_name, int group_size_x, int group_size_y, int group_size_z)
        {
            if(!program_pool.TryGetValue((source_name,group_size_x,group_size_y,group_size_z),out program))
            {
                string source = IO.ReadResource(source_name);
                // inject code
                var lines = source.Split('\n').ToList();
                lines[0] = "#version 450";
                lines.Insert(1, $"layout(local_size_x = {group_size_x}, local_size_y = {group_size_y}, local_size_z = {group_size_z}) in;");
                source = string.Join("\n", lines);
                program = new MyGL.Program(
                   new MyGL.Shader(ShaderType.ComputeShader, source));
                program_pool.Add((source_name, group_size_x, group_size_y, group_size_z), program);
            }
        }
        protected void QueueForRun(int group_count_x, int group_count_y, int group_count_z)
        {
            program.Use();
            MyGL.CheckError(() => GL.DispatchCompute(group_count_x, group_count_y, group_count_z));
            //GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits); // seems not needed
            //MyGL.CheckError(()=> GL.Finish());
        }
        public void Uniform(string name, uint x) { program.Use(); program.Uniform(program.GetUniformLocation(name), x); }
        public void Uniform(string name, int x) { program.Use(); program.Uniform(program.GetUniformLocation(name), x); }
    }
}