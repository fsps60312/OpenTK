using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    partial class GPUArray<T> : GPUArray where T : struct
    {
        public void Random()
        {
            Param.Array(this);
            new Shader($"SIFT.shaders.fill_random.glsl").QueueForRun(Length);
        }
        public void Range()
        {
            Param.Array(this);
            new Shader($"SIFT.shaders.fill_range.glsl").QueueForRun(Length);
        }
        public void Value(int value)
        {
            Param.Array(this);
            new Shader($"SIFT.shaders.fill_value.glsl", p => p.Uniform("value", value)).QueueForRun(Length);
        }
        public bool Contains(int value)
        {
            var flag = new GPUArray<int>(new[] { 0 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.search_value.glsl", p => p.Uniform("value", value)).QueueForRun(Length);
            return flag[0] == 1;
        }
    }
}
