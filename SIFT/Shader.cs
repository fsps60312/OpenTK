using System;

namespace SIFT
{
    abstract partial class GameBase
    {
        class Shader : ShaderBase
        {
            const int default_group_size_x = 64;
            public Shader(string source_name, Action<Shader> init_action = null) : base(source_name, default_group_size_x, 1, 1)
            {
                init_action?.Invoke(this);
            }
            public void QueueForRunInSequence(int n, params object[] ps)
            {
                object[] new_ps = new object[ps.Length + 1];
                ps.CopyTo(new_ps, 1);
                for (int offset = 0; offset < n;  )
                {
                    int actual_group_count_x = Math.Min(max_group_count_x, ((n - offset) + group_size_x - 1) / group_size_x);
                    new_ps[0] = ("global_invocation_id_x_offset", (uint)offset);
                    base.QueueForRun(actual_group_count_x, 1, 1, new_ps);
                    offset += group_size_x * actual_group_count_x;
                }
            }
            //public void QueueForRun(int n, params object[] ps)
            //{
            //    base.QueueForRun((n + group_size_x - 1) / group_size_x, 1, 1, ps);
            //}
        }
    }
}