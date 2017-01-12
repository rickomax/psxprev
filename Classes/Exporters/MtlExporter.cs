using System;
using System.IO;

namespace PSXPrev.Classes.Exporters
{
    public class MtlExporter : IDisposable
    {
        private readonly string _fileTitle;
        public object FileTitle
        {
            get { return _fileTitle; }
        }

        private readonly StreamWriter _writer;
        private readonly int _modelIndex;
        private readonly bool[] _exportedPages = new bool[31];

        public MtlExporter(int modelIndex, string modelFileTitle, string selectedPath)
        {
            _modelIndex = modelIndex;
           _fileTitle = string.Format("{0}.mtl", modelFileTitle);
           var fileName = string.Format("{0}/{1}", selectedPath, _fileTitle);
            _writer = new StreamWriter(fileName);
        }

        public bool AddMaterial(Texture.Texture selectedTexture, int texturePage)
        {
            if (_exportedPages[texturePage])
            {
                return false;
            }
            _exportedPages[texturePage] = true;
            _writer.WriteLine("newmtl mtl{0}", texturePage);
            _writer.WriteLine("Ka 0.00000 0.00000 0.00000");
            _writer.WriteLine("Kd 0.50000 0.50000 0.50000");
            _writer.WriteLine("Ks 0.00000 0.00000 0.00000");
            _writer.WriteLine("d 1.00000");
            _writer.WriteLine("illum 0");
            _writer.WriteLine("map_Kd {0}_{1}.png", _modelIndex, texturePage);
            return true;
        }

        public void Dispose()
        {
            _writer.Close();
        }
    }
}