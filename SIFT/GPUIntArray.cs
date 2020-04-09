using System;

namespace SIFT
{
    abstract partial class GameBase
    {
        protected partial class GPUIntArray : GPUArray<int>
        {
            public GPUIntArray() : base() { }
            public GPUIntArray(int n) : base(n) { }
            public GPUIntArray(int[] data) : base(data) { }
            public GPUIntArray(GPUIntArray array) : base(array) { }
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
                var flag = new GPUIntArray(new[] { 1 });
                Param.Array(this, flag);
                new Shader($"SIFT.shaders.is_range.glsl").QueueForRun(Length);
                return flag[0] == 1;
            }
            public bool IsSorted()
            {
                var flag = new GPUIntArray(new[] { 1 });
                Param.Array(this, flag);
                new Shader($"SIFT.shaders.is_sorted.glsl").QueueForRun(Length);
                return flag[0] == 1;
            }
            public bool AllEqual(int value)
            {
                var flag = new GPUIntArray(new[] { 1 });
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
                var flag = new GPUIntArray(new[] { 0 });
                Param.Array(this, flag);
                new Shader($"SIFT.shaders.contains_value.glsl", p => p.Uniform("value", value)).QueueForRun(Length);
                return flag[0] == 1;
            }
            #region Sort
            public void Sort()
            {
                new ParallelAdaptiveBitonicSorter(this).Sort();
            }
            private void BitonicSort()
            {
                int n = this.Length;
                GPUIntArray
                    buf1 = new GPUIntArray(n),
                    buf2 = new GPUIntArray(n),
                    l = new GPUIntArray(n),
                    r = new GPUIntArray(n),
                    t = new GPUIntArray(n),
                    shift = new GPUIntArray(n);
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
            class ParallelAdaptiveBitonicSorter
            {
                GPUIntArray value, left, rigt, output;
                public ParallelAdaptiveBitonicSorter(GPUIntArray array)
                {
                    this.value = array;
                    left = new GPUIntArray(array.Length);
                    rigt = new GPUIntArray(array.Length);
                    output = new GPUIntArray(array.Length);
                }
                private void PrintTree(int root)
                {
                    if (root == -1) return;
                    PrintTree(left[root]);
                    PrintN(value[root], " ");
                    PrintTree(rigt[root]);
                }
                private void BiMerge(int root, int spare, bool ascend)
                {
                    if (root == -1) return;
                    if ((value[root] < value[spare]) != ascend) // exchange 
                    {
                        (value[root], value[spare]) = (value[spare], value[root]);
                        (left[root], rigt[root]) = (rigt[root], left[root]);
                    }
                    (int p, int q) = (left[root], rigt[root]);
                    while (p != -1)
                    {
                        if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                        {
                            (value[p], value[q]) = (value[q], value[p]);
                            (left[p], left[q]) = (left[q], left[p]);
                            (p, q) = (rigt[p], rigt[q]);
                        }
                        else // go left
                        {
                            (p, q) = (left[p], left[q]);
                        }
                    }
                    BiMerge(left[root], root, ascend);
                    BiMerge(rigt[root], spare, ascend);
                }
                private void BiSort(int root, int spare, bool ascend)
                {
                    if (root == -1) return;
                    BiSort(left[root], root, true);
                    BiSort(rigt[root], spare, false);
                    BiMerge(root, spare, ascend);
                }
                private void Flatten(int root, int l, int r)
                {
                    int mid = (l + r) / 2;
                    output[mid] = value[root];
                    if (l == r) return;
                    Flatten(left[root], l, mid - 1);
                    Flatten(rigt[root], mid + 1, r);
                }
                private void PMerge(int n)
                {
                    Assert(__builtin_popcount(n) == 1);
                    int[] roots = new int[n], spares = new int[n];
                    if (false)
                    {
                        for (int start_level = 1; start_level < __builtin_ctz(n); start_level++)
                        {
                            for (int level = start_level; level >= 1; level--)
                            {
                                for (int id = 0; id < (n >> level); id++)
                                {
                                    Print("a");
                                    int i = (id << level) + (1 << (level - 1)) - 1;
                                    int origin_id = id + (n >> level);
                                    int root = roots[origin_id];
                                    int spare = spares[origin_id]; // 10101001111 -> 101010
                                    bool ascend = __builtin_popcount(i >> start_level) % 2 == 0;
                                    if ((value[root] < value[spare]) != ascend) // exchange 
                                    {
                                        (value[root], value[spare]) = (value[spare], value[root]);
                                        (left[root], rigt[root]) = (rigt[root], left[root]);
                                    }
                                    if (level > 1)
                                    {
                                        (int p, int q) = (left[root], rigt[root]);
                                        while (p != -1)
                                        {
                                            Print("c");
                                            if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                                            {
                                                (value[p], value[q]) = (value[q], value[p]);
                                                (left[p], left[q]) = (left[q], left[p]);
                                                (p, q) = (rigt[p], rigt[q]);
                                            }
                                            else // go left
                                            {
                                                (p, q) = (left[p], left[q]);
                                            }
                                        }
                                        (roots[origin_id * 2], spares[origin_id * 2]) = (left[root], root);
                                        (roots[origin_id * 2 + 1], spares[origin_id * 2 + 1]) = (rigt[root], spare);
                                    }
                                    Print("b");
                                }
                            }
                        }
                    }
                    else
                    {
                        //////////////////////////////////////////////////////
                        {
                            int level = 1;
                            for (int id = 0; id < n / 2; id++)
                            {
                                int i = id * 2;
                                int root = id + n / 2;
                                int spare = root >> (__builtin_ctz(~root) + 1); // 10101001111 -> 101010
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                            }
                        }
                        //////////////////////////////////////////////////////
                        {
                            //   [1]
                            // [2 , 3]
                            //[4,5,6,7]
                            // 1234567
                            int level = 2;
                            for (int id = 0; id < n / 4; id++)
                            {
                                int i = id * 4 + 1;
                                int root = id + n / 4;
                                int spare = root >> (__builtin_ctz(~root) + 1); // 10101001111 -> 101010
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                                (int p, int q) = (left[root], rigt[root]);
                                while (p != -1)
                                {
                                    if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                                    {
                                        (value[p], value[q]) = (value[q], value[p]);
                                        (left[p], left[q]) = (left[q], left[p]);
                                        (p, q) = (rigt[p], rigt[q]);
                                    }
                                    else // go left
                                    {
                                        (p, q) = (left[p], left[q]);
                                    }
                                }
                                (roots[root * 2], spares[root * 2]) = (left[root], root);
                                (roots[root * 2 + 1], spares[root * 2 + 1]) = (rigt[root], spare);
                                //BiMerge(left[root], root, ascend);
                                //BiMerge(rigt[root], spare, ascend);
                            }
                            for (int id = 0; id < n / 2; id++)
                            {
                                int i = id * 2;
                                int root = roots[id + n / 2];
                                int spare = spares[id + n / 2];
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                            }
                        }
                        //////////////////////////////////////////////////////
                        {
                            //   [1]
                            // [2 , 3]
                            //[4,5,6,7]
                            // 1234567
                            int level = 3;
                            for (int id = 0; id < n / 8; id++)
                            {
                                int i = id * 8 + 3;
                                int origin_id = id + n / 8;
                                int root = origin_id;
                                int spare = root >> (__builtin_ctz(~root) + 1); // 10101001111 -> 101010
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                                (int p, int q) = (left[root], rigt[root]);
                                while (p != -1)
                                {
                                    if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                                    {
                                        (value[p], value[q]) = (value[q], value[p]);
                                        (left[p], left[q]) = (left[q], left[p]);
                                        (p, q) = (rigt[p], rigt[q]);
                                    }
                                    else // go left
                                    {
                                        (p, q) = (left[p], left[q]);
                                    }
                                }
                                (roots[origin_id * 2], spares[origin_id * 2]) = (left[root], root);
                                (roots[origin_id * 2 + 1], spares[origin_id * 2 + 1]) = (rigt[root], spare);
                                //BiMerge(left[root], root, ascend);
                                //BiMerge(rigt[root], spare, ascend);
                            }
                            //   [1]
                            // [2 , 3]
                            //[4,5,6,7]
                            // 1234567
                            for (int id = 0; id < n / 4; id++)
                            {
                                int i = id * 4 + 1;
                                int origin_id = id + n / 4;
                                int root = roots[origin_id];
                                int spare = spares[origin_id]; // 10101001111 -> 101010
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                                (int p, int q) = (left[root], rigt[root]);
                                while (p != -1)
                                {
                                    if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                                    {
                                        (value[p], value[q]) = (value[q], value[p]);
                                        (left[p], left[q]) = (left[q], left[p]);
                                        (p, q) = (rigt[p], rigt[q]);
                                    }
                                    else // go left
                                    {
                                        (p, q) = (left[p], left[q]);
                                    }
                                }
                                (roots[origin_id * 2], spares[origin_id * 2]) = (left[root], root);
                                (roots[origin_id * 2 + 1], spares[origin_id * 2 + 1]) = (rigt[root], spare);
                                //BiMerge(left[root], root, ascend);
                                //BiMerge(rigt[root], spare, ascend);
                            }
                            //   [1]
                            // [2 , 3]
                            //[4,5,6,7]
                            // 1234567
                            for (int id = 0; id < n / 2; id++)
                            {
                                int i = id * 2;
                                int origin_id = id + n / 2;
                                int root = roots[origin_id];
                                int spare = spares[origin_id]; // 10101001111 -> 101010
                                bool ascend = __builtin_popcount(i >> level) % 2 == 0;
                                if ((value[root] < value[spare]) != ascend) // exchange 
                                {
                                    (value[root], value[spare]) = (value[spare], value[root]);
                                    (left[root], rigt[root]) = (rigt[root], left[root]);
                                }
                            }
                        }
                    }
                    Print("hi");
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    Assert(__builtin_popcount(n) == 1);
                    for (int i = 1; i < n; i++)
                    {
                        (left[i], rigt[i]) = ((i << 1) | 1) > n - 1 ? (-1, -1) : (i << 1, (i << 1) | 1);
                    }
                    PMerge(n);
                    Flatten(1, 0, n - 2);
                    output[n - 1] = value[0];
                    value.Swap(output);
                }
            }
            class AdaptiveBitonicSorter
            {
                GPUIntArray value, left, rigt, output;
                public AdaptiveBitonicSorter(GPUIntArray array)
                {
                    this.value = array;
                    left = new GPUIntArray(array.Length);
                    rigt = new GPUIntArray(array.Length);
                    output = new GPUIntArray(array.Length);
                }
                private void PrintTree(int root)
                {
                    if (root == -1) return;
                    PrintTree(left[root]);
                    PrintN(value[root]," ");
                    PrintTree(rigt[root]);
                }
                private void BiMerge(int root, int spare, bool ascend)
                {
                    if (root == -1) return;
                    if ((value[root] < value[spare]) != ascend) // exchange 
                    {
                        (value[root], value[spare]) = (value[spare], value[root]);
                        (left[root], rigt[root]) = (rigt[root], left[root]);
                    }
                    (int p, int q) = (left[root], rigt[root]);
                    while (p != -1)
                    {
                        if ((value[p] < value[q]) != ascend) // swap left tree of p & q, go right
                        {
                            (value[p], value[q]) = (value[q], value[p]);
                            (left[p], left[q]) = (left[q], left[p]);
                            (p, q) = (rigt[p], rigt[q]);
                        }
                        else // go left
                        {
                            (p, q) = (left[p], left[q]);
                        }
                    }
                    BiMerge(left[root], root, ascend);
                    BiMerge(rigt[root], spare, ascend);
                }
                private void BiSort(int root,int spare,bool ascend)
                {
                    if (root == -1) return;
                    BiSort(left[root], root, true);
                    BiSort(rigt[root], spare, false);
                    BiMerge(root, spare, ascend);
                }
                private void Flatten(int root,int l,int r)
                {
                    int mid = (l + r) / 2;
                    output[mid] = value[root];
                    if (l == r) return;
                    Flatten(left[root], l, mid - 1);
                    Flatten(rigt[root], mid + 1, r);
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    Assert(__builtin_popcount(n) == 1);
                    for (int i = 1; i < n; i++)
                    {
                        (left[i], rigt[i]) = ((i << 1) | 1) > n - 1 ? (-1, -1) : (i << 1, (i << 1) | 1);
                    }
                    int root = 1, spare = 0;
                    BiSort(root, spare, true);
                    Flatten(root, 0, n - 2);
                    output[n - 1] = value[spare];
                    value.Swap(output);
                }
            }
            #endregion
        }
    }
}
