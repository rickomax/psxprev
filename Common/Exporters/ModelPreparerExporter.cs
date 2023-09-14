using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using PSXPrev.Common.Renderer;

namespace PSXPrev.Common.Exporters
{
    // Class to split up and reconfigure models into a format that can be exported.
    // Currently this just separates models with tiled textures and assigns tiled textures to the models.
    public class ModelPreparerExporter : IDisposable
    {
        private class RootEntityBuilder
        {
            public readonly Dictionary<RootEntity, Tuple<RootEntity, List<ModelEntity>>> RootEntityModels = new Dictionary<RootEntity, Tuple<RootEntity, List<ModelEntity>>>();
            public readonly List<ModelEntity> AllModels = new List<ModelEntity>();
        }

        private readonly Dictionary<Tuple<uint, Vector4>, TiledTextureInfo> _groupedModels = new Dictionary<Tuple<uint, Vector4>, TiledTextureInfo>();
        private readonly Dictionary<Tuple<int, long>, RootEntityBuilder> _rootEntityModels = new Dictionary<Tuple<int, long>, RootEntityBuilder>();
        private readonly List<Tuple<int, long>> _groups = new List<Tuple<int, long>>();
        private readonly Texture[] _copiedVRAMPages = new Texture[VRAM.PageCount];
        private readonly bool _bakeConnections;
        private readonly ExportModelOptions _options;
        private SingleTextureInfo _singleInfo;

        private bool CanPrepareAll => _options.ModelGrouping == ExportModelGrouping.GroupAllModels || !_options.ExportTextures || _options.ShareTextures;

        public ModelPreparerExporter(ExportModelOptions options, bool bakeConnections = true)
        {
            _options = options;
            _bakeConnections = bakeConnections;
        }


        // I'm not happy with how this is setup right now, but it's a quick fix. -trigger
        public RootEntity[] GetPreparedRootEntity(/*RootEntity[] rootEntities,*/ Tuple<int, long> group, out List<ModelEntity> allModels)
        {
            var rootBuilder = _rootEntityModels[group];

            allModels = rootBuilder.AllModels;

            var preparedRootEntities = new RootEntity[rootBuilder.RootEntityModels.Count];
            var index = 0;
            foreach (var rootTuple in rootBuilder.RootEntityModels.Values)
            {
                preparedRootEntities[index++] = rootTuple.Item1;
            }
            return preparedRootEntities;
        }

        // Call this after AddRootEntity has been called for all root entities that plan to be exported.
        // Returns the number of individual models to export
        public Tuple<int, long>[] PrepareAll(RootEntity[] rootEntities)
        {
            switch (_options.ModelGrouping)
            {
                case ExportModelGrouping.GroupAllModels:
                    if (rootEntities.Length > 0)
                    {
                        var group = new Tuple<int, long>(-1, -1);
                        _groups.Add(group);
                    }
                    break;

                case ExportModelGrouping.Default:
                    for (var i = 0; i < rootEntities.Length; i++)
                    {
                        var group = new Tuple<int, long>(i, -1);
                        _groups.Add(group);
                    }
                    break;

                case ExportModelGrouping.SplitSubModelsByTMDID:
                    var tmdids = new HashSet<uint>();
                    for (var i = 0; i < rootEntities.Length; i++)
                    {
                        tmdids.Clear();
                        foreach (ModelEntity model in rootEntities[i].ChildEntities)
                        {
                            tmdids.Add(model.TMDID);
                        }
                        foreach (var tmdid in tmdids.OrderBy(t => t))
                        {
                            var group = new Tuple<int, long>(i, tmdid);
                            _groups.Add(group);
                        }
                    }
                    break;
            }

            Prepare(rootEntities, false, _groups);
            return _groups.ToArray();
        }

        // Call this when preparing to export the current set of root entities.
        // NOTE: This assumes that PrepareCurrent will NEVER be called with the same RootEntity twice!
        public RootEntity[] PrepareCurrent(RootEntity[] rootEntities, Tuple<int, long> group, out List<ModelEntity> allModels)
        {
            CleanupCurrent();

            Prepare(rootEntities, true, new Tuple<int, long>[] { group });
            return GetPreparedRootEntity(group, out allModels);
        }

        private void Prepare(RootEntity[] rootEntities, bool current, IEnumerable<Tuple<int, long>> groups)
        {
            // Add all entities if all entities are processed together.
            // Or add current entities if we need to separate processing.
            if (CanPrepareAll != current)
            {
                // Process root entities now.
                foreach (var group in groups)
                {
                    if (group.Item1 != -1)
                    {
                        var i = group.Item1;
                        AddRootEntity(rootEntities[i], i, group);
                    }
                    else
                    {
                        for (var i = 0; i < rootEntities.Length; i++)
                        {
                            AddRootEntity(rootEntities[i], i, group);
                        }
                    }
                }

                // Assign child entities and coordinates to new root entities.
                foreach (var rootBuilder in _rootEntityModels.Values)
                {
                    foreach (var rootTuple in rootBuilder.RootEntityModels.Values)
                    {
                        var newRootEntity = rootTuple.Item1;
                        var models = rootTuple.Item2;

                        // Setup children array
                        var newChildEntities = new EntityBase[models.Count];
                        for (var i = 0; i < newChildEntities.Length; i++)
                        {
                            newChildEntities[i] = models[i];
                            newChildEntities[i].ParentEntity = newRootEntity;
                        }
                        newRootEntity.ChildEntities = newChildEntities;

                        // Clone coordinates array
                        var coords = newRootEntity.Coords;
                        if (coords != null)
                        {
                            var newCoords = new Coordinate[coords.Length];
                            for (var i = 0; i < coords.Length; i++)
                            {
                                newCoords[i] = new Coordinate(coords[i], newCoords);
                            }
                            newRootEntity.Coords = newCoords;
                        }

                        // Fix connections for new root entity
                        if (_options.AttachLimbs)
                        {
                            newRootEntity.FixConnections(_bakeConnections);
                        }
                    }
                }

                PrepareTiledTextures();

                PrepareSingleTexture(rootEntities, groups);
            }
        }


        private void RedrawEntityTextures(RootEntity rootEntity)
        {
            if (_options.RedrawTextures)
            {
                foreach (var texture in rootEntity.OwnedTextures)
                {
                    var vramTexture = _copiedVRAMPages[texture.TexturePage];
                    if (vramTexture != null)
                    {
                        VRAM.DrawTexture(vramTexture, texture);
                    }
                }
            }
        }

        private void PrepareTiledTextures()
        {
            if (_options.TiledTextures)
            {
                foreach (var tiledInfo in _groupedModels.Values)
                {
                    if (tiledInfo.IsTiled)
                    {
                        PrepareTiledTexture(tiledInfo); // Only need to prepare tiled textures
                    }
                }
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
            tiledInfo.SetupTiledTexture(maxUv, powerOfTwo: !_options.SingleTexture);

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
                            uvs[j] = tiledInfo.ConvertUV(baseUv[j], false);
                        }
                        triangle.Uv = uvs;
                        triangle.TiledUv = null; // We're not tiled anymore.
                    }
                }
            }
        }

        private void PrepareSingleTexture(RootEntity[] rootEntities, IEnumerable<Tuple<int, long>> groups)
        {
            if (_options.SingleTexture)
            {
                // Add models and textures to singleInfo
                _singleInfo = new SingleTextureInfo();
                var addedTextures = new HashSet<Texture>();
                foreach (var group in groups)
                {
                    GetPreparedRootEntity(group, out var models);
                    foreach (var model in models)
                    {
                        if (model.HasTexture)
                        {
                            if (addedTextures.Add(model.Texture))
                            {
                                _singleInfo.OriginalTextures.Add(model.Texture);
                            }
                            _singleInfo.Models.Add(model);
                        }
                    }
                }

                // Compute packing and create a new single texture that contains all other textures.
                _singleInfo.SetupSingleTexture(powerOfTwo: true, sort: true, pack: true, alwaysCreateTexture: false);

                // Assign new single texture to models.
                foreach (var model in _singleInfo.Models)
                {
                    if (model.HasTexture)
                    {
                        var uvConverter = _singleInfo.GetUVConverter(model.Texture);
                        model.Texture = _singleInfo.SingleTexture;

                        // Convert triangle UVs to new single texture UVs (we're no longer 256x256).
                        foreach (var triangle in model.Triangles)
                        {
                            var origUv = triangle.Uv;
                            // Create a new array because we don't own triangle.Uv's array.
                            var uvs = new Vector2[3];
                            for (var j = 0; j < 3; j++)
                            {
                                uvs[j] = uvConverter.ConvertUV(origUv[j], false);
                            }
                            triangle.Uv = uvs;
                            triangle.TiledUv = null; // We're not tiled anymore.
                        }
                    }
                }
            }
        }

        private bool ModelNeedsCopy(ModelEntity model)
        {
            if (_options.AttachLimbs && model.HasAttached)
            {
                return true; // Model vertices need to be changed
            }
            if (model.HasTexture)
            {
                if (_options.SingleTexture || _options.RedrawTextures || (_options.TiledTextures && model.NeedsTiled))
                {
                    return true; // Model texture needs to be changed
                }
                if (model.NeedsTextureLookup)
                {
                    return true;
                }
            }
            return false;
        }

        private Triangle CloneTriangle(Triangle triangle, IUVConverter uvConverter)
        {
            var newTriangle = new Triangle(triangle);
            // If attaching limbs, then we need to clone vertices to prevent overwriting the existing model vertices.
            if (_options.AttachLimbs)
            {
                // Some exporters will assign attached indices even if there are none, so only clone the array if any are found.
                if (newTriangle.AttachedIndices != null)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (newTriangle.AttachedIndices[j] != Triangle.NoAttachment)
                        {
                            newTriangle.Vertices = (Vector3[])newTriangle.Vertices.Clone();
                            break;
                        }
                    }
                }
                if (newTriangle.AttachedNormalIndices != null)
                {
                    for (var j = 0; j < 3; j++)
                    {
                        if (newTriangle.AttachedNormalIndices[j] != Triangle.NoAttachment)
                        {
                            newTriangle.Normals = (Vector3[])newTriangle.Normals.Clone();
                            break;
                        }
                    }
                }
            }
            if (uvConverter != null)
            {
                var tiled = newTriangle.IsTiled;

                var origUvs = newTriangle.Uv;
                var uvs = new Vector2[3];
                for (var j = 0; j < 3; j++)
                {
                    uvs[j] = uvConverter.ConvertUV(origUvs[j], tiled);
                }
                newTriangle.Uv = uvs;

                if (tiled)
                {
                    var origBaseUvs = newTriangle.TiledUv?.BaseUv;
                    var baseUvs = new Vector2[3];
                    for (var j = 0; j < 3; j++)
                    {
                        baseUvs[j] = uvConverter.ConvertUV(origBaseUvs[j], tiled);
                    }
                    newTriangle.TiledUv = new TiledUV(baseUvs, uvConverter.ConvertTiledArea(newTriangle.TiledUv.Area));
                }
            }
            return newTriangle;
        }

        private void SeparateModel(RootEntity rootEntity, int rootIndex, ModelEntity model)
        {
            var modelNeedsCopy = ModelNeedsCopy(model);

            // Create a copy of the VRAM page so that we can redraw textures to it.
            var texture = model.Texture;
            if (modelNeedsCopy && _options.RedrawTextures && model.HasTexture)
            {
                var texturePage = texture.TexturePage;
                if (texture.IsVRAMPage)
                {
                    var vramTexture = _copiedVRAMPages[texturePage];
                    if (_copiedVRAMPages[texturePage] == null)
                    {
                        // A copy doesn't exist yet, create one.
                        vramTexture = new Texture(texture, true);
                        vramTexture.TextureName = $"{texture.TextureName ?? nameof(Texture)} Copy";
                        _copiedVRAMPages[texturePage] = vramTexture;
                    }
                    texture = vramTexture;
                }
            }

            var uvConverter = model.TextureLookup;
            // Currently we're forced to create copies to support root entity transforms.
            /*if (!modelNeedsCopy)
            {
                // No changes to the model.
                AddModel(rootEntity, model, Vector4.Zero);
            }
            else*/ if (!modelNeedsCopy || !_options.TiledTextures)
            {
                // Create copies for every model with copies of each triangle.
                var triangles = model.Triangles;
                var newTriangles = new Triangle[triangles.Length];
                for (var i = 0; i < triangles.Length; i++)
                {
                    newTriangles[i] = modelNeedsCopy ? CloneTriangle(triangles[i], uvConverter) : triangles[i];
                }
                var newModel = new ModelEntity(model, newTriangles)
                {
                    Texture = texture,
                };
                AddModel(rootEntity, rootIndex, newModel, Vector4.Zero);
            }
            else
            {
                // Separate models by tiled area, and force-create copies if we're creating a single texture.
                var groupedTriangles = new Dictionary<Vector4, List<Triangle>>();

                var needsTextureLookup = model.NeedsTextureLookup;
                foreach (var triangle in model.Triangles)
                {
                    var needsTiled = false;
                    if (_options.TiledTextures)
                    {
                        if (needsTextureLookup)
                        {
                            needsTiled = triangle.IsTiled;
                        }
                        else
                        {
                            needsTiled = triangle.NeedsTiled;
                        }
                    }
                    var triangleNeedsCopy = true;// _options.SingleTexture || _options.AttachLimbs || needsTiled || needsTextureLookup;
                    var newTriangle = triangleNeedsCopy ? CloneTriangle(triangle, uvConverter) : triangle;
                    // Use newTriangle since it will have the converted packed tiled area
                    var tiledArea = needsTiled ? newTriangle.TiledArea.Value : Vector4.Zero;
                    if (!groupedTriangles.TryGetValue(tiledArea, out var triangles))
                    {
                        triangles = new List<Triangle>();
                        groupedTriangles.Add(tiledArea, triangles);
                    }
                    // We only need to create a copy for tiled triangles, since we'll be modifying those.
                    triangles.Add(newTriangle);
                }

                foreach (var kvp in groupedTriangles)
                {
                    var tiledArea = kvp.Key;
                    var triangles = kvp.Value;
                    var newModel = new ModelEntity(model, triangles.ToArray())
                    {
                        Texture = texture,
                    };
                    if (_options.AttachLimbs && model.HasAttached)
                    {
                        // Recompute attached since, we may no longer have attached vertices.
                        newModel.ComputeAttached();
                    }
                    AddModel(rootEntity, rootIndex, newModel, tiledArea);
                }
                if (groupedTriangles.Count == 0)
                {
                    // Make sure we add a model in-case we have attachables that are needed for FixConnections.
                    var newModel = new ModelEntity(model, new Triangle[0]);
                    AddModel(rootEntity, rootIndex, newModel, Vector4.Zero);
                }
            }
        }

        private void AddRootEntity(RootEntity rootEntity, int rootIndex, Tuple<int, long> group)
        {
            foreach (ModelEntity model in rootEntity.ChildEntities)
            {
                if (group.Item2 == -1 || group.Item2 == model.TMDID)
                {
                    SeparateModel(rootEntity, rootIndex, model);
                }
            }

            RedrawEntityTextures(rootEntity);
        }

        private void AddModel(RootEntity rootEntity, int rootIndex, ModelEntity model, Vector4 tiledArea)
        {
            if (model.HasTexture)
            {
                var tuple = new Tuple<uint, Vector4>(model.TexturePage, tiledArea);
                if (!_groupedModels.TryGetValue(tuple, out var tiledInfo))
                {
                    tiledInfo = new TiledTextureInfo(model.Texture, tiledArea);
                    _groupedModels.Add(tuple, tiledInfo);
                }
                tiledInfo.Models.Add(model);
            }

            var group = GetModelGroup(rootIndex, model);
            if (!_rootEntityModels.TryGetValue(group, out var rootBuilder))
            {
                rootBuilder = new RootEntityBuilder();
                _rootEntityModels.Add(group, rootBuilder);
            }
            if (!rootBuilder.RootEntityModels.TryGetValue(rootEntity, out var rootTuple))
            {
                var newRootEntity = new RootEntity(rootEntity);
                var models = new List<ModelEntity>();
                rootTuple = new Tuple<RootEntity, List<ModelEntity>>(newRootEntity, models);
                rootBuilder.RootEntityModels.Add(rootEntity, rootTuple);
            }
            rootTuple.Item2.Add(model);
            // Ideally we would want to exclude models with zero trangles from the AllModels list,
            // but currently we expect the order of models in all root entities to match the order of the AllModels list.
            // An empty model would be added in the scenario that it exists to store attachable vertices.
            rootBuilder.AllModels.Add(model);
        }

        private Tuple<int, long> GetModelGroup(int rootIndex, ModelEntity model)
        {
            switch (_options.ModelGrouping)
            {
                default:
                case ExportModelGrouping.GroupAllModels:
                    return new Tuple<int, long>(-1, -1);

                case ExportModelGrouping.Default:
                    return new Tuple<int, long>(rootIndex, -1);

                case ExportModelGrouping.SplitSubModelsByTMDID:
                    return new Tuple<int, long>(rootIndex, model.TMDID);
            }
        }

        private void CleanupCurrent()
        {
            if (!CanPrepareAll)
            {
                Dispose(); // Dispose does everything we need and nothing more
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < _copiedVRAMPages.Length; i++)
            {
                _copiedVRAMPages[i]?.Dispose();
                _copiedVRAMPages[i] = null;
            }
            if (_singleInfo != null)
            {
                _singleInfo.Dispose();
                _singleInfo = null;
            }
            foreach (var tiledInfo in _groupedModels.Values)
            {
                tiledInfo.Dispose();
            }
            _groupedModels.Clear();
            _rootEntityModels.Clear();
        }

        // Order by:
        // 1. Texture pages:
        //     i. Page:   lowest to highest
        // 2. Loose textures:
        //     i. Width:  highest to lowest
        //    ii. Height: highest to lowest
        //   iii. Page:   lowest to highest
        private static int CompareTextures(Texture a, Texture b)
        {
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
        }


        private class SingleTextureInfo : IDisposable
        {
            // Use this as an alignment since texture tiling is always in units of 8.
            private const int PACKED_ALIGN = 8;

            public class PackedTextureInfo : IUVConverter
            {
                //public Texture Texture;
                public float OffsetX;
                public float OffsetY;
                public float ScalarX;
                public float ScalarY;

                public Vector2 ConvertUV(Vector2 origUv, bool tiled)
                {
                    return new Vector2((tiled ? 0f : OffsetX) + origUv.X * ScalarX,
                                       (tiled ? 0f : OffsetY) + origUv.Y * ScalarY);
                }

                Vector4 IUVConverter.ConvertTiledArea(Vector4 tiledArea) => throw new NotImplementedException();
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

            public void SetupSingleTexture(bool powerOfTwo, bool sort, bool pack, bool alwaysCreateTexture = false)
            {
                if (!alwaysCreateTexture && OriginalTextures.Count == 0 && Models.Count == 0)
                {
                    return; // No textures or textured models. We don't need to create a texture.
                }
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
                    OriginalTextures.Sort(CompareTextures);
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
                if (powerOfTwo)
                {
                    total = GeomMath.RoundUpToPower(total, 2);
                }
                _countX = Math.Max(1, total);
                _countY = 1;
                var w = 1;
                while (w <= total)
                {
                    // Round height up to account for partially-used rows.
                    var h = (total + w - 1) / w;

                    if (powerOfTwo)
                    {
                        // Find a power of two for the height.
                        h = GeomMath.RoundUpToPower(h, 2);
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
                        SingleTexture = new Texture(bitmap, 0, 0, 32, 0, 0, null, null);
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


        private class TiledTextureInfo : IUVConverter, IDisposable
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


            public Vector2 ConvertUV(Vector2 baseUv, bool tiled)
            {
                return new Vector2(baseUv.X * _scalarX, baseUv.Y * _scalarY);
            }

            Vector4 IUVConverter.ConvertTiledArea(Vector4 tiledArea) => throw new NotImplementedException();

            public void SetupTiledTexture(Vector2 maxUv, bool powerOfTwo)
            {
                // Make sure repeat is at least one or higher.
                _repeatX = (int)Math.Max(1, Width  != 0f ? Math.Ceiling(maxUv.X / Width)  : 1f);
                _repeatY = (int)Math.Max(1, Height != 0f ? Math.Ceiling(maxUv.Y / Height) : 1f);

                // We can't just use this, since we still need to support disabling the UV alignment fix.
                //var uvScalar = (float)VRAM.PageSize;
                var uvScalar = GeomMath.UVScalar;
                var srcX = (int)(X * uvScalar);
                var srcY = (int)(Y * uvScalar);
                var srcWidth  = Width  != 0f ? (int)(Width  * uvScalar) : VRAM.PageSize;
                var srcHeight = Height != 0f ? (int)(Height * uvScalar) : VRAM.PageSize;
                var srcRect = new Rectangle(srcX, srcY, srcWidth, srcHeight);

                var fullWidth  = _repeatX * srcWidth;
                var fullHeight = _repeatY * srcHeight;
                if (powerOfTwo)
                {
                    fullWidth  = GeomMath.RoundUpToPower(fullWidth,  2);
                    fullHeight = GeomMath.RoundUpToPower(fullHeight, 2);
                }

                //_scalarX = (Width  != 0f ? 1f / (Width  * _repeatX) : 1f);
                //_scalarY = (Height != 0f ? 1f / (Height * _repeatY) : 1f);
                _scalarX = (Width  != 0f ? uvScalar / fullWidth  : 1f);
                _scalarY = (Height != 0f ? uvScalar / fullHeight : 1f);

                if (TiledTexture == null)
                {
                    var bitmap = VRAM.ConvertTiledTexture(OriginalTexture, srcRect, _repeatX, _repeatY, fullWidth, fullHeight, false);
                    try
                    {
                        TiledTexture = new Texture(bitmap, 0, 0, 32, TexturePage, 0, null, null);
                        //TiledTexture.TextureName = $"Tiled[{srcX},{srcY} {srcWidth}x{srcHeight}]";
                        TiledTexture.TextureName = $"Tiled[{_repeatX}x{_repeatY}]";
                        //TiledTexture.SemiTransparentMap = VRAM.ConvertTiledTexture(OriginalTexture, srcRect, _repeatX, _repeatY, fullWidth, fullHeight, true);
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
