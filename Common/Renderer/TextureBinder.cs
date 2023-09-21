using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public class TextureBinder : IDisposable
    {
        private readonly int[] _textureIds = new int[VRAM.PageCount];

        public TextureBinder()
        {
            GL.GenTextures(VRAM.PageCount, _textureIds);
        }

        public void Dispose()
        {
            GL.DeleteTextures(VRAM.PageCount, _textureIds);
        }

        public int GetTextureID(int index)
        {
            return _textureIds[index];
        }

        public void UpdateTexture(Bitmap bitmap, int index)
        {
            var textureId = _textureIds[index];
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            var bmpData = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            try
            {
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            }
            finally
            {
                bitmap.UnlockBits(bmpData);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        }

        public void BindTexture(int textureId)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            // Sampler uniform NEEDS to be set with uint, not int!
            GL.Uniform1(Shader.AttributeIndex_Texture, (uint)textureId);
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
