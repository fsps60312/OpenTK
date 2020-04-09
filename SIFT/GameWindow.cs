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
    class GameWindow : OpenTK.GameWindow
    {
        private MyGL.Buffer<float> window_vertex_buffer;
        private MyGL.Program texture_program;
        public GPUImage Canvas { get; private set; }
        Game game;
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
            window_vertex_buffer = new MyGL.Buffer<float>();
            window_vertex_buffer.Data(g_window_vertex_buffer_data, BufferUsageHint.StaticDraw);
        }
        private void CreateVertexArrayObject()
        {
            int id = MyGL.CheckError(()=> GL.GenVertexArray());
            MyGL.CheckError(() => GL.BindVertexArray(id));
        }
        public GameWindow()
            // set window resolution, title, and default behaviour
            : base(1280, 720, GraphicsMode.Default, "OpenTK Intro",
            GameWindowFlags.Default, DisplayDevice.Default,
            // ask for an OpenGL 3.0 forward compatible context
            3, 0, GraphicsContextFlags.ForwardCompatible)
        {
            Console.WriteLine("gl version: " + GL.GetString(StringName.Version));
            texture_program = new MyGL.Program(
                new MyGL.Shader(ShaderType.VertexShader, IO.ReadResource("SIFT.shaders.example_vertex_shader.glsl")),
                new MyGL.Shader(ShaderType.FragmentShader, IO.ReadResource("SIFT.shaders.example_fragment_shader.glsl")));
            InitFBO();
            CreateVertexArrayObject();
            Canvas = new GPUImage(this.Width, this.Height);
        }
        protected override void OnResize(EventArgs e)
        {
            MyGL.CheckError(() => GL.Viewport(0, 0, this.Width, this.Height));
        }
        protected override void OnLoad(EventArgs e)
        {
            // this is called when the window starts running
            game = new Game(this);
        }
        public delegate void DoubleHandler(double secs);
        public event DoubleHandler Update, Render;
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // this is called every frame, put game logic here
            Update?.Invoke(e.Time);
        }
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            MyGL.Shader.GC();
            MyGL.Program.GC();
            MyGL.Buffer.GC();
            MyGL.Texture.GC();
            MyGL.CheckError(() => GL.ClearColor(Color4.Purple));
            MyGL.CheckError(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            //RunShaders();
            Render?.Invoke(e.Time);

            texture_program.Use();
            int index = 0; // same as "location" in vertex shader
            MyGL.CheckError(() => GL.EnableVertexAttribArray(index));
            window_vertex_buffer.Bind(BufferTarget.ArrayBuffer);
            MyGL.CheckError(() => GL.VertexAttribPointer(index, 2, VertexAttribPointerType.Float, false, 0, IntPtr.Zero));
            /// Draw Rendered Texture
            MyGL.CheckError(() => GL.DrawArrays(PrimitiveType.Triangles, 0, 3 * 2)); // Starting from vertex 0; 3 vertices total -> 1 triangle

            MyGL.CheckError(() => GL.DisableVertexAttribArray(index));

            this.SwapBuffers();
        }
    }
}
