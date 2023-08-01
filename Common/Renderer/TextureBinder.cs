﻿using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public class TextureBinder : IDisposable
    {
        private readonly uint[] _textures = new uint[VRAM.PageCount];

        public TextureBinder()
        {
            GL.GenTextures(VRAM.PageCount, _textures);
        }

        public void Dispose()
        {
            GL.DeleteTextures(VRAM.PageCount, _textures);
        }

        public uint GetTexture(int index)
        {
            return _textures[index];
        }

        public uint UpdateTexture(Bitmap bitmap, int index)
        {
            var texture = _textures[index];
            GL.BindTexture(TextureTarget.Texture2D, texture);

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            Unbind();
            return texture;
        }

        public void BindTexture(uint texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.Uniform1(Scene.AttributeIndexTexture, texture);
        }

        public void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
