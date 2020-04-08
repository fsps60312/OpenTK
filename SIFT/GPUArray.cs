using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    interface GPUArray
    {
        void Bind(int location);
    }
    partial class GPUArray<T> : GPUArray where T : struct
    {
        public string Name { get; set; } = "GPUArray<" + typeof(T).Name + ">";
        public int Length { get; private set; } = 0;
        private MyGL.Buffer<T> buffer = new MyGL.Buffer<T>();
        public GPUArray() { }
        public GPUArray(int length)
        {
            buffer.Data(length, BufferUsageHint.StreamDraw);
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
        public override string ToString()
        {
            return Name + ": {" + string.Join(", ", this.ToArray()) + "}";
        }
    }
}