using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    partial class GPUArray<T> : GPUArray where T : struct
    {
        public void Random()
        {
            Param.Array(this);
            new Shader($"SIFT.shaders.fill_random.glsl", p => p.Uniform("initial_seed", (uint)Rand.Next())).QueueForRun(Length);
        }
        public void Range()
        {
            Param.Array(this);
            new Shader($"SIFT.shaders.fill_range.glsl").QueueForRun(Length);
        }
        public bool IsRange()
        {
            var flag = new GPUArray<int>(new[] { 0 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.not_range.glsl").QueueForRun(Length);
            return flag[0] == 0;
        }
        public bool IsSorted()
        {
            var flag = new GPUArray<int>(new[] { 0 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.not_sorted.glsl").QueueForRun(Length);
            return flag[0] == 0;
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
            new Shader($"SIFT.shaders.contains_value.glsl", p => p.Uniform("value", value)).QueueForRun(Length);
            return flag[0] == 1;
        }
    }
}
