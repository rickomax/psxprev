using System;
using System.Globalization;
using System.IO;


namespace PSXPrev
{
    public class MtlExporter : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly int _modelIndex;
        private readonly bool[] _exportedPages = new bool[31];

        public MtlExporter(int modelIndex, string selectedPath)
        {
            _modelIndex = modelIndex;
            _writer = new StreamWriter(selectedPath + "/mtl" + modelIndex + ".mtl");
        }

        public bool AddMaterial(Texture selectedTexture, int texturePage)
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