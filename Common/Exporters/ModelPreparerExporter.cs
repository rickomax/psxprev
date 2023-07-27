using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using PSXPrev.Common.Renderer;

namespace PSXPrev.Common.Exporters
{
    // Class to split up and reconfigure models into a format that can be exported.
    // Currently this just separates models with tiled textures and assigns tiled textures to the models.
    public class ModelPreparerExporter : IDisposable
    {
        private readonly Dictionary<Tuple<uint, Vector4>, TiledTextureInfo> _groupedModels = new Dictionary<Tuple<uint, Vector4>, TiledTextureInfo>();
        private readonly Dictionary<RootEntity, List<ModelEntity>> _rootEntityModels = new Dictionary<RootEntity, List<ModelEntity>>();
        private readonly bool _tiledTextures;
        private readonly bool _singleTexture; // Should all texture pages/tiled textures be merged into one bitmap?
        private SingleTextureInfo _currentSingleInfo;

        public ModelPreparerExporter(bool tiledTextures = true, bool singleTexture = false)
        {
            _tiledTextures = tiledTextures;
            _singleTexture = singleTexture;
        }


        public void AddRootEntity(RootEntity rootEntity)
        {
            // todo: Maybe make this optional later. But for now we should
            // fix connections even if we haven't viewed the model yet.
            rootEntity.FixConnections();

            foreach (ModelEntity model in rootEntity.ChildEntities)
            {
                SeparateModel(rootEntity, model);
            }
        }

        public List<ModelEntity> GetModels(RootEntity rootEntity)
        {
            return _rootEntityModels[rootEntity];
        }

        // Call this after AddRootEntity has been called for all root entities that plan to be exported.
        public void PrepareAll()
        {
            foreach (var tiledInfo in _groupedModels.Values)
            {
                if (tiledInfo.IsTiled)
                {
                    PrepareTiledTexture(tiledInfo); // Only need to prepare tiled textures
                }
            }
        }

        // Call this when preparing to export the current set of root entities.
        // NOTE: This assumes that PrepareCurrent will NEVER be called with the same RootEntity twice!
        public void PrepareCurrent(params RootEntity[] rootEntities)
        {
            if (_currentSingleInfo != null)
            {
                // Cleanup previous singleInfo
                _currentSingleInfo.Dispose();
                _currentSingleInfo = null;
            }
            if (_singleTexture)
            {
                // Add models and textures to singleInfo
                _currentSingleInfo = new SingleTextureInfo();
                var addedTextures = new HashSet<Texture>();
                foreach (var rootEntity in rootEntities)
                {
                    var models = GetModels(rootEntity);
                    foreach (var model in models)
                    {
                        if (model.Texture != null && model.IsTextured && addedTextures.Add(model.Texture))
                        {
                            _currentSingleInfo.OriginalTextures.Add(model.Texture);
                        }
                    }
                    _currentSingleInfo.Models.AddRange(models);
                }

                PrepareSingleTexture(_currentSingleInfo);
            }
        }


        private void PrepareTiledTexture(TiledTextureInfo tiledInfo)
        {
            // Find how many times the texture needs to repeat in the X and Y directions.
            var maxUv = Vector2.Zero;
            foreach (var model in tiledInfo.Models)
            {
                foreach (var triangle in model.Triangles)
                {
                    var baseUv = triangle.TiledBaseUv;
                    if (baseUv != null)
                    {
                        for (var j = 0; j < 3; j++)
                        {
                            maxUv = Vector2.ComponentMax(maxUv, baseUv[j]);
                        }
                    }
                }
            }

            // Create a new tiled texture that repeats the needed amount of times.
            tiledInfo.SetupTiledTexture(maxUv);

            // Assign new tiled texture to models.
            foreach (var model in tiledInfo.Models)
            {
                model.Texture = tiledInfo.TiledTexture;

                // Convert triangle UVs to new tiled texture UVs (we're no longer 256x256).
                foreach (var triangle in model.Triangles)
                {
                    var baseUv = triangle.TiledBaseUv;
                    if (baseUv != null)
                    {
                        // Create a new array because we don't own triangle.Uv's array.
                        var uvs = new Vector2[3];
                        for (var j = 0; j < 3; j++)
                        {
                            uvs[j] = tiledInfo.Convert(baseUv[j]);
                        }
                        triangle.Uv = uvs;
                        triangle.TiledUv = null; // We're not tiled anymore.
                    }
                }
            }
        }

        private void PrepareSingleTexture(SingleTextureInfo singleInfo)
        {
            // Compute packing and create a new single texture that contains all other textures.
            singleInfo.SetupSingleTexture(true, true, true);

            // Assign new single texture to models.
            foreach (var model in singleInfo.Models)
            {
                if (model.Texture != null && model.IsTextured)
                {
                    var uvConverter = singleInfo.GetUVConverter(model.Texture);
                    model.Texture = singleInfo.SingleTexture;

                    // Convert triangle UVs to new single texture UVs (we're no longer 256x256).
                    foreach (var triangle in model.Triangles)
                    {
                        var origUv = triangle.Uv;
                        // Create a new array because we don't own triangle.Uv's array.
                        var uvs = new Vector2[3];
                        for (var j = 0; j < 3; j++)
                        {
                            uvs[j] = uvConverter.Convert(origUv[j]);
                        }
                        triangle.Uv = uvs;
                        triangle.TiledUv = null; // We're not tiled anymore.
                    }
                }
            }
        }

        private void SeparateModel(RootEntity rootEntity, ModelEntity model)
        {
            if (!_singleTexture && (!_tiledTextures || model.Texture == null || !model.IsTextured || !model.NeedsTiled))
            {
                // No changes to the model.
                AddModel(rootEntity, model, Vector4.Zero);
            }
            else if (_singleTexture && !_tiledTextures)
            {
                // Create copies for every model with copies of each triangle.
                var triangles = model.Triangles;
                var newTriangles = new Triangle[triangles.Length];
                for (var i = 0; i < triangles.Length; i++)
                {
                    newTriangles[i] = new Triangle(triangles[i]);
                }
                var newModel = new ModelEntity(model, newTriangles);
                AddModel(rootEntity, newModel, Vector4.Zero);
            }
            else
            {
                // Separate models by tiled area, and force-create copies if we're creating a single texture.
                var groupedTriangles = new Dictionary<Vector4, List<Triangle>>();

                foreach (var triangle in model.Triangles)
                {
                    var needsTiled = _tiledTextures && triangle.NeedsTiled;
                    var needsCopy = _singleTexture || needsTiled;
                    var tiledArea = needsTiled ? triangle.TiledArea.Value : Vector4.Zero;
                    if (!groupedTriangles.TryGetValue(tiledArea, out var triangles))
                    {
                        triangles = new List<Triangle>();
                        groupedTriangles.Add(tiledArea, triangles);
                    }
                    // We only need to create a copy for tiled triangles, since we'll be modifying those.
                    triangles.Add(needsCopy ? new Triangle(triangle) : triangle);
                }

                foreach (var kvp in groupedTriangles)
                {
                    var tiledArea = kvp.Key;
                    var triangles = kvp.Value;
                    var newModel = new ModelEntity(model, triangles.ToArray());
                    AddModel(rootEntity, newModel, tiledArea);
                }
            }
        }

        private void AddModel(RootEntity rootEntity, ModelEntity model, Vector4 tiledArea)
        {
            var tuple = new Tuple<uint, Vector4>(model.TexturePage, tiledArea);
            if (!_groupedModels.TryGetValue(tuple, out var tiledInfo))
            {
                tiledInfo = new TiledTextureInfo(model.Texture, tiledArea);
                _groupedModels.Add(tuple, tiledInfo);
            }
            tiledInfo.Models.Add(model);

            if (!_rootEntityModels.TryGetValue(rootEntity, out var childModels))
            {
                childModels = new List<ModelEntity>();
                _rootEntityModels.Add(rootEntity, childModels);
            }
            childModels.Add(model);
        }

        public void Dispose()
        {
            if (_currentSingleInfo != null)
            {
                _currentSingleInfo.Dispose();
                _currentSingleInfo = null;
            }
            foreach (var tiledInfo in _groupedModels.Values)
            {
                tiledInfo.Dispose();
            }
            _groupedModels.Clear();
            _rootEntityModels.Clear();
        }


        private class SingleTextureInfo : IDisposable
        {
            // Use this as an alignment since texture tiling is always in units of 8.
            private const int PACKED_ALIGN = 8;

            public class PackedTextureInfo
            {
                //public Texture Texture;
                public float OffsetX;
                public float OffsetY;
                public float ScalarX;
                public float ScalarY;

                public Vector2 Convert(Vector2 origUv)
                {
                    return new Vector2(OffsetX + origUv.X * ScalarX, OffsetY + origUv.Y * ScalarY);
                }
            }

            public List<Texture> OriginalTextures { get; } = new List<Texture>();
            public List<ModelEntity> Models { get; } = new List<ModelEntity>();

            public Texture SingleTexture { get; private set; }

            private readonly List<List<Texture>> _packedTextures = new List<List<Texture>>();
            private readonly Dictionary<Texture, PackedTextureInfo> _packedTextureInfos = new Dictionary<Texture, PackedTextureInfo>();

            private int _countX = 1;
            private int _countY = 1;


            public PackedTextureInfo GetUVConverter(Texture oldTexture)
            {
                return _packedTextureInfos[oldTexture];
            }

            public void SetupSingleTexture(bool powerOfTwo, bool sort, bool pack)
            {
                // Sort textures.
                if (sort || pack) // Sorting is required when packing using greedy method
                {
                    // Order by:
                    // 1. Texture pages:
                    //     i. Page:   lowest to highest
                    // 2. Loose textures:
                    //     i. Width:  highest to lowest
                    //    ii. Height: highest to lowest
                    //   iii. Page:   lowest to highest
                    OriginalTextures.Sort((a, b) => {
                        if (!a.IsVRAMPage || !b.IsVRAMPage)
                        {
                            if (a.IsVRAMPage != b.IsVRAMPage)
                            {
                                return -a.IsVRAMPage.CompareTo(b.IsVRAMPage);
                            }
                            else if (a.Width != b.Width)
                            {
                                return -a.Width.CompareTo(b.Width);
                            }
                            else if (a.Height != b.Height)
                            {
                                return -a.Height.CompareTo(b.Height);
                            }
                        }
                        return a.TexturePage.CompareTo(b.TexturePage);
                    });
                }

                // Pack multiple loose textures into single cells to reduce required space.
                if (pack)
                {
                    PackTextures();
                }
                else
                {
                    // Add each texture to its own cell.
                    foreach (var texture in OriginalTextures)
                    {
                        _packedTextures.Add(new List<Texture> { texture });
                    }
                }

                // Find a column/row count with the smallest max dimension (closest to a square).
                // todo: This calculation can be simplified/optimized by a lot, but it works for now...
                var total = _packedTextures.Count;
                _countX = total;
                _countY = 1;
                var w = 1;
                while (w <= total)
                {
                    // Round height up to account for partially-used rows.
                    var h = (total + w - 1) / w;

                    if (powerOfTwo)
                    {
                        // Find a power of two for the height.
                        var h2 = 1;
                        while (h2 < h)
                        {
                            h2 *= 2;
                        }
                        h = h2;
                    }

                    // Use <= to prefer larger widths over larger heights.
                    if (Math.Max(w, h) <= Math.Max(_countX, _countY))
                    {
                        _countX = w;
                        _countY = h;
                    }

                    // Loop increment
                    if (powerOfTwo)
                    {
                        w *= 2;
                    }
                    else
                    {
                        w++;
                    }
                }

                // Create packed texture info to help convert texture UVs later.
                var cellIndex = 0;
                foreach (var cellTextures in _packedTextures)
                {
                    var column = cellIndex % _countX;
                    var row    = cellIndex / _countX;
                    foreach (var texture in cellTextures)
                    {
                        // We can't just use this, since we still need to support disabling the UV alignment fix.
                        //var uvScalar = (float)VRAM.PageSize;
                        //var width  = texture.RenderWidth;
                        //var height = texture.RenderHeight;
                        var uvScalar = GeomMath.UVScalar;
                        var width  = !texture.IsVRAMPage ? texture.Width  : (int)uvScalar; //VRAM.PageSize;
                        var height = !texture.IsVRAMPage ? texture.Height : (int)uvScalar; //VRAM.PageSize;
                        var packedInfo = new PackedTextureInfo
                        {
                            //Texture = texture,
                            // Keep float casts in-case we replace uvScalar with VRAM.PageSize.
                            OffsetX = ((float)texture.X / uvScalar + column) / _countX,
                            OffsetY = ((float)texture.Y / uvScalar + row)    / _countY,
                            ScalarX = (width  > 0 ? ((float)width  / (uvScalar * _countX)) : (1f / _countX)),
                            ScalarY = (height > 0 ? ((float)height / (uvScalar * _countY)) : (1f / _countY)),
                        };
                        _packedTextureInfos.Add(texture, packedInfo);
                    }
                    cellIndex++;
                }

                if (SingleTexture == null)
                {
                    var bitmap = VRAM.ConvertSingleTexture(_packedTextures, _countX, _countY, false);
                    try
                    {
                        SingleTexture = new Texture(bitmap, 0, 0, 32, 0);
                        SingleTexture.TextureName = $"Single[{_countX}x{_countY}]";
                        //SingleTexture.SemiTransparentMap = VRAM.ConvertSingleTexture(_packedTextures, _countX, _countY, true);
                    }
                    catch
                    {
                        bitmap.Dispose();
                        throw;
                    }
                }
            }

            private void PackTextures()
            {
                // Add VRAM texture pages as their own cells, no other textures can be placed in them.
                foreach (var texture in OriginalTextures)
                {
                    if (texture.IsVRAMPage)
                    {
                        _packedTextures.Add(new List<Texture> { texture });
                    }
                }

                // Greedy rectangle packing: <https://stackoverflow.com/a/1213571/7517185>
                var packedRects = new List<List<Rectangle>>();

                var vramCellCount = _packedTextures.Count; // Now many indices to skip when adding to _packedTextures.
                foreach (var texture in OriginalTextures)
                {
                    if (texture.IsVRAMPage)
                    {
                        continue;
                    }

                    var rect = new Rectangle(0, 0, texture.Width, texture.Height);

                    var cellFound = false;
                    var cellIndex = 0;
                    foreach (var cellRects in packedRects)
                    {
                        for (var x = 0; x + rect.Width <= VRAM.PageSize && !cellFound; x += PACKED_ALIGN)
                        {
                            rect.X = x;
                            for (var y = 0; y + rect.Height <= VRAM.PageSize && !cellFound; y += PACKED_ALIGN)
                            {
                                rect.Y = y;

                                cellFound = true; // Assume a space is found in the cell unless we intersect.
                                foreach (var otherRect in cellRects)
                                {
                                    if (rect.IntersectsWith(otherRect))
                                    {
                                        cellFound = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (cellFound)
                        {
                            // Add to existing cell
                            texture.X = rect.X;
                            texture.Y = rect.Y;
                            cellRects.Add(rect);
                            _packedTextures[cellIndex + vramCellCount].Add(texture);
                            break;
                        }
                        cellIndex++;
                    }
                    if (!cellFound)
                    {
                        // Start a new cell
                        rect.X = 0; // Reset rect position used during intersection checks
                        rect.Y = 0;
                        packedRects.Add(new List<Rectangle> { rect });
                        _packedTextures.Add(new List<Texture> { texture });
                    }
                }
            }

            public void Dispose()
            {
                if (SingleTexture != null)
                {
                    SingleTexture.Dispose();
                    SingleTexture = null;
                }
                // Clear texture X,Y coordinates assigned during PackTextures.
                foreach (var texture in OriginalTextures)
                {
                    texture.X = 0;
                    texture.Y = 0;
                }
            }
        }


        private class TiledTextureInfo : IDisposable
        {
            public Texture OriginalTexture { get; }
            public float X { get; } // Position added to BaseUv after wrapping.
            public float Y { get; }
            public float Width { get; } // Denominator of modulus with BaseUv for wrapping.
            public float Height { get; }

            public List<ModelEntity> Models { get; } = new List<ModelEntity>();

            public Texture TiledTexture { get; private set; }

            private int _repeatX = 1;
            private int _repeatY = 1;
            private float _scalarX = 1f; // Cached scalars to convert base UVs to tiled texture UVs.
            private float _scalarY = 1f;

            public int TexturePage => OriginalTexture.TexturePage;

            public bool IsTiled => Width != 0f && Height != 0f;

            public TiledTextureInfo(Texture originalTexture, Vector4 tiledArea)
            {
                OriginalTexture = originalTexture;
                X = tiledArea.X;
                Y = tiledArea.Y;
                Width  = tiledArea.Z;
                Height = tiledArea.W;
            }


            public Vector2 Convert(Vector2 baseUv)
            {
                return new Vector2(baseUv.X * _scalarX, baseUv.Y * _scalarY);
            }

            public void SetupTiledTexture(Vector2 maxUv)
            {
                // Make sure repeat is at least one or higher.
                _repeatX = (int)Math.Max(1, Width  != 0f ? Math.Ceiling(maxUv.X / Width)  : 1f);
                _repeatY = (int)Math.Max(1, Height != 0f ? Math.Ceiling(maxUv.Y / Height) : 1f);

                _scalarX = (Width  != 0f ? 1f / (Width  * _repeatX) : 1f);
                _scalarY = (Height != 0f ? 1f / (Height * _repeatY) : 1f);

                if (TiledTexture == null)
                {
                    // We can't just use this, since we still need to support disabling the UV alignment fix.
                    //var uvScalar = (float)VRAM.PageSize;
                    var uvScalar = GeomMath.UVScalar;
                    var srcX = (int)(X * uvScalar);
                    var srcY = (int)(Y * uvScalar);
                    var srcWidth  = Width  != 0f ? (int)(Width  * uvScalar) : VRAM.PageSize;
                    var srcHeight = Height != 0f ? (int)(Height * uvScalar) : VRAM.PageSize;
                    var srcRect = new Rectangle(srcX, srcY, srcWidth, srcHeight);

                    var bitmap = VRAM.ConvertTiledTexture(OriginalTexture, srcRect, _repeatX, _repeatY, false);
                    try
                    {
                        TiledTexture = new Texture(bitmap, 0, 0, 32, TexturePage);
                        //TiledTexture.TextureName = $"Tiled[{srcX},{srcY} {srcWidth}x{srcHeight}]";
                        TiledTexture.TextureName = $"Tiled[{_repeatX}x{_repeatY}]";
                        //TiledTexture.SemiTransparentMap = VRAM.ConvertTiledTexture(OriginalTexture, srcRect, _repeatX, _repeatY, true);
                    }
                    catch
                    {
                        bitmap.Dispose();
                        throw;
                    }
                }
            }

            public void Dispose()
            {
                if (TiledTexture != null)
                {
                    TiledTexture.Dispose();
                    TiledTexture = null;
                }
            }
        }
    }
}
