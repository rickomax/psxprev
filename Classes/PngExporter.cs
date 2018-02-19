using System.Drawing;
using System.Drawing.Imaging;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class PngExporter
    {
        public void Export(Texture selectedTexture, int modelIndex, int textureIndex, string selectedPath)
        {
            selectedTexture.Bitmap.Save(selectedPath + "/" + modelIndex + "_" + textureIndex + ".png", ImageFormat.Png);
        }

        public void Export(Texture[] selectedTextures, string selectedPath)
        {
            for (var i = 0; i < selectedTextures.Length; i++)
            {
                var selectedTexture = selectedTextures[i];
                Export(selectedTexture, 0, i, selectedPath);  
            }
        }
    }
}
