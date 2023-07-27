using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Parsers
{
    public class MODParser : FileOffsetScanner
    {
        public MODParser(EntityAddedAction entityAdded)
            : base(entityAdded: entityAdded)
        {
        }

        public override string FormatName => "MOD";

        protected override void Parse(BinaryReader reader)
        {
            var rootEntity = ReadModels(reader);
            if (rootEntity != null)
            {
                EntityResults.Add(rootEntity);
            }
        }

        // The real divisor is supposedly 4096f, but that creates VERY SMALL models.
        private const float FIXED_DIV = 16f; // 4096f;

        private static Vector3 ReadVector(BinaryReader reader, bool normal = false, bool readPad = true)
        {
            var div = (normal ? 4096f : FIXED_DIV);
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

        private static RootEntity ReadModels(BinaryReader reader)
        {
            var groupedTriangles = new Dictionary<RenderInfo, List<Triangle>>();

            void AddTriangle(Triangle triangle, uint tPage, RenderFlags renderFlags)
            {
                renderFlags |= RenderFlags.DoubleSided; //todo
                if (renderFlags.HasFlag(RenderFlags.Textured))
                {
                    triangle.CorrectUVTearing();
                }
                var renderInfo = new RenderInfo(tPage, renderFlags);
                if (!groupedTriangles.TryGetValue(renderInfo, out var triangles))
                {
                    triangles = new List<Triangle>();
                    groupedTriangles.Add(renderInfo, triangles);
                }
                triangles.Add(triangle);
            }

            // Some data is properly handled thanks to vs49688's work on CrocUtils.
            // Notably: Fixed point size, flags, bounding box indices, color, UVs, and the collision section.
            // <https://github.com/vs49688/CrocUtils>

            // Many MOD files with sub-models seem to lack positioning information for each sub-model.
            // For example, TK##_TRK files are a giant collection of grid pieces all occupying the same space.
            // This may be handled by another file...

            var count = reader.ReadUInt16();
            if (count == 0 || count > Program.MaxMODModels)
            {
                return null;
            }
            var flags = reader.ReadUInt16();
            var collision = ((flags >> 0) & 0x1) == 1;

            var models = new List<ModelEntity>();
            for (uint i = 0; i < count; i++)
            {
                // Same fixed point fraction size used for Int16's.
                var radius = reader.ReadInt32() / FIXED_DIV;

                // Bounding box indices: <https://github.com/vs49688/CrocUtils/blob/5e07b7cb60fb5edec465a509d8924ae6682eaba7/libcroc/include/libcroc/moddef.h#L97-L108>
                var boundingBox = new Vector3[9]; // 0 is the center.
                for (var j = 0; j < 9; j++)
                {
                    boundingBox[j] = ReadVector(reader);
                }

                // See notes by countFaces.
                var countVerts = reader.ReadUInt32();
                if (countVerts > Program.MaxMODVertices)
                {
                    return null;
                }
                var vertices = new Vector3[countVerts];
                var normals = new Vector3[countVerts];
                for (var j = 0; j < countVerts; j++)
                {
                    vertices[j] = ReadVector(reader);
                }
                for (var j = 0; j < countVerts; j++)
                {
                    normals[j] = ReadVector(reader, normal: true);
                }

                // It's valid for some models to define 0 vertices and/or faces.
                // When this happens, it's likely that the MOD file is split up into multiple sub-models,
                // and the one at this index is intended to be empty.
                // 
                // Even if the counts are zero, we need to keep reading till the end of this sub-model,
                // so that the reader offset for the next sub-model is correct.
                var countFaces = reader.ReadUInt32();
                if (countFaces > 0 && countVerts == 0)
                {
                    // Although this check is technically handled in the faces loop,
                    // we can check this now to set the capacity of triangles without wasting memory.
                    return null;
                }
                if (countFaces > Program.MaxMODFaces)
                {
                    return null;
                }

                // Groups are observed as consecutive faces that share some similar properties.
                // They are defined by the fourth uint16 in a face. When non-zero, a new group is started.
                // A non-zero value specifies how many faces (including this one) remain in the group.
                // After that many values have passed, either a new group is started, or the end of the face list is reached.
                // The only similar properties observed in groups is that each face always uses the same texture/quad flags.
                //var groupRemaining = 0;

                for (var j = 0; j < countFaces; j++)
                {
                    // Not present in MOD files for the PSX.
                    //var materialName = Encoding.ASCII.GetString(reader.ReadBytes(64));

                    // This vector almost always has a length nearing 1 (more often than the normals list).
                    // Only two files has been found where the vector length was 0.
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
                    if (index0 >= countVerts || index1 >= countVerts || index2 >= countVerts || index3 >= countVerts)
                    {
                        return null;
                    }

                    var faceInfo = reader.ReadUInt32();
                    var faceFlags = (faceInfo >> 24) & 0xff;
                    var texture = ((faceFlags >> 0) & 0x1) == 1;
                    var quad    = ((faceFlags >> 3) & 0x1) == 1;
                    var uvFlip  = ((faceFlags >> 4) & 0x1) == 1;
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

                    var vertex0 = vertices[index0];
                    var vertex1 = vertices[index1];
                    var vertex2 = vertices[index2];
                    var normal0 = normals[index0];
                    var normal1 = normals[index1];
                    var normal2 = normals[index2];
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
                    uint tPage;

                    // We can't actually use UVs yet, since they're likely scaled for the material, and not the whole VRAM page.
                    Vector2 uv0, uv1, uv2, uv3;
                    Color color;
                    if (texture)
                    {
                        renderFlags |= RenderFlags.Textured;
                        if (!quad)
                        {
                            // Is uvFlip flag ignored for tri? (flag has been observed both set and unset)
                            uv0 = new Vector2(0, 0);
                            uv1 = new Vector2(0, 1);
                            uv2 = new Vector2(1, 0);
                            uv3 = Vector2.Zero;
                        }
                        else
                        {
                            if (!uvFlip)
                            {
                                uv0 = new Vector2(1, 0);
                                uv1 = new Vector2(0, 0);
                                uv2 = new Vector2(1, 1);
                                uv3 = new Vector2(0, 1);
                            }
                            else
                            {
                                uv0 = new Vector2(0, 0);
                                uv1 = new Vector2(1, 0);
                                uv2 = new Vector2(0, 1);
                                uv3 = new Vector2(1, 1);
                            }
                        }

                        var materialId = faceInfo & 0xffff;
                        color = Color.Grey;

                        tPage = 0; //todo
                    }
                    else
                    {
                        uv0 = uv1 = uv2 = uv3 = Vector2.Zero;

                        var r = ((faceInfo >>  0) & 0xff) / 255f;
                        var g = ((faceInfo >>  8) & 0xff) / 255f;
                        var b = ((faceInfo >> 16) & 0xff) / 255f;
                        color = new Color(r, g, b);

                        tPage = 0; //todo
                    }

                    AddTriangle(new Triangle
                    {
                        Vertices = new[] { vertex0, vertex1, vertex2 },
                        Normals = new[] { normal0, normal1, normal2 },
                        Colors = new[] { color, color, color },
                        Uv = new[] { Vector2.Zero, Vector2.Zero, Vector2.Zero }, // Can't use UVs yet
                        AttachableIndices = new [] {uint.MaxValue, uint.MaxValue, uint.MaxValue}
                    }, tPage, renderFlags);
                    if (quad)
                    {
                        var vertex3 = vertices[index3];
                        var normal3 = normals[index3];
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

                        AddTriangle(new Triangle
                        {
                            Vertices = new[] { vertex1, vertex3, vertex2 },
                            Normals = new[] { normal1, normal3, normal2 },
                            Colors = new[] { color, color, color },
                            Uv = new[] { Vector2.Zero, Vector2.Zero, Vector2.Zero }, // Can't use UVs yet
                            AttachableIndices = new[] { uint.MaxValue, uint.MaxValue, uint.MaxValue }
                        }, tPage, renderFlags);
                    }
                }

                if (collision)
                {
                    // Skip collision(?) data.
                    var size1 = reader.ReadUInt16();
                    var size2 = reader.ReadUInt16();
                    reader.BaseStream.Seek((size1 + size2) * 44, SeekOrigin.Current);
                }

                foreach (var kvp in groupedTriangles)
                {
                    var renderInfo = kvp.Key;
                    var triangles = kvp.Value;
                    var model = new ModelEntity
                    {
                        Triangles = triangles.ToArray(),
                        TexturePage = renderInfo.TexturePage,
                        RenderFlags = renderInfo.RenderFlags,
                        MixtureRate = renderInfo.MixtureRate,
                        TMDID = i + 1, //todo
                    };
                    models.Add(model);
                }
                groupedTriangles.Clear();
            }
            if (models.Count > 0)
            {
                var entity = new RootEntity();
                foreach (var model in models)
                {
                    model.ParentEntity = entity;
                }
                entity.ChildEntities = models.ToArray();
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }
    }
}
