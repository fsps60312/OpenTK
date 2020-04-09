using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    abstract partial class GameBase
    {
        protected interface GPUArray
        {
            void Bind(int location);
        }
        protected abstract class GPUArray<T> : GPUArray where T : struct
        {
            public int Length { get; private set; } = 0;
            private MyGL.Buffer<T> buffer = new MyGL.Buffer<T>();
            public GPUArray() { }
            public GPUArray(int length)
            {
                buffer.Data(length, BufferUsageHint.StreamDraw);
                Length = length;
            }
            public GPUArray(T[] data) { Data(data); }
            public GPUArray(GPUArray<T> array) : this(array.Length)
            {
                Param.Array(array, this); new Shader($"SIFT.shaders.copy.glsl").QueueForRun(Length);
            }
            public void Data(T[] data)
            {
                buffer.Data(data, BufferUsageHint.StreamDraw);
                Length = data.Length;
            }
            public void SubData(int offset, T[] data)
            {
                buffer.SubData(offset, data);
            }
            public T[] GetRange(int index, int count)
            {
                return buffer.GetSubData(index, count);
            }
            public T[] ToArray()
            {
                return buffer.GetSubData(0, Length);
            }
            public void Bind(int location)
            {
                buffer.BindBase(BufferRangeTarget.ShaderStorageBuffer, location);
            }
            public T this[int key]
            {
                get { return buffer.GetSubData(key, 1)[0]; }
                set { buffer.SubData(key, ref value); }
            }
            public void Swap(GPUArray<T> array)
            {
                (this.buffer, array.buffer) = (array.buffer, this.buffer);
                (this.Length, array.Length) = (array.Length, this.Length);
            }
            public override string ToString()
            {
                return "{" + string.Join(", ", this.ToArray()) + "}";
            }
        }
    }
}