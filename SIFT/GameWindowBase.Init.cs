using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    abstract partial class GameWindowBase : OpenTK.GameWindow
    {
        private MyGL.Buffer window_vertex_buffer;
        private MyGL.Program texture_program;
        protected GPUImage Canvas { get; private set; }
        private void InitFBO()
        {
            float[] g_window_vertex_buffer_data = new[]{
                -1.0f, -1.0f,
                 1.0f, -1.0f,
                -1.0f,  1.0f,
                -1.0f,  1.0f,
                 1.0f, -1.0f,
                 1.0f,  1.0f,
            };
            window_vertex_buffer = new MyGL.Buffer();
            window_vertex_buffer.Data(g_window_vertex_buffer_data, BufferUsageHint.StaticDraw);
        }
        private void CreateVertexArrayObject()
        {
            int id = GL.GenVertexArray();
            GL.BindVertexArray(id);
        }
        public GameWindowBase()
            // set window resolution, title, and default behaviour
            : base(1280, 720, GraphicsMode.Default, "OpenTK Intro",
            GameWindowFlags.Default, DisplayDevice.Default,
            // ask for an OpenGL 3.0 forward compatible context
            4, 6, GraphicsContextFlags.ForwardCompatible)
        {
            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }
        protected override void OnLoad(EventArgs e)
        {
            // this is called when the window starts running
            texture_program = new MyGL.Program(
                new MyGL.Shader(ShaderType.VertexShader, IO.ReadResource("SIFT.shaders.example_vertex_shader.glsl")),
                new MyGL.Shader(ShaderType.FragmentShader, IO.ReadResource("SIFT.shaders.example_fragment_shader.glsl")));
            Console.WriteLine("success");
            InitFBO();
            CreateVertexArrayObject();
            Canvas = new GPUImage(this.Width, this.Height);
            Init();
        }
        protected abstract void Update(double secs);
        protected abstract void Render(double secs);
        protected abstract void Init();
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            Update(e.Time);
        }
        //private void RunShaders()
        //{
        //    render_texture.BindImage(0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba16f);
        //    compute_program.Use();
        //    GL.DispatchCompute(this.Width / 4, this.Height / 4, 1);
        //}
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.ClearColor(Color4.Purple);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //RunShaders();
            Render(e.Time);

            texture_program.Use();
            int index = 0; // same as "location" in vertex shader
            GL.EnableVertexAttribArray(index); MyGL.CheckError();
            window_vertex_buffer.Bind(BufferTarget.ArrayBuffer);
            MyGL.CheckError();
            GL.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            MyGL.CheckError();
            /// Draw Rendered Texture
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3 * 2); MyGL.CheckError(); // Starting from vertex 0; 3 vertices total -> 1 triangle

            GL.DisableVertexAttribArray(index);

            this.SwapBuffers();
        }
    }
}
