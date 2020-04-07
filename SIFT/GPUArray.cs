using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    interface GPUArray
    {
        void Bind(int location);
    }
    partial class GPUArray<T> : GPUArray where T : struct
    {
        public int Length { get; private set; } = 0;
        private MyGL.Buffer buffer = new MyGL.Buffer();
        public GPUArray() { }
        public GPUArray(int length)
        {
            buffer.Data(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)) * length, BufferUsageHint.StreamDraw);
            Length = length;
        }
        public GPUArray(T[] data) { Data(data); }
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
            return buffer.GetSubData<T>(index, count);
        }
        public T[] ToArray()
        {
            return buffer.GetSubData<T>(0, Length);
        }
        public void Bind(int location)
        {
            buffer.BindBase(BufferRangeTarget.ShaderStorageBuffer, location);
        }
        public T this[int key]
        {
            get { return buffer.GetSubData<T>(key, 1)[0]; }
            set { buffer.SubData(key, ref value); }
        }
    }
}