using System;
using System.IO;

namespace PSXPrev.Classes
{
    public class MtlExporter : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly int _modelIndex;
        private readonly bool[] _exportedPages = new bool[32];

        public MtlExporter(string selectedPath, int modelIndex)
        {
            _writer = new StreamWriter($"{selectedPath}/mtl{modelIndex}.mtl");
        }

        public bool AddMaterial(int texturePage)
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
            _writer.WriteLine("map_Kd {0}.png", texturePage);
            return true;
        }

        public void Dispose()
        {
            _writer.Close();
        }
    }
}