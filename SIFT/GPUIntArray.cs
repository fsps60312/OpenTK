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
                new Shader($"SIFT.shaders.fill_random.glsl").QueueForRunInSequence(Length, this, ("initial_seed", (uint)Rand.Next()));
            }
            public void Range()
            {
                new Shader($"SIFT.shaders.fill_range.glsl").QueueForRunInSequence(Length, this);
            }
            public bool IsRange()
            {
                var flag = new GPUIntArray(new[] { 1 });
                new Shader($"SIFT.shaders.is_range.glsl").QueueForRunInSequence(Length, this, flag);
                return flag[0] == 1;
            }
            public bool IsSorted()
            {
                var flag = new GPUIntArray(new[] { 1 });
                new Shader($"SIFT.shaders.is_sorted.glsl").QueueForRunInSequence(Length, this, flag);
                return flag[0] == 1;
            }
            public bool IsValue(int value)
            {
                var flag = new GPUIntArray(new[] { 1 });
                new Shader($"SIFT.shaders.is_value.glsl").QueueForRunInSequence(Length, this, flag, ("value", value));
                return flag[0] == 1;
            }
            public void Value(int value)
            {
                new Shader($"SIFT.shaders.fill_value.glsl").QueueForRunInSequence(Length, this, ("value", value));
            }
            public void AddValue(int value)
            {
                new Shader($"SIFT.shaders.add_value.glsl").QueueForRunInSequence(Length, this, ("value", value));
            }
            public bool Contains(int value)
            {
                var flag = new GPUIntArray(new[] { 0 });
                new Shader($"SIFT.shaders.contains_value.glsl").QueueForRunInSequence(Length, this, flag, ("value", value));
                return flag[0] == 1;
            }
            #region Sort
            public void Sort()
            {
                new ParallelMergeSorter(this).Sort();
            }
            class ParallelMergeSorter
            {
                GPUIntArray value, output;
                GPUIntArray left, rigt;
                GPUIntArray a;
                //GPUIntArray debug;
                public ParallelMergeSorter(GPUIntArray array)
                {
                    this.value = array;
                    output = new GPUIntArray(array.Length);
                    left = new GPUIntArray(array.Length);
                    rigt = new GPUIntArray(array.Length);
                    a = new GPUIntArray(array.Length);
                    //debug = new GPUIntArray(array.Length);
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    int max_level = (__builtin_popcount(n) == 1 ? 31 : 32) - __builtin_clz(n);
                    left.Value(0); rigt.Value(n - 1);
                    //Print(value);
                    for (int level = 1; level <= max_level; level++)
                    {
                        if (false) // n log^2 n
                        {
                            //OpenTK.Graphics.OpenGL.GL.Finish();
                            //Print(Timing(() =>
                            //{
                                new Shader("SIFT.shaders.merge_sort.glsl").QueueForRunInSequence(n,
                                ("level", level),
                                value, left, rigt, output);
                            //    OpenTK.Graphics.OpenGL.GL.Finish();
                            //}));
                        }
                        else // n log n
                        {
                            int total_execute_cnt = 0;
                            for (int stride_level = max_level; stride_level >= 0; stride_level--)
                            {
                                // (1 << sl) - 1, 3(1 << sl) - 1, 5(1 << sl) - 1, 7(1 << sl) - 1, ...
                                int offset = (1 << stride_level) - 1;
                                //int stride = 1 << stride_level << 1;
                                // offset + ? * stride <= n - 1
                                // ? * stride <= n - 1 - offset
                                int execute_cnt = ((n - 1 - offset) >> stride_level >> 1) + 1;
                                total_execute_cnt += execute_cnt;

                                //OpenTK.Graphics.OpenGL.GL.Finish();
                                //Print(Timing(() =>
                                //{
                                    new Shader("SIFT.shaders.merge_sort_write_a.glsl").QueueForRunInSequence(execute_cnt,
                                    ("level", level),
                                    ("stride_level", stride_level),
                                    value, left, rigt, a);
                                //    OpenTK.Graphics.OpenGL.GL.Finish();
                                //}));
                                //Print(a);
                            }
                            //Print();
                            Assert(total_execute_cnt == n);
                            new Shader("SIFT.shaders.merge_sort_read_a_write_o.glsl").QueueForRunInSequence(n,
                                ("level", level),
                                value, left, rigt, a, output);
                        }
                        value.Swap(output);
                        //Print(value);
                        //Print(debug);
                    }
                }
            }
            class InPlaceParallelBitonicSorter
            {
                GPUIntArray value;
                GPUIntArray left, rigt;
                public InPlaceParallelBitonicSorter(GPUIntArray array)
                {
                    this.value = array;
                    left = new GPUIntArray(array.Length);
                    rigt = new GPUIntArray(array.Length);
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    int num_levels = (__builtin_popcount(n) == 1 ? 31 : 32) - __builtin_clz(n);
                    left.Value(0); rigt.Value(n - 1);
                    for (int start_level = 1; start_level <= num_levels; start_level++)
                    {
                        for (int level = start_level; level >= 1; level--)
                        {
                            new Shader("SIFT.shaders.bitonic_merge_inplace.glsl").QueueForRunInSequence(n,
                                ("start_level", start_level),
                                ("level", level),
                                ("reverse", level == start_level ? 1 : 0),
                                value, left, rigt);
                        }
                    }
                }
            }
            class ParallelBitonicSorter
            {
                GPUIntArray value;
                GPUIntArray left, rigt, output;
                public ParallelBitonicSorter(GPUIntArray array)
                {
                    this.value = array;
                    left = new GPUIntArray(array.Length);
                    rigt = new GPUIntArray(array.Length);
                    output = new GPUIntArray(array.Length);
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    int num_levels = (__builtin_popcount(n) == 1 ? 31 : 32) - __builtin_clz(n);
                    left.Value(0); rigt.Value(n - 1);
                    for(int start_level=1;start_level<=num_levels;start_level++)
                    {
                        for(int level=start_level;level>=1;level--)
                        {
                            new Shader("SIFT.shaders.bitonic_merge.glsl").QueueForRunInSequence(n,
                                ("start_level", start_level),
                                ("level", level),
                                ("reverse", level == start_level ? 1 : 0),
                                value, left, rigt, output);
                            value.Swap(output);
                            //Print(value);
                        }
                    }
                }
            }
            class ParallelAdaptiveBitonicSorter
            {
                GPUIntArray value, left, rigt, roots, spares;
                public ParallelAdaptiveBitonicSorter(GPUIntArray array)
                {
                    int n = array.Length;
                    Assert(__builtin_popcount(n) == 1);
                    this.value = array;
                    left = new GPUIntArray(n);
                    rigt = new GPUIntArray(n);
                    roots = new GPUIntArray(n);
                    spares = new GPUIntArray(n);
                }
                private void PrintTree(int root)
                {
                    if (root == -1) return;
                    PrintTree(left[root]);
                    PrintN(value[root], " ");
                    PrintTree(rigt[root]);
                }
                private void PrintTree(int root,int spare)
                {
                    PrintTree(root);
                    Print(spare);
                }
                private void PMerge(int n)
                {
                    Assert(__builtin_popcount(n) == 1);
                    for (int thread_id = 0; thread_id < n - 1; thread_id++)
                    {
                        int i = thread_id;
                        int cto = __builtin_ctz(~i);
                        int shift = (1 << (cto - 1));
                        (left[i], rigt[i]) = cto == 0 ? /*leaf*/ (-1, -1) : (i - shift, i + shift);
                        roots[i] = i;
                        spares[i] = i + (1 << cto);
                    }
                    for (int start_level = 1; start_level <= __builtin_ctz(n); start_level++)
                    {
                        //PrintTree(n / 2 - 1, n - 1);
                        for (int level = start_level; level >= 1; level--)
                        {
                            int id_max = n >> level;
                            for (int thread_id = 0; thread_id < id_max; thread_id++)
                            {
                                int i = (thread_id << level) + (1 << (level - 1)) - 1;
                                int origin_id = i; // origin location on binary tree (before any node swaps)
                                int root = roots[origin_id];
                                int spare = spares[origin_id]; // 10101001111 -> 101010
                                //Print("i",i,"root", root, "spare", spare);
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
                                    int shift = 1 << (level - 2);
                                    (roots[origin_id - shift], spares[origin_id - shift]) = (left[root], root);
                                    (roots[origin_id + shift], spares[origin_id + shift]) = (rigt[root], spare);
                                }
                            }
                        }
                    }
                    for (int level = __builtin_ctz(n); level >= 1; level--)
                    {
                        int id_max = n >> level;
                        for (int thread_id = 0; thread_id < id_max; thread_id++)
                        {
                            int i = (thread_id << level) + (1 << (level - 1)) - 1;
                            int origin_id = i; // origin location on binary tree (before any node swaps)
                            int root = roots[origin_id];
                            spares[i] = value[root];
                        }
                    }
                    spares[n - 1] = value[n - 1];
                    value.Swap(spares);
                }
                public void Sort()
                {
                    int n = value.Length;
                    if (n <= 1) return;
                    Assert(__builtin_popcount(n) == 1);
                    //Print(n, length,__builtin_clz(length),__builtin_ctz(length));
                    PMerge(n);
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
