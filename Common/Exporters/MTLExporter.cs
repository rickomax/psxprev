using System;
using System.Collections.Generic;
using System.IO;
using PSXPrev.Common.Renderer;

namespace PSXPrev.Common.Exporters
{
    public class MTLExporter : IDisposable
    {
        public const string UntexturedID = "none";

        private readonly StreamWriter _writer;
        private readonly string _selectedPath;
        private readonly string _baseName;
        private readonly string _baseTextureName;
        private readonly MaterialDictionary _materialIds;
        private readonly HashSet<Texture> _writtenMaterials = new HashSet<Texture>();

        public string FileName { get; }

        public MTLExporter(string selectedPath, string baseName, string baseTextureName, MaterialDictionary materialIds = null)
        {
            FileName = $"{baseName}.mtl";
            _writer = new StreamWriter($"{selectedPath}/{FileName}");
            _selectedPath = selectedPath;
            _baseName = baseName;
            _baseTextureName = baseTextureName;
            _materialIds = materialIds ?? new MaterialDictionary();

            // Add a material without a file for use with untextured models.
            WriteMaterial(null);
        }

        public string GetMaterialName(Texture texture)
        {
            return GetMaterialName(_materialIds.Get(texture));
        }

        public string GetMaterialName(int? materialId)
        {
            return $"mtl{materialId?.ToString() ?? MTLExporter.UntexturedID}";
        }

        public bool AddMaterial(Texture texture, out int materialId)
        {
            if (texture == null)
            {
                materialId = -1;
                return false;
            }
            // We may have already exported this material for another model,
            // but we may still need to write it to the mtl file for this model.
            var exported = !_materialIds.Add(texture, out materialId);
            if (_writtenMaterials.Add(texture))
            {
                WriteMaterial(materialId);
            }
            return !exported;
        }

        private void WriteMaterial(int? materialId)
        {
            _writer.WriteLine("newmtl {0}", GetMaterialName(materialId));
            _writer.WriteLine("Ka 0.00000 0.00000 0.00000"); // ambient color
            _writer.WriteLine("Kd 0.50000 0.50000 0.50000"); // diffuse color
            _writer.WriteLine("Ks 0.00000 0.00000 0.00000"); // specular color
            _writer.WriteLine("d 1.00000"); // "dissolved" (opaque)
            _writer.WriteLine("illum 0"); // illumination: 0-color on and ambient off
            if (materialId.HasValue)
            {
                _writer.WriteLine("map_Kd {0}.png", _baseTextureName + materialId); // diffuse texture map
                //todo: Output alpha for transparent pixels
                //_writer.WriteLine("map_d {0}a.png", _baseTextureName + materialId); // alpha texture map
            }
        }

        public void Dispose()
        {
            _writer.Close();
        }


        public class MaterialDictionary
        {
            private readonly bool[] _exportedPages = new bool[VRAM.PageCount]; // 32
            private readonly Dictionary<Texture, int> _exportedTextures = new Dictionary<Texture, int>();

            public bool Add(Texture texture, out int materialId)
            {
                if (texture == null)
                {
                    materialId = -1;
                    return false;
                }
                else if (texture.IsVRAMPage)
                {
                    materialId = texture.TexturePage;
                    if (_exportedPages[texture.TexturePage])
                    {
                        return false;
                    }
                    _exportedPages[texture.TexturePage] = true;
                }
                else
                {
                    if (_exportedTextures.TryGetValue(texture, out materialId))
                    {
                        return false;
                    }
                    materialId = _exportedPages.Length + _exportedTextures.Count;
                    _exportedTextures.Add(texture, materialId);
                }

                return true; // Material added
            }

            public bool Contains(Texture texture)
            {
                if (texture == null)
                {
                    return true;
                }
                else if (texture.IsVRAMPage)
                {
                    return _exportedPages[texture.TexturePage];
                }
                else
                {
                    return _exportedTextures.ContainsKey(texture);
                }
            }

            public int? Get(Texture texture)
            {
                if (texture == null)
                {
                    return null;
                }
                else if (texture.IsVRAMPage)
                {
                    if (!_exportedPages[texture.TexturePage])
                    {
                        throw new KeyNotFoundException();
                    }
                    return texture.TexturePage;
                }
                else
                {
                    return _exportedTextures[texture];
                }
            }

            public bool TryGetValue(Texture texture, out int? materialId)
            {
                if (texture == null)
                {
                    materialId = null;
                    return true;
                }
                else if (texture.IsVRAMPage)
                {
                    if (_exportedPages[texture.TexturePage])
                    {
                        materialId = texture.TexturePage;
                        return true;
                    }
                }
                else
                {
                    if (_exportedTextures.TryGetValue(texture, out var materialIdValue))
                    {
                        materialId = materialIdValue;
                        return true;
                    }
                }
                materialId = null;
                return false;
            }

            public void Clear()
            {
                for (var i = 0; i < _exportedPages.Length; i++)
                {
                    _exportedPages[i] = false;
                }
                _exportedTextures.Clear();
            }
        }
    }
}