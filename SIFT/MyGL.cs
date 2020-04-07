using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace SIFT
{
    public static class MyGL
    {
        const int GL_TRUE = 1;
        const int GL_FALSE = 0;
        public static void CheckError()
        {
            ErrorCode code = GL.GetError();
            if (code != ErrorCode.NoError)
            {
                throw new Exception($"MyGL.CheckError: {code}");
            }
        }
        public class Shader
        {
            public int id { get; private set; }
            public Shader(ShaderType type, string source = null)
            {
                id = GL.CreateShader(type); CheckError();
                if (source != null)
                {
                    Source(source);
                    Compile();
                }
            }
            public void Source(string source)
            {
                GL.ShaderSource(id, source); CheckError();
            }
            public void Compile()
            {
                GL.CompileShader(id); CheckError();
                string log=GL.GetShaderInfoLog(id);
                Console.WriteLine($"ShaderInfoLog: {log}");
                GL.GetShader(id, ShaderParameter.CompileStatus, out int compile_status);
                if (compile_status != GL_TRUE) throw new Exception();
            }
            public void Delete()
            {
                GL.DeleteShader(id); CheckError();
                id = 0;
            }
            ~Shader() { Delete(); }
        }
        public class Program
        {
            public int id { get; private set; }
            public Program(params Shader[] shaders_to_attach)
            {
                id = GL.CreateProgram(); if (id == 0) throw new Exception();
                foreach(var shader in shaders_to_attach) Attach(shader);
                Link();
                foreach (var shader in shaders_to_attach) Detach(shader);
            }
            public void Attach(Shader shader)
            {
                GL.AttachShader(id, shader.id);CheckError();
            }
            public void Detach(Shader shader)
            {
                GL.DetachShader(id, shader.id);CheckError();
            }
            public void Link()
            {
                GL.LinkProgram(id);CheckError();
                string log=GL.GetProgramInfoLog(id);CheckError();
                Console.WriteLine($"ProgramInfoLog: {log}");
                GL.GetProgram(id, GetProgramParameterName.LinkStatus, out int link_status);CheckError();
                if (link_status != GL_TRUE) throw new Exception();
            }
            public void Use()
            {
                GL.UseProgram(id); CheckError();
            }
            public int GetUniformLocation(string name)
            {
                int location = GL.GetUniformLocation(id, name);CheckError();
                return location;
            }
            #region Uniform
            public void Uniform(int location, uint x)
            {
                GL.Uniform1(location, x); CheckError();
            }
            public void Uniform(int location, int x)
            {
                GL.Uniform1(location, x); CheckError();
            }
            #endregion
            public void Delete()
            {
                GL.DeleteProgram(id);CheckError();
                id = 0;
            }
            ~Program() { Delete(); }
        }
        public class Texture
        {
            public int id { get; private set; }
            public Texture()
            {
                id = GL.GenTexture();CheckError();
            }
            public void TextureStorage2DVec4(int width, int height)
            {
                GL.TextureStorage2D(id, 1, SizedInternalFormat.Rgba16f, width, height);CheckError();
            }
            public void Bind()
            {
                GL.BindTexture(TextureTarget.TextureRectangle, id); CheckError();
            }
            public void BindImage(int unit,TextureAccess access,SizedInternalFormat format)
            {
                GL.BindImageTexture(unit, id, 0, false, 0, access, format);CheckError();
            }
            public void Delete()
            {
                GL.DeleteTexture(id);CheckError();
                id = 0;
            }
            ~Texture() { Delete(); }
        }
        public class Buffer
        {
            public int id { get; private set; }
            public Buffer()
            {
                GL.CreateBuffers(1, out int i);
                id = i; CheckError();
            }

            public void Data(int num_bytes, BufferUsageHint usage)
            {
                GL.NamedBufferData(id, num_bytes, IntPtr.Zero, usage);CheckError();
            }
            // byte, char, double, float, int, long, short
            public void Data<T>(T[] data, BufferUsageHint usage)where T:struct
            {
                GL.NamedBufferData<T>(id, Marshal.SizeOf(typeof(T)) * data.Length, data, usage);CheckError();
            }
            public void SubData<T>(int offset, T[] data) where T : struct
            {
                int u = Marshal.SizeOf(typeof(T));
                GL.NamedBufferSubData<T>(id, new IntPtr(u * offset), u * data.Length, data); CheckError();
            }
            public void SubData<T>(int offset, ref T data) where T : struct
            {
                int u = Marshal.SizeOf(typeof(T));
                GL.NamedBufferSubData<T>(id, new IntPtr(u * offset), u, ref data); CheckError();
            }
            public void Bind(BufferTarget target)
            {
                GL.BindBuffer(target, id); CheckError();
            }
            public T[] GetSubData<T>(int offset,int size)where T:struct
            {
                T[] ret = new T[size];
                int u = Marshal.SizeOf(typeof(T));
                GL.GetNamedBufferSubData<T>(id, new IntPtr(u * offset), u * size, ret);CheckError();
                return ret;
            }
            public void BindBase(BufferRangeTarget target,int index)
            {
                GL.BindBufferBase(target, index, id);CheckError();
            }
            public void Delete()
            {
                GL.DeleteBuffer(id);CheckError();
                id = 0;
            }
            ~Buffer() { Delete(); }
        }
    }
}
