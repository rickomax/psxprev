using System;
using System.IO;

namespace PSXPrev.Classes
{
    public class MtlExporter : IDisposable
    {
        public const string UntexturedID = "none";

        private readonly StreamWriter _writer;
        private readonly int _modelIndex;
        private readonly bool[] _exportedPages = new bool[VRAMPages.PageCount]; // 32

        public string FileName { get; }

        public MtlExporter(string selectedPath, int modelIndex)
        {
            FileName = $"mtl{modelIndex}.mtl";
            _writer = new StreamWriter($"{selectedPath}/{FileName}");
            _modelIndex = modelIndex;

            // Add a material without a file for use with untextured models.
            WriteMaterial(GetMaterialName(null), null);
        }

        public string GetMaterialName(int? texturePage)
        {
            var materialId = texturePage?.ToString() ?? MtlExporter.UntexturedID;
            return $"mtl{materialId}";
        }

        public bool AddMaterial(int texturePage)
        {
            if (_exportedPages[texturePage])
            {
                return false;
            }
            _exportedPages[texturePage] = true;

            WriteMaterial(GetMaterialName(texturePage), texturePage.ToString());
            return true;
        }

        private void WriteMaterial(string materialName, string fileName)
        {
            _writer.WriteLine("newmtl {0}", materialName);
            _writer.WriteLine("Ka 0.00000 0.00000 0.00000"); // ambient color
            _writer.WriteLine("Kd 0.50000 0.50000 0.50000"); // diffuse color
            _writer.WriteLine("Ks 0.00000 0.00000 0.00000"); // specular color
            _writer.WriteLine("d 1.00000"); // "dissolved" (opaque)
            _writer.WriteLine("illum 0"); // illumination: 0-color on and ambient off
            if (fileName != null)
            {
                _writer.WriteLine("map_Kd {0}.png", fileName); // diffuse texture map
                //todo: Output alpha for transparent pixels
                //_writer.WriteLine("map_d {0}.png", fileName2); // alpha texture map
            }
        }

        public void Dispose()
        {
            _writer.Close();
        }
    }
}