﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    abstract class WrapperVirtualWindow
    {
        class ActualWindow : OpenTK.GameWindow
        {
            public ActualWindow() { }
        }
        ActualWindow actual_window = new ActualWindow();
        protected WrapperVirtualWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)
        {
            actual_window.Resize += (o, e) => OnResize(e);
            actual_window.Load += (o, e) => OnLoad(e);
            actual_window.UpdateFrame += (o, e) => OnUpdateFrame(e);
            actual_window.RenderFrame += (o, e) => OnRenderFrame(e);
        }

        protected abstract void OnResize(EventArgs e);
        protected abstract void OnLoad(EventArgs e);
        protected abstract void OnUpdateFrame(FrameEventArgs e);
        protected abstract void OnRenderFrame(FrameEventArgs e);
        protected void SwapBuffers() { actual_window.SwapBuffers(); }
        public void Run(double updates_per_second, double frames_per_second) { actual_window.Run(updates_per_second, frames_per_second); }
        public int Width
        {
            get { return actual_window.Width; }
            set { actual_window.Width = value; }
        }
        public int Height
        {
            get { return actual_window.Height; }
            set { actual_window.Height = value; }
        }
    }
    abstract partial class GameWindowBase:WrapperVirtualWindow // : OpenTK.GameWindow
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
            int id = MyGL.CheckError(()=> GL.GenVertexArray());
            MyGL.CheckError(() => GL.BindVertexArray(id));
        }
        public GameWindowBase()
            // set window resolution, title, and default behaviour
            : base(1280, 720, GraphicsMode.Default, "OpenTK Intro",
            GameWindowFlags.Default, DisplayDevice.Default,
            // ask for an OpenGL 3.0 forward compatible context
            4, 6, GraphicsContextFlags.ForwardCompatible)
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
        }
        protected abstract void Update(double secs);
        protected abstract void Render(double secs);
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
            MyGL.Shader.GC();
            MyGL.Program.GC();
            MyGL.Buffer.GC();
            MyGL.Texture.GC();
            MyGL.CheckError(() => GL.ClearColor(Color4.Purple));
            MyGL.CheckError(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

            //RunShaders();
            Render(e.Time);

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
