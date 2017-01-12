using System.Drawing.Imaging;

namespace PSXPrev.Classes.Exporters
{
    public class PngExporter
    {
        public void Export(Texture.Texture selectedTexture, int modelIndex, int textureIndex, string selectedPath)
        {
            selectedTexture.Bitmap.Save(selectedPath + "/" + modelIndex + "_" + textureIndex + ".png", ImageFormat.Png);
        }

        public void Export(Texture.Texture[] selectedTextures, string selectedPath)
        {
            for (var i = 0; i < selectedTextures.Length; i++)
            {
                var selectedTexture = selectedTextures[i];
                Export(selectedTexture, 0, i, selectedPath);  
            }
        }
    }
}
