using System;

namespace SIFT
{
    class Shader2D : ShaderBase
    {
        const int default_group_size_x = 8;
        const int default_group_size_y = 8;
        public Shader2D(string source_name, Action<Shader2D> init_action = null) : base(source_name, default_group_size_x, default_group_size_y, 1)
        {
            init_action?.Invoke(this);
        }
        public void Run(int n, int m)
        {
            QueueForRun((n + default_group_size_x - 1) / default_group_size_x,
                (m + default_group_size_y - 1) / default_group_size_y,
                1);
        }
    }
}