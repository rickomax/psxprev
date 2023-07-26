using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PSXPrev.Classes
{
    public class PrimitiveData
    {
        // Vertex type used for repeat mesh packets.
        public const PrimitiveDataType MESHVERTEX = PrimitiveDataType.VERTEX0;


        public PrimitiveType PrimitiveType { get; set; }
        public RenderFlags RenderFlags { get; set; }

        public int MeshLength { get; private set; } = 1;

        // We have a separate dictionary for Data at index 0 as an optimization
        // for reading packets that are generally never meshes.
        public Dictionary<PrimitiveDataType, uint> Data { get; private set; } = new Dictionary<PrimitiveDataType, uint>();
        public Dictionary<PrimitiveDataType, uint[]> MeshData { get; private set; } // Excludes index 0, which is in Data

        public int[] PadCount { get; private set; } = new int[1]; // Includes index 0

        private bool UseMeshData => MeshLength > 1;

        public uint this[int index, PrimitiveDataType dataType]
        {
            get
            {
                if (index == 0)
                {
                    return Data[dataType];
                }
                else
                {
                    return MeshData[dataType][index - 1];
                }
            }
            set
            {
                if (index == 0)
                {
                    Data[dataType] = value;
                }
                else
                {
                    MeshData[dataType][index - 1] = value;
                }
            }
        }


        public int GetCount(int index)
        {
            if (index == 0)
            {
                return Data.Count;
            }
            else
            {
                return MeshData.Count;
            }
        }

        public IEnumerable<PrimitiveDataType> GetKeys(int index)
        {
            if (index == 0)
            {
                return Data.Keys;
            }
            else
            {
                return MeshData.Keys;
            }
        }

        public IEnumerable<uint> GetValues(int index)
        {
            if (index == 0)
            {
                return Data.Values;
            }
            else
            {
                return MeshData.Select(kvp => kvp.Value[index - 1]);
            }
        }

        public IEnumerable<KeyValuePair<PrimitiveDataType, uint>> GetData(int index)
        {
            if (index == 0)
            {
                return Data;
            }
            else
            {
                return MeshData.Select(kvp => new KeyValuePair<PrimitiveDataType, uint>(kvp.Key, kvp.Value[index - 1]));
            }
        }


        public void SetMeshLength(int length)
        {
            MeshLength = Math.Max(1, length);
            if (UseMeshData)
            {
                MeshData = new Dictionary<PrimitiveDataType, uint[]>();
                PadCount = new int[MeshLength];
            }
        }


        public bool ContainsKey(int index, PrimitiveDataType dataType)
        {
            if (index == 0)
            {
                return Data.ContainsKey(dataType);
            }
            else
            {
                return MeshData.ContainsKey(dataType);
            }
        }

        public bool TryGetValue(int index, PrimitiveDataType dataType, out uint value)
        {
            if (index == 0)
            {
                return Data.TryGetValue(dataType, out value);
            }
            else
            {
                if (MeshData.TryGetValue(dataType, out var values))
                {
                    value = values[index - 1];
                    return true;
                }
                value = 0;
                return false;
            }
        }

        // DOES NOT support getting VERTEX3 for quads!
        public bool TryGetVertex(int index, int vertex, out uint vertexIndex)
        {
            if (index == 0)
            {
                // Support for getting VERTEX3.
                return Data.TryGetValue(PrimitiveDataType.VERTEX0 + vertex, out vertexIndex);
            }
            else
            {
                vertex += index;
                if (vertex < 3)
                {
                    return Data.TryGetValue(PrimitiveDataType.VERTEX0 + vertex, out vertexIndex);
                }
                else
                {
                    // +1 because MeshData starts at index 1.
                    return TryGetValue(vertex - 3 + 1, PrimitiveData.MESHVERTEX, out vertexIndex);
                }
            }
        }

        public bool TryGetNormal(int index, int normal, out uint normalIndex)
        {
            return TryGetValue(index, PrimitiveDataType.NORMAL0 + normal, out normalIndex);
        }

        public void Add(int index, PrimitiveDataType dataType, uint value)
        {
            if (index == 0)
            {
                Data.Add(dataType, value);
            }
            //else if (index == 1)
            //{
            //    var values = new uint[MeshLength - 1];
            //    MeshData.Add(dataType, values);
            //    values[index - 1] = value;
            //}
            else
            {
                //MeshData[dataType][index - 1] = value;
                if (!MeshData.TryGetValue(dataType, out var values))
                {
                    values = new uint[MeshLength - 1];
                    MeshData.Add(dataType, values);
                }
                values[index - 1] = value;
            }
        }

        public void ReadData(BinaryReader reader, int index, PrimitiveDataType dataType, int? dataLength = null)
        {
            uint value = 0;
            if (dataLength == null)
            {
                switch (PrimitiveData.GetDataLength(dataType))
                {
                    case 1:
                        value = reader.ReadByte();
                        break;
                    case 2:
                        value = reader.ReadUInt16();
                        break;
                    case 4:
                        value = reader.ReadUInt32();
                        break;
                }
            }
            else
            {
                for (var i = 0; i < dataLength.Value; i++)
                {
                    reader.ReadByte();
                }
                value = (uint)dataLength.Value;
            }
            var isPadding = dataType >= PrimitiveDataType.PAD1;
            if (!isPadding) //|| Program.Debug)
            {
                // Unlike MeshData, PadCount includes index 0.
                var key = isPadding ? PrimitiveDataType.PAD1 + PadCount[index]++ : dataType;
                Add(index, key, value);

                // Useful for packet debugging, but too verbose for Program.Debug. Keep commented out!
                //Program.Logger.WriteLine($"ReadData({index}, {dataType}, {value} (0x{value:x}))");
            }
        }

        //public override string ToString()
        //{
        //    return Data.Keys.Aggregate("", (current, key) => current + (key + "-"));
        //}

        public string PrintPrimitiveData()
        {
            var value = new StringBuilder();
            if (UseMeshData)
            {
                value.Append("\t[0]");
            }
            foreach (var kvp in Data)
            {
                value.Append("\t").Append(kvp.Key).Append(":").Append(kvp.Value);
            }
            for (var m = 1; m < MeshLength; m++)
            {
                value.Append("\n\t[").Append(m).Append("]");
                foreach (var kvp in MeshData)
                {
                    value.Append("\t").Append(kvp.Key).Append(":").Append(kvp.Value[m - 1]);
                }
            }
            return value.ToString();
        }


        public static int GetDataLength(PrimitiveDataType dataType)
        {
            switch (dataType)
            {
                case PrimitiveDataType.U0:
                case PrimitiveDataType.U1:
                case PrimitiveDataType.U2:
                case PrimitiveDataType.U3:
                case PrimitiveDataType.V0:
                case PrimitiveDataType.V1:
                case PrimitiveDataType.V2:
                case PrimitiveDataType.V3:
                case PrimitiveDataType.R0:
                case PrimitiveDataType.R1:
                case PrimitiveDataType.R2:
                case PrimitiveDataType.R3:
                case PrimitiveDataType.G0:
                case PrimitiveDataType.G1:
                case PrimitiveDataType.G2:
                case PrimitiveDataType.G3:
                case PrimitiveDataType.B0:
                case PrimitiveDataType.B1:
                case PrimitiveDataType.B2:
                case PrimitiveDataType.B3:
                case PrimitiveDataType.Mode:
                case PrimitiveDataType.PAD1:
                    return 1;
                case PrimitiveDataType.CBA:
                case PrimitiveDataType.TSB:
                case PrimitiveDataType.VERTEX0:
                case PrimitiveDataType.VERTEX1:
                case PrimitiveDataType.VERTEX2:
                case PrimitiveDataType.VERTEX3:
                case PrimitiveDataType.NORMAL0:
                case PrimitiveDataType.NORMAL1:
                case PrimitiveDataType.NORMAL2:
                case PrimitiveDataType.NORMAL3:
                case PrimitiveDataType.PAD2:
                    return 2;
                case PrimitiveDataType.TILE:
                    return 4;
            }
            return 0;
        }
    }
}
