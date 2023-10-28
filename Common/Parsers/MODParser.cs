using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Parsers
{
    // Argonaut's Blazing Render engine: .MOD model format
    public class MODParser : FileOffsetScanner
    {
        public const string FormatNameConst = "MOD";

        private float _scaleDivisor = 1f;
        //private readonly Vector3[] _boundingBox = new Vector3[9];
        private Vector3[] _vertices;
        private Vector3[] _normals;
        private readonly Dictionary<RenderInfo, List<Triangle>> _groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();
        private readonly List<ModelEntity> _models = new List<ModelEntity>();

        public MODParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => FormatNameConst;

        protected override void Parse(BinaryReader reader)
        {
            _scaleDivisor = Settings.Instance.AdvancedMODScaleDivisor;
            _groupedTriangles.Clear();
            _models.Clear();

            ReadMOD(reader);
        }

        private Vector3 ReadVector(BinaryReader reader, bool normal = false, bool readPad = true)
        {
            var div = (normal ? 4096f : _scaleDivisor);
            var x = reader.ReadInt16() / div;
            var y = reader.ReadInt16() / div;
            var z = reader.ReadInt16() / div;
            if (readPad)
            {
                var pad = reader.ReadUInt16();
            }
            // Note: With this order, some models do seem right-side up,
            // but many seem to be pitched at a 90deg angle (up?/down?).
            // Note that the later models don't look flipped or anything. i.e. letters and buttons are correct.
            return new Vector3(x, -z, y);
        }

        private bool ReadMOD(BinaryReader reader)
        {
            // Some data is properly handled thanks to vs49688's work on CrocUtils.
            // Notably: Fixed point size, flags, bounding box indices, color, UVs, and the collision section.
            // <https://github.com/vs49688/CrocUtils>

            // Many MOD files with sub-models seem to lack positioning information for each sub-model.
            // For example, TK##_TRK files are a giant collection of grid pieces all occupying the same space.
            // This may be handled by another file...

            var modelCount = reader.ReadUInt16();
            if (modelCount == 0 || modelCount > Limits.MaxMODModels)
            {
                return false;
            }
            var flags = reader.ReadUInt16();
            var collision = ((flags >> 0) & 0x1) == 1;

            for (uint i = 0; i < modelCount; i++)
            {
                if (!ReadModel(reader, i, collision))
                {
                    return false;
                }
            }

            if (_models.Count > 0)
            {
                var rootEntity = new RootEntity();
                rootEntity.ChildEntities = _models.ToArray();
                rootEntity.ComputeBounds();
                EntityResults.Add(rootEntity);
                return true;
            }

            return false;
        }

        private bool ReadModel(BinaryReader reader, uint modelIndex, bool collision)
        {
            // Same fixed point fraction size used for Int16's.
            var radius = reader.ReadInt32() / _scaleDivisor;

            // Bounding box indices: <https://github.com/vs49688/CrocUtils/blob/5e07b7cb60fb5edec465a509d8924ae6682eaba7/libcroc/include/libcroc/moddef.h#L97-L108>
            // 0 is the center.
            for (var j = 0; j < 9; j++)
            {
                /*_boundingBox[j] =*/ ReadVector(reader);
            }

            // See notes by countFaces.
            var vertexCount = reader.ReadUInt32();
            if (vertexCount > Limits.MaxMODVertices)
            {
                return false;
            }
            if (_vertices == null || _vertices.Length < vertexCount)
            {
                _vertices = new Vector3[vertexCount];
                _normals = new Vector3[vertexCount];
            }
            for (var j = 0; j < vertexCount; j++)
            {
                _vertices[j] = ReadVector(reader);
            }
            for (var j = 0; j < vertexCount; j++)
            {
                _normals[j] = ReadVector(reader, normal: true);
            }

            // It's valid for some models to define 0 vertices and/or faces.
            // When this happens, it's likely that the MOD file is split up into multiple sub-models,
            // and the one at this index is intended to be empty.
            // 
            // Even if the counts are zero, we need to keep reading till the end of this sub-model,
            // so that the reader offset for the next sub-model is correct.
            var faceCount = reader.ReadUInt32();
            if (faceCount > 0 && vertexCount == 0)
            {
                // Although this check is technically handled in the faces loop,
                // we can check this now to set the capacity of triangles without wasting memory.
                return false;
            }
            if (faceCount > Limits.MaxMODFaces)
            {
                return false;
            }

            // Groups are observed as consecutive faces that share some similar properties.
            // They are defined by the fourth uint16 in a face. When non-zero, a new group is started.
            // A non-zero value specifies how many faces (including this one) remain in the group.
            // After that many values have passed, either a new group is started, or the end of the face list is reached.
            // The only similar properties observed in groups is that each face always uses the same texture/quad flags.
            //var groupRemaining = 0;

            for (var j = 0; j < faceCount; j++)
            {
                // Not present in MOD files for the PSX.
                //var materialName = Encoding.ASCII.GetString(reader.ReadBytes(64));

                // This vector almost always has a length nearing 1 (more often than the normals list).
                // Only two files have been found where the vector length was 0.
                // (NEPT00.MOD and NEPTD00.MOD, where flags=0x00)
                // 
                // My theory is that this is an opportunity to allow using
                // a normal that differs from the vertex indices.
                var unitVec = ReadVector(reader, normal: true, readPad: false);
                var groupLength = reader.ReadUInt16(); // Zero means we're already in a group.

                var index0 = reader.ReadUInt16();
                var index1 = reader.ReadUInt16();
                var index2 = reader.ReadUInt16();
                var index3 = reader.ReadUInt16();
                if (index0 >= vertexCount || index1 >= vertexCount || index2 >= vertexCount || index3 >= vertexCount)
                {
                    return false;
                }

                var faceInfo = reader.ReadUInt32();
                var faceFlags = (faceInfo >> 24) & 0xff;
                var textured = ((faceFlags >> 0) & 0x1) == 1;
                var quad     = ((faceFlags >> 3) & 0x1) == 1;
                var uvFlip   = ((faceFlags >> 4) & 0x1) == 1;
                // This is just a guess, and I'm not confident in it.
                //var gouraud = ((faceFlags >> 7) & 0x1) == 0;
                // All other flag bits have been spotted (set and unset), the most common being 0x80.

                //if (groupLength != 0)
                //{
                //    groupRemaining = groupLength;
                //}
                //else if (groupRemaining <= 0)
                //{
                //}
                //groupRemaining--;

                var vertex0 = _vertices[index0];
                var vertex1 = _vertices[index1];
                var vertex2 = _vertices[index2];
                var vertex3 = quad ? _vertices[index3] : Vector3.Zero;
                var normal0 = _normals[index0];
                var normal1 = _normals[index1];
                var normal2 = _normals[index2];
                var normal3 = quad ? _normals[index3] : Vector3.Zero;
                // Just a guess.
                //if (!gouraud && !unitVec.IsZero())
                //{
                //    normal0 = normal1 = normal2 = unitVec;
                //}
                //else
                if (normal0.IsZero() || normal1.IsZero() || normal2.IsZero())
                {
                    normal0 = normal1 = normal2 = GeomMath.CalculateNormal(vertex0, vertex1, vertex2);
                }

                var renderFlags = RenderFlags.None;
                uint tPage = 0;

                // We can't actually use UVs yet, since they're likely scaled for the material, and not the whole VRAM page.
                Vector2 uv0, uv1, uv2, uv3;
                Color3 color;
                if (textured)
                {
                    //renderFlags |= RenderFlags.Textured; // Not supported yet
                    tPage = faceInfo & 0xffff;
                    if (!quad)
                    {
                        // Is uvFlip flag ignored for tri? (flag has been observed both set and unset)
                        uv0 = new Vector2(0f, 0f);
                        uv1 = new Vector2(0f, 1f);
                        uv2 = new Vector2(1f, 0f);
                        uv3 = Vector2.Zero;
                    }
                    else
                    {
                        if (!uvFlip)
                        {
                            uv0 = new Vector2(1f, 0f);
                            uv1 = new Vector2(0f, 0f);
                            uv2 = new Vector2(1f, 1f);
                            uv3 = new Vector2(0f, 1f);
                        }
                        else
                        {
                            uv0 = new Vector2(0f, 0f);
                            uv1 = new Vector2(1f, 0f);
                            uv2 = new Vector2(0f, 1f);
                            uv3 = new Vector2(1f, 1f);
                        }
                    }

                    color = Color3.Grey;
                }
                else
                {
                    uv0 = uv1 = uv2 = uv3 = Vector2.Zero;

                    var r = (byte)(faceInfo      );
                    var g = (byte)(faceInfo >>  8);
                    var b = (byte)(faceInfo >> 16);
                    color = new Color3(r, g, b);
                }

                var triangle1 = new Triangle
                {
                    Vertices = new[] { vertex0, vertex1, vertex2 },
                    Normals = new[] { normal0, normal1, normal2 },
                    Colors = new[] { color, color, color },
                    Uv = Triangle.EmptyUv, // Can't use UVs yet
                };
                if (textured)
                {
                    //triangle1.TiledUv = new TiledUV(triangle1.Uv, 0f, 0f, 1f, 1f);
                    //triangle1.Uv = (Vector2[])triangle1.Uv.Clone();
                }
                AddTriangle(triangle1, tPage, renderFlags);

                if (quad)
                {
                    // Just a guess.
                    //if (!gouraud && !unitVec.IsZero())
                    //{
                    //    normal1 = normal3 = normal2 = unitVec;
                    //}
                    //else
                    if (normal1.IsZero() || normal3.IsZero() || normal2.IsZero())
                    {
                        normal1 = normal3 = normal2 = GeomMath.CalculateNormal(vertex1, vertex3, vertex2);
                    }

                    var triangle2 = new Triangle
                    {
                        Vertices = new[] { vertex1, vertex3, vertex2 },
                        Normals = new[] { normal1, normal3, normal2 },
                        Colors = new[] { color, color, color },
                        Uv = Triangle.EmptyUv, // Can't use UVs yet
                    };
                    if (textured)
                    {
                        //triangle2.TiledUv = new TiledUV(triangle2.Uv, 0f, 0f, 1f, 1f);
                        //triangle2.Uv = (Vector2[])triangle2.Uv.Clone();
                    }
                    AddTriangle(triangle2, tPage, renderFlags);
                }
            }

            if (collision)
            {
                // Skip collision(?) data.
                var size1 = reader.ReadUInt16();
                var size2 = reader.ReadUInt16();
                reader.BaseStream.Seek((size1 + size2) * 44, SeekOrigin.Current);
            }

            FlushModels(modelIndex);
            return true;
        }

        private void FlushModels(uint modelIndex)
        {
            foreach (var kvp in _groupedTriangles)
            {
                var renderInfo = kvp.Key;
                var triangles = kvp.Value;
                var model = new ModelEntity
                {
                    Triangles = triangles.ToArray(),
                    TexturePage = 0,
                    //TextureLookup = CreateTextureLookup(renderInfo),
                    RenderFlags = renderInfo.RenderFlags,
                    MixtureRate = renderInfo.MixtureRate,
                    TMDID = modelIndex + 1,
                };
                _models.Add(model);
            }
            _groupedTriangles.Clear();
        }

        private static TextureLookup CreateTextureLookup(RenderInfo renderInfo)
        {
            if (renderInfo.RenderFlags.HasFlag(RenderFlags.Textured))
            {
                return new TextureLookup
                {
                    ID = renderInfo.TexturePage, // Numeric (index?) identifier
                    //ExpectedFormat = , //todo
                    UVConversion = TextureUVConversion.TextureSpace,
                    TiledAreaConversion = TextureUVConversion.TextureSpace,
                };
            }
            return null;
        }

        private void AddTriangle(Triangle triangle, uint tPage, RenderFlags renderFlags, MixtureRate mixtureRate = MixtureRate.None)
        {
            renderFlags |= RenderFlags.DoubleSided; //todo
            if (renderFlags.HasFlag(RenderFlags.Textured))
            {
                triangle.CorrectUVTearing();
            }
            var renderInfo = new RenderInfo(tPage, renderFlags, mixtureRate);
            if (!_groupedTriangles.TryGetValue(renderInfo, out var triangles))
            {
                triangles = new List<Triangle>();
                _groupedTriangles.Add(renderInfo, triangles);
            }
            triangles.Add(triangle);
        }
    }
}
