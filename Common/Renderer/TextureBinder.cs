using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public class TextureBinder : IDisposable
    {
        private readonly uint[] _textureIds = new uint[VRAM.PageCount];

        public TextureBinder()
        {
            GL.GenTextures(VRAM.PageCount, _textureIds);
        }

        public void Dispose()
        {
            GL.DeleteTextures(VRAM.PageCount, _textureIds);
        }

        public uint GetTextureID(int index)
        {
            return _textureIds[index];
        }

        public uint UpdateTexture(Bitmap bitmap, int index)
        {
            var texture = _textureIds[index];
            GL.BindTexture(TextureTarget.Texture2D, texture);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            Unbind();
            return texture;
        }

        public void BindTexture(uint textureId)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(Scene.AttributeIndexTexture, textureId);
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
