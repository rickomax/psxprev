using System.Drawing.Imaging;

namespace PSXPrev.Classes
{
    public class PngExporter
    {
        public void Export(Texture selectedTexture, int textureIndex, string selectedPath)
        {
            var filePath = $"{selectedPath}/{textureIndex}.png";
            if (selectedTexture.IsVRAMPage)
            {
                // Remove the semi-transparency section from the exported bitmap.
                using (var bitmap = VRAMPages.GetTextureOnly(selectedTexture))
                {
                    bitmap.Save(filePath, ImageFormat.Png);
                }
            }
            else
            {
                selectedTexture.Bitmap.Save(filePath, ImageFormat.Png);
            }
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
