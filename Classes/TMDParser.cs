using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using OpenTK;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class TMDPacketStructureColumn
    {
        public bool IsPadding { get; set; }
        public bool IsVertex { get => Name.StartsWith("VERT"); }
        public bool IsNormal { get => Name.StartsWith("NORM"); }
        public bool IsColor { get => Name[0] == 'R' || Name[0] == 'G' || Name[0] == 'B'; }
        public bool IsTexcoord { get => Name[0] == 'S' || Name[0] == 'T'; }

        public string Name { get; set; }
        public int Length { get; set; }

        public TMDPacketStructureColumn(string name, int length, int? expectedValue = null)
        {
            Name = name;
            Length = length;
            if (name == "_")
            {
                IsPadding = true;
            }
        }

        public static implicit operator TMDPacketStructureColumn(string meta)
        {
            var props = meta.Split('-');
            var name = props[0];
            var length = int.Parse(props[1]);
            return new TMDPacketStructureColumn(name, length);
        }
    }

    public class TMDPacketStructure
    {
        public string Primitive { get; set; }
        public string Classification { get; set; }
        public List<TMDPacketStructureColumn> Structure { get; set; }

        public int TotalColumns
        {
            get
            {
                return Structure.Sum(x => x.Length);
            }
        }

        public override string ToString()
        {
            var map = "---------------------------------\n";
            var rows = TotalColumns / 4;
            var columnIndex = 0;
            var structureIndex = 0;
            var carry = 0;
            for (var i = 0; i < rows; ++i)
            {
                var jmpStruct = 0;
                map += "|";
                var cols = string.Empty;
                for (var j = structureIndex; j < structureIndex + 4; ++j)
                {
                    var col = Structure[j];
                    cols = $"{(col.IsPadding ? string.Empty : col.Name)}{new String('\t', col.Length)}| " + cols;
                    columnIndex += col.Length;
                    jmpStruct++;
                    if (columnIndex >= 4)
                    {
                        break;
                    }
                }
                map += cols;
                columnIndex = 0;
                structureIndex += jmpStruct;
                map += "\n---------------------------------\n";
            }

            var body = $"Primitive: {Primitive}\n" +
                $"Classification: {Classification}\n" +
                map;
            return body;
        }
    }

    public class TMDParser
    {
        private long _offset;
        private Action<RootEntity, long> entityAddedAction;

        public TMDParser(Action<RootEntity, long> entityAdded)
        {
            entityAddedAction = entityAdded;
        }

        public RootEntity[] LookForTmd(BinaryReader reader, string fileTitle)
        {
            if (reader == null)
            {
                throw (new Exception("File must be opened"));
            }

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var entities = new List<RootEntity>();

            while (reader.BaseStream.CanRead)
            {
                _offset = reader.BaseStream.Position;
                try
                {
                    var version = reader.ReadUInt32();
                    if (version == 0x00000041)
                    {
                        var entity = ParseTmd(reader, _offset);
                        if (entity != null)
                        {
                            entity.EntityName = string.Format("{0}{1:X}", fileTitle, _offset > 0 ? "_" + _offset : string.Empty);
                            entities.Add(entity);
                            entityAddedAction(entity, reader.BaseStream.Position);
                            Program.Logger.WriteLine("Found TMD Model at offset {0:X}", _offset);
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (exp is EndOfStreamException)
                    {
                        //if (checkOffset >= reader.BaseStream.Length - 4)
                        //{
                        break;
                        //}
                        //reader.BaseStream.Seek(checkOffset + 1, SeekOrigin.Begin);
                    }
                    Program.Logger.WriteLine(exp);
                } finally
                {
                    reader.BaseStream.Seek(_offset + 1, SeekOrigin.Begin);
                }
            }
            return entities.ToArray();
        }

        private RootEntity ParseTmd(BinaryReader reader, long startAddress)
        {
            var flags = reader.ReadUInt32();
            if (flags != 0 && flags != 1)
            {
                return null;
            }

            var nObj = reader.ReadUInt32();
            if (nObj == 0 || nObj > 5000)
            {
                return null;
            }

            var models = new List<ModelEntity>();

            var objBlocks = new ObjBlock[nObj];

            var objOffset = reader.BaseStream.Position;

            for (var o = 0; o < nObj; o++)
            {
                var vertTop = reader.ReadUInt32();
                var nVert = reader.ReadUInt32();
                var normalTop = reader.ReadUInt32();
                var nNormal = reader.ReadUInt32();
                var primitiveTop = reader.ReadUInt32();
                var nPrimitive = reader.ReadUInt32();
                var scale = reader.ReadInt32();

                if (flags == 0)
                {
                    vertTop += (uint)objOffset;
                    normalTop += (uint)objOffset;
                    primitiveTop += (uint)objOffset;
                }

                objBlocks[o] = new ObjBlock
                {
                    VertTop = vertTop,
                    NVert = nVert,
                    NormalTop = normalTop,
                    NNormal = nNormal,
                    PrimitiveTop = primitiveTop,
                    NPrimitive = nPrimitive,
                    Scale = scale
                };
            }

            for (int o = 0; o < objBlocks.Length; o++)
            {
                var objBlock = objBlocks[o];
                var scale = (float)Math.Pow(objBlock.Scale, 2);
                if(scale == 0)
                {
                    scale = 1;
                }

                var vertices = new Vector3[objBlock.NVert];
                reader.BaseStream.Seek(objBlock.VertTop, SeekOrigin.Begin);
                for (var v = 0; v < objBlock.NVert; v++)
                {
                    var vx = reader.ReadInt16();
                    var vy = reader.ReadInt16();
                    var vz = reader.ReadInt16();
                    var pad = reader.ReadInt16();
                    var vertex = new Vector3
                    {
                        X = (float)vx * scale,
                        Y = (float)vy * scale,
                        Z = (float)vz * scale
                    };
                    vertices[v] = vertex;
                }

                var normals = new Vector3[objBlock.NNormal];
                reader.BaseStream.Seek(objBlock.NormalTop, SeekOrigin.Begin);
                for (var n = 0; n < objBlock.NNormal; n++)
                {
                    var nx = FInt.Create(reader.ReadInt16());
                    var ny = FInt.Create(reader.ReadInt16());
                    var nz = FInt.Create(reader.ReadInt16());
                    var pad = FInt.Create(reader.ReadInt16());
                    var normal = new Vector3
                    {
                        X = (float)nx.ToDouble(),
                        Y = (float)nx.ToDouble(),
                        Z = (float)nx.ToDouble()
                    };
                    normals[n] = normal;
                }

                var hasNormals = false;
                var hasColors = false;
                var hasUvs = false;

                var groupedTriangles = new Dictionary<int, List<Triangle>>();
                //var missingTriangles = new List<MissingTriangle>();

                reader.BaseStream.Seek(objBlock.PrimitiveTop, SeekOrigin.Begin);
                for (var p = 0; p < objBlock.NPrimitive; p++)
                {
                    var res = ReadPrimitive(reader, groupedTriangles, vertices, normals);
                    if (res.HasValue && !res.Value)
                    {
                        break;
                    }
                }

                foreach (var key in groupedTriangles.Keys)
                {
                    var triangles = groupedTriangles[key];
                    if (triangles.Count > 0)
                    {
                        var model = new ModelEntity
                        {
                            Triangles = triangles.ToArray(),
                            RelativeAddresses = flags == 0,
                            HasNormals = hasNormals,
                            HasColors = hasColors,
                            HasUvs = hasUvs,
                            TexturePage = key,
                            Visible = true
                        };
                        models.Add(model);
                    }
                }
            }

            EndModel:
            if (models.Count > 0)
            {
                var entity = new RootEntity
                {
                    ChildEntities = (EntityBase[])models.ToArray()
                };
                entity.ComputeBounds();
                return entity;
            }
            return null;
        }

        private TMDPacketStructure CreatePolygonPacketStructure(byte flag, byte mode)
        {
            var option = (mode & 0x1F);
            var flagMode = option | (((short)flag) << 8);

            var lgtBit = ((flag >> 0) & 0x01) == 0;
            var bckfceBit = ((flag >> 1) & 0x01) == 1;
            var grdBit = ((flag >> 2) & 0x01) == 1;
            var unk0Bit = ((flag >> 3) & 0x01) == 1;
            var unk1Bit = ((flag >> 4) & 0x01) == 0;
            var unk2Bit = ((flag >> 5) & 0x01) == 0;

            var tgeBit = ((option >> 0) & 0x01) == 0;
            var abeBit = ((option >> 1) & 0x01) == 1;
            var tmeBit = ((option >> 2) & 0x01) == 1;
            var isqBit = ((option >> 3) & 0x01) == 1;
            var iipBit = ((option >> 4) & 0x01) == 1;
            var code = ((mode >> 5) & 0x03);

            var hasLight = lgtBit;
            var isFlat = !iipBit;
            var isGouraud = iipBit;
            var hasTexture = tmeBit;
            var hasGradation = grdBit;
            var isSolid = !grdBit;
            var hasColors = (!hasTexture && hasLight) || (!hasLight && !hasTexture);

            var descr = $"{(isFlat ? "Flat" : "Gouraud")}, {(hasTexture ? "Texture" : "No-Texture")} {((!hasColors ? "" : (isSolid ? "(solid)" : "(gradation)")))}";
            var sects = new List<string>();

            var packet = new List<TMDPacketStructureColumn>();
            var numVerts = isqBit ? 4 : 3;
            if (hasTexture)
            {
                for (var i = 0; i < numVerts; ++i)
                {
                    packet.Add($"S{i}-1"); packet.Add($"T{i}-1");
                    if (i == 0) { packet.Add($"CBA-2"); }
                    else if (i == 1) { packet.Add($"TSB-2"); }
                    else { packet.Add($"_-1"); packet.Add($"_-1"); }
                }
            }
            if (hasColors)
            {
                for (var i = 0; i < numVerts; ++i)
                {
                    if (hasGradation || (isSolid && i == 0))
                    {
                        packet.Add($"R{i}-1"); packet.Add($"G{i}-1"); packet.Add($"B{i}-1");
                        if (i == 0)
                        {
                            packet.Add($"MODE-1");
                        }
                        else
                        {
                            packet.Add($"_-1");
                        }
                    }
                }
            }
            for (var i = 0; i < numVerts; ++i)
            {
                if (hasLight)
                {
                    if ((isFlat && i == 0) || isGouraud) { packet.Add($"NORM{i}-2"); }
                }
                packet.Add($"VERT{i}-2");
            }

            var modBytes = packet.Sum(x => x.Length) % 4;
            if (modBytes != 0)
            {
                var padding = 4 - modBytes;
                packet.Add($"_-{padding}");
            }
            var primitiveType = $"{numVerts} Vertex Polygon with {(hasLight ? "Light" : "No Light")} Source Calculation";

            return new TMDPacketStructure
            {
                Primitive = primitiveType,
                Classification = descr.Trim(),
                Structure = packet
            };
        }
        
        private Dictionary<string, object> ExtractColumnsFromReader(BinaryReader reader, TMDPacketStructure packet, byte mode, Vector3[] vertices, Vector3[] normals)
        {
            var columns = new Dictionary<string, object>();
            var pad = 0;
            for (var i = 0; i < packet.Structure.Count; ++i)
            {
                var col = packet.Structure[i];
                var key = col.IsPadding ? $"PAD{pad++}" : col.Name;
                var val = (col.Length == 2) ? (object)reader.ReadUInt16() : (object)reader.ReadByte();
                columns[key] = val;
                if (col.IsVertex)
                {
                    if ((ushort)val >= vertices.Length)
                    {
                        return null;
                    }
                }
                if (col.IsNormal)
                {
                    if ((ushort)val >= normals.Length)
                    {
                        return null;
                    }
                }
            }
            if (columns.ContainsKey("MODE") && (byte)columns["MODE"] != mode)
            {
                return null;
            }
            return columns;
        }

        private bool? ReadPrimitive(BinaryReader reader, Dictionary<int, List<Triangle>> groupedTriangles, Vector3[] vertices, Vector3[] normals)
        {
            var olen = reader.ReadByte();
            var ilen = reader.ReadByte();
            var flag = reader.ReadByte();
            var mode = reader.ReadByte();
            var offset = reader.BaseStream.Position;
            var code = ((mode >> 5) & 0x03);

            var hasPrimitive = false;

            if(code == 1)
            {
                var pakStruc = CreatePolygonPacketStructure(flag, mode);
                var columns = ExtractColumnsFromReader(reader, pakStruc, mode, vertices, normals);
                if (columns != null)
                {
                    AddTrianglesToGroup(groupedTriangles, columns, vertices, normals);
                }
                hasPrimitive = true;
            }

            reader.BaseStream.Seek(offset + (ilen * 4), SeekOrigin.Begin);

            return hasPrimitive;
        }

        private void AddTriangle(Dictionary<int, List<Triangle>> groupedTriangles, Triangle triangle, int p)
        {
            List<Triangle> triangles;
            if (groupedTriangles.ContainsKey(p))
            {
                triangles = groupedTriangles[p];
            }
            else
            {
                triangles = new List<Triangle>();
                groupedTriangles.Add(p, triangles);
            }
            triangles.Add(triangle);
        }

        private void AddTrianglesToGroup(Dictionary<int, List<Triangle>> group, Dictionary<string, object> columns, Vector3[] vertices, Vector3[] normals)
        {
            var colors = new List<Color>();
            var texcoords = new List<Vector3>();
            var norms = new List<Vector3>();
            var verts = new List<Vector3>();
            for (var i = 0; i < 4; ++i)
            {
                if (columns.ContainsKey($"S{i}"))
                {
                    var s = ((float)(byte)columns[$"S{i}"]) / 256.0f;
                    var t = ((float)(byte)columns[$"T{i}"]) / 256.0f;
                    texcoords.Add(new Vector3 { X = s, Y = t });
                }
                if (columns.ContainsKey($"R{i}"))
                {
                    var r = ((float)(byte)columns[$"R{i}"]) / 256.0f;
                    var g = ((float)(byte)columns[$"G{i}"]) / 256.0f;
                    var b = ((float)(byte)columns[$"B{i}"]) / 256.0f;
                    colors.Add(new Color { R = r, G = g, B = b });
                }
                if (columns.ContainsKey($"NORM{i}"))
                {
                    var nidx = (ushort)columns[$"NORM{i}"];
                    var norm = normals[nidx];
                    norms.Add(new Vector3 { X = norm.X, Y = norm.Y, Z = norm.Z });
                }
                if (columns.ContainsKey($"VERT{i}"))
                {
                    var nidx = (ushort)columns[$"VERT{i}"];
                    var vert = vertices[nidx];
                    verts.Add(new Vector3 { X = vert.X, Y = vert.Y, Z = vert.Z });
                }
            }

            var tPage = 0;
            if (columns.ContainsKey("TSB"))
            {
                tPage = (ushort)columns["TSB"] & 0x1F;
            }

            var defaultColor = new Color { R = 1, G = 1, B = 1 };
            var defaultTexcoord = new Vector3 { X = 0, Y = 0 };
            var defaultNormal = new Vector3 { X = 0, Y = 0, Z = 0 };
            var triColors = new[] { defaultColor, defaultColor, defaultColor };
            var triNormals = new[] { defaultNormal, defaultNormal, defaultNormal };
            var triVerts = new[] { verts[0 % verts.Count], verts[1 % verts.Count], verts[2 % verts.Count] };
            var triTexcoords = new[] { defaultTexcoord, defaultTexcoord, defaultTexcoord };
            if (colors.Count > 0)
            {
                triColors = new[] { colors[0 % colors.Count], colors[1 % colors.Count], colors[2 % colors.Count] };
            }
            if (norms.Count > 0)
            {
                triNormals = new[] { norms[0 % norms.Count], norms[1 % norms.Count], norms[2 % norms.Count] };
            }
            if (texcoords.Count > 0)
            {
                triTexcoords = new[] { texcoords[0 % texcoords.Count], texcoords[1 % texcoords.Count], texcoords[2 % texcoords.Count] };
            }
            var triangle = new Triangle
            {
                PrimitiveType = Triangle.PrimitiveTypeEnum.GsUF3,
                Colors = triColors,
                Normals = triNormals,
                Vertices = triVerts,
                Uv = triTexcoords
            };
            AddTriangle(group, triangle, tPage);

            if (verts.Count > 3)
            {
                triColors = new[] { defaultColor, defaultColor, defaultColor };
                triNormals = new[] { defaultNormal, defaultNormal, defaultNormal };
                triVerts = new[] { verts[1 % verts.Count], verts[3 % verts.Count], verts[2 % verts.Count] };
                triTexcoords = new[] { defaultTexcoord, defaultTexcoord, defaultTexcoord };
                if (colors.Count > 0)
                {
                    triColors = new[] { colors[1 % colors.Count], colors[3 % colors.Count], colors[2 % colors.Count] };
                }
                if (norms.Count > 0)
                {
                    triNormals = new[] { norms[1 % norms.Count], norms[3 % norms.Count], norms[2 % norms.Count] };
                }
                if (texcoords.Count > 0)
                {
                    triTexcoords = new[] { texcoords[1 % texcoords.Count], texcoords[3 % texcoords.Count], texcoords[2 % texcoords.Count] };
                }
                triangle = new Triangle
                {
                    PrimitiveType = Triangle.PrimitiveTypeEnum.GsUF3,
                    Colors = triColors,
                    Normals = triNormals,
                    Vertices = triVerts,
                    Uv = triTexcoords
                };
                AddTriangle(group, triangle, tPage);
            }
        }
    }

    internal class ObjBlock
    {
        public uint VertTop;
        public uint NVert;
        public uint NormalTop;
        public uint NNormal;
        public uint PrimitiveTop;
        public uint NPrimitive;
        public int Scale;
    }
}