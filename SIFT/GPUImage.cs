using OpenTK.Graphics.OpenGL;

namespace SIFT
{
    class GPUImage
    {
        private MyGL.Texture texture = new MyGL.Texture();
        public GPUImage(int width, int height)
        {
            texture.Bind();
            texture.TextureStorage2DVec4(width, height);
        }
        public void Bind(int location, TextureAccess access)
        {
            texture.BindImage(location, access, SizedInternalFormat.Rgba16f);
        }
    }
}