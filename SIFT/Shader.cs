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
            public void QueueForRun(int n, params object[] ps)
            {
                base.QueueForRun((n + default_group_size_x - 1) / default_group_size_x, 1, 1, ps);
            }
        }
    }
}