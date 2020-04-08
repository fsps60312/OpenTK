using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace SIFT
{
    public static class MyGL
    {
        const int GL_TRUE = 1;
        const int GL_FALSE = 0;
        public static void AssertError()
        {
            ErrorCode code = GL.GetError();
            if (code != ErrorCode.NoError)
            {
                throw new Exception($"MyGL.CheckError: {code}");
            }
        }
        public static void CheckError(Action gl_action)
        {
            AssertError();
            gl_action.Invoke();
            AssertError();
        }
        public static T CheckError<T>(Func<T> gl_action)
        {
            AssertError();
            T ret = gl_action.Invoke();
            AssertError();
            return ret;
        }
        public class Shader
        {
            public int id { get; private set; }
            public Shader(ShaderType type, string source = null)
            {
                CheckError(()=>id = GL.CreateShader(type));
                if (source != null)
                {
                    Source(source);
                    Compile();
                }
            }
            public void Source(string source)
            {
                CheckError(() => GL.ShaderSource(id, source));
            }
            public void Compile()
            {
                CheckError(() => GL.CompileShader(id));
                string log= CheckError(() => GL.GetShaderInfoLog(id));
                if(!string.IsNullOrEmpty(log))Console.WriteLine($"ShaderInfoLog: {log}");
                AssertError(); GL.GetShader(id, ShaderParameter.CompileStatus, out int compile_status);AssertError();
                if (compile_status != GL_TRUE) throw new Exception();
            }
            static BlockingCollection<int> garbage = new BlockingCollection<int>();
            public static void GC()
            {
                while(garbage.Count>0)CheckError(() => GL.DeleteShader(garbage.Take()));
            }
            ~Shader() { garbage.Add(id); }
        }
        public class Program
        {
            public int id { get; private set; }
            public Program(params Shader[] shaders_to_attach)
            {
                id = CheckError(() => GL.CreateProgram()); if (id == 0) throw new Exception();
                foreach(var shader in shaders_to_attach) Attach(shader);
                Link();
                foreach (var shader in shaders_to_attach) Detach(shader);
            }
            public void Attach(Shader shader)
            {
                CheckError(() => GL.AttachShader(id, shader.id));
            }
            public void Detach(Shader shader)
            {
                CheckError(() => GL.DetachShader(id, shader.id));
            }
            public void Link()
            {
                CheckError(() => GL.LinkProgram(id));
                string log= CheckError(() => GL.GetProgramInfoLog(id));
                if (!string.IsNullOrEmpty(log)) Console.WriteLine($"ProgramInfoLog: {log}");
                AssertError(); GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int link_status); AssertError();
                if (link_status != GL_TRUE) throw new Exception();
            }
            public void Use()
            {
                CheckError(() => GL.UseProgram(id));
            }
            public int GetUniformLocation(string name)
            {
                int location = CheckError(() => GL.GetUniformLocation(id, name));
                return location;
            }
            #region Uniform
            public void Uniform(int location, uint x)
            {
                MyGL.CheckError(() => GL.Uniform1(location, x));
            }
            public void Uniform(int location, int x)
            {
                MyGL.CheckError(() => GL.Uniform1(location, x));
            }
            #endregion
            static BlockingCollection<int> garbage = new BlockingCollection<int>();
            public static void GC()
            {
                while (garbage.Count > 0) CheckError(() => GL.DeleteProgram(garbage.Take()));
            }
            ~Program() { garbage.Add(id); }
        }
        public class Texture
        {
            public int id { get; private set; }
            public Texture()
            {
                id = CheckError(() => GL.GenTexture());
            }
            public void TextureStorage2DVec4(int width, int height)
            {
                CheckError(() => GL.TextureStorage2D(id, 1, SizedInternalFormat.Rgba16f, width, height));
            }
            public void Bind()
            {
                CheckError(() => GL.BindTexture(TextureTarget.TextureRectangle, id));
            }
            public void BindImage(int unit,TextureAccess access,SizedInternalFormat format)
            {
                CheckError(() => GL.BindImageTexture(unit, id, 0, false, 0, access, format));
            }
            static BlockingCollection<int> garbage = new BlockingCollection<int>();
            public static void GC()
            {
                while (garbage.Count > 0) CheckError(() => GL.DeleteTexture(garbage.Take()));
            }
            ~Texture() { garbage.Add(id); }
        }
        public abstract class Buffer
        {
            public int id { get; private set; }
            protected Buffer()
            {
                AssertError(); GL.CreateBuffers(1, out int i);AssertError();
                id = i;
            }
            private static BlockingCollection<int> garbage = new BlockingCollection<int>();
            public static void GC()
            {
                while (garbage.Count > 0) CheckError(() => GL.DeleteBuffer(garbage.Take()));
            }
            ~Buffer() { garbage.Add(id); }
        }
        public class Buffer<T>:Buffer where T:struct
        {
            public Buffer():base() { }
            public void Data(int length, BufferUsageHint usage)
            {
                CheckError(() => GL.NamedBufferData(id, Marshal.SizeOf(typeof(T)) * length, IntPtr.Zero, usage));
            }
            // byte, char, double, float, int, long, short
            public void Data(T[] data, BufferUsageHint usage)
            {
                CheckError(() => GL.NamedBufferData<T>(id, Marshal.SizeOf(typeof(T)) * data.Length, data, usage));
            }
            public void SubData(int offset, T[] data)
            {
                int u = Marshal.SizeOf(typeof(T));
                CheckError(() => GL.NamedBufferSubData<T>(id, new IntPtr(u * offset), u * data.Length, data));
            }
            public void SubData(int offset, ref T data)
            {
                int u = Marshal.SizeOf(typeof(T));
                AssertError(); GL.NamedBufferSubData<T>(id, new IntPtr(u * offset), u, ref data); AssertError();
            }
            public void Bind(BufferTarget target)
            {
                CheckError(() => GL.BindBuffer(target, id));
            }
            public T[] GetSubData(int offset,int size)
            {
                T[] ret = new T[size];
                int u = Marshal.SizeOf(typeof(T));
                CheckError(() => GL.GetNamedBufferSubData<T>(id, new IntPtr(u * offset), u * size, ret));
                return ret;
            }
            public void BindBase(BufferRangeTarget target,int index)
            {
                CheckError(() => GL.BindBufferBase(target, index, id));
            }
        }
    }
}
