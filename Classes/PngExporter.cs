using System.Drawing.Imaging;

namespace PSXPrev.Classes
{
    public class PngExporter
    {
        public void Export(Texture selectedTexture, int textureIndex, string selectedPath)
        {
            selectedTexture.Bitmap.Save(selectedPath + "/" + textureIndex + ".png", ImageFormat.Png);
        }

        public void Export(Texture[] selectedTextures, string selectedPath)
        {
            for (var i = 0; i < selectedTextures.Length; i++)
            {
                var selectedTexture = selectedTextures[i];
                Export(selectedTexture, i, selectedPath);  
            }
        }
    }
}
