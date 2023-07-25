﻿using System.Drawing;
using System.Drawing.Imaging;

namespace PSXPrev.Classes
{
    public class PngExporter
    {
        public void Export(Texture texture, int textureId, string selectedPath)
        {
            Export(texture, textureId.ToString(), selectedPath);
        }

        public void Export(Texture texture, string name, string selectedPath)
        {
            if (texture.IsVRAMPage)
            {
                // Remove the semi-transparency section from the exported bitmap.
                using (var bitmap = VRAMPages.ConvertTexture(texture, false))
                {
                    ExportBitmap(bitmap, name, selectedPath);
                }
            }
            else
            {
                ExportBitmap(texture.Bitmap, name, selectedPath);
            }
        }

        public void ExportEmpty(System.Drawing.Color color, int textureId, string selectedPath, int width = 1, int height = 1)
        {
            ExportEmpty(color, textureId.ToString(), selectedPath, width, height);
        }

        public void ExportEmpty(System.Drawing.Color color, string name, string selectedPath, int width = 1, int height = 1)
        {
            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.Clear(color);
                }
                ExportBitmap(bitmap, name, selectedPath);
            }
        }

        public void Export(Texture[] textures, string selectedPath)
        {
            for (var i = 0; i < textures.Length; i++)
            {
                Export(textures[i], i, selectedPath);
            }
        }

        private void ExportBitmap(Bitmap bitmap, string name, string selectedPath)
        {
            var filePath = $"{selectedPath}/{name}.png";
            bitmap.Save(filePath, ImageFormat.Png);
        }
    }
}
