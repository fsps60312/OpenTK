using System;

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
            var flag = new GPUArray<int>(new[] { 1 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.is_range.glsl").QueueForRun(Length);
            return flag[0] == 1;
        }
        public bool IsSorted()
        {
            var flag = new GPUArray<int>(new[] { 1 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.is_sorted.glsl").QueueForRun(Length);
            return flag[0] == 1;
        }
        public bool AllEqual(int value)
        {
            var flag = new GPUArray<int>(new[] { 1 });
            Param.Array(this, flag);
            new Shader($"SIFT.shaders.all_equal.glsl", p => p.Uniform("value", value)).QueueForRun(Length);
            return flag[0] == 1;
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
        private void BitonicSort()
        {
            int n = this.Length;
            GPUArray<int>
                buf1 = new GPUArray<int>(n) { Name = "_" },
                buf2 = new GPUArray<int>(n) { Name = "__" },
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
                Param.Array(t, l, r, buf1, buf2); new Shader("SIFT.shaders.bitonic_tree_pull_1.glsl").QueueForRun(n);
                Param.Array(buf1, l); new Shader("SIFT.shaders.copy.glsl").QueueForRun(n);
                Param.Array(buf2, r); new Shader("SIFT.shaders.copy.glsl").QueueForRun(n);
                Param.Array(t); new Shader("SIFT.shaders.bitonic_tree_pull_2.glsl").QueueForRun(n);
                cur_depth--;
            });
            var bitonic_merge = new Action<int>(level =>
            {
                Param.Array(this, t, l, r, buf1, shift); new Shader("SIFT.shaders.bitonic_merge.glsl", p => p.Uniform("level", level)).QueueForRun(n);
                Param.Array(buf1, this); new Shader("SIFT.shaders.copy.glsl").QueueForRun(n);
            });
            //OpenTK.Graphics.OpenGL.GL.Finish(); Console.WriteLine("a");
            while (!l.IsRange()) tree_push();
            int max_depth = cur_depth;
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
                //OpenTK.Graphics.OpenGL.GL.Finish(); Console.WriteLine("b");
                while (cur_depth > sort_depth)
                {
                    tree_pull();
                }
                //OpenTK.Graphics.OpenGL.GL.Finish(); Console.WriteLine("c");
            }
            //if (!this.IsSorted()) throw new Exception();
        }
        public void Sort()
        {
            BitonicSort();
        }
        public GPUArray<T> Clone()
        {
            var ret = new GPUArray<T>(this.Length);
            Param.Array(this, ret); new Shader($"SIFT.shaders.copy.glsl").QueueForRun(Length);
            return ret;
        }
    }
}
