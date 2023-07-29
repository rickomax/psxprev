using System.Globalization;
using System.IO;
using OpenTK;

namespace PSXPrev.Common.Exporters
{
    public class glTF2Exporter
    {
        private StreamWriter _writer;
        private BinaryWriter _binaryWriter;

        private PNGExporter _pngExporter;
        private ModelPreparerExporter _modelPreparer;
        private string _baseName;
        private ExportModelOptions _options;
        private string _selectedPath;

        private const int ComponentTypeFloat = 5126;
        private const int PrimitiveMode_Triangles = 4;
        private const int Target_ArrayBuffer = 34962;

        private const string AssetTemplate = " \"asset\" : {\r\n  \"generator\" : \"PSXPREV\",\r\n  \"version\" : \"2.0\"\r\n },";

        public void Export(RootEntity[] entities, string selectedPath, ExportModelOptions options = null)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            _options.Validate();

            _pngExporter = new PNGExporter();
            _modelPreparer = new ModelPreparerExporter(_options);
            _selectedPath = selectedPath;

            // Prepare the shared state for all models being exported (mainly setting up tiled textures).
            _modelPreparer.PrepareAll(entities);

            if (!_options.MergeEntities)
            {
                for (var i = 0; i < entities.Length; i++)
                {
                    ExportEntities(i, entities[i]);
                }
            }

            _pngExporter = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, params RootEntity[] entities)
        {
            _baseName = $"obj{index}";

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                var entity = entities[entityIndex];
                _writer = new StreamWriter($"{_selectedPath}\\{_baseName}_{entityIndex}.gltf");
                _writer.WriteLine("{");

                var models = _modelPreparer.GetModels(entity);

                // Binary buffer creation
                var binaryBufferShortFilename = $"{_baseName}_{entityIndex}.bin";
                var binaryBufferFilename = $"{_selectedPath}\\{binaryBufferShortFilename}";
                _binaryWriter = new BinaryWriter(File.OpenWrite(binaryBufferFilename));

                //Write Asset
                _writer.WriteLine(AssetTemplate);

                //Write Scenes
                _writer.WriteLine("\"scene\": 0,");
                _writer.WriteLine(" \"scenes\": [");
                _writer.WriteLine("  {");
                _writer.WriteLine("  \"nodes\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    _writer.WriteLine(i == models.Count - 1 ? $"{i}" : $"{i},");
                }
                _writer.WriteLine("  ]");
                _writer.WriteLine(" }");
                _writer.WriteLine("],");

                // Write Buffer Views
                var initialOffset = _binaryWriter.BaseStream.Position;
                var offset = initialOffset;
                _writer.WriteLine("\"bufferViews\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    WriteBufferViews(model, ref offset, initialOffset, i == models.Count - 1);
                }
                _writer.WriteLine("],");

                // Write Accessors
                _writer.WriteLine("\"accessors\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var bufferViewIndex = i * 4;
                    var vertexCount = model.Triangles.Length * 3;
                    WriteAccessor(bufferViewIndex + 0, 0, ComponentTypeFloat, vertexCount, "VEC3", false, model.Bounds3D); // Vertex positions
                    WriteAccessor(bufferViewIndex + 1, 0, ComponentTypeFloat, vertexCount, "VEC3"); // Vertex colors
                    WriteAccessor(bufferViewIndex + 2, 0, ComponentTypeFloat, vertexCount, "VEC3"); // Vertex normals
                    WriteAccessor(bufferViewIndex + 3, 0, ComponentTypeFloat, vertexCount, "VEC2", i == models.Count - 1); // Vertex uvs
                }
                _writer.WriteLine("],");

                //Write Meshes
                _writer.WriteLine("\"meshes\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    var bufferViewIndex = i * 4;
                    _writer.WriteLine("{");
                    _writer.WriteLine($" \"name\": \"{model.EntityName}\",");
                    _writer.WriteLine(" \"primitives\": [");
                    _writer.WriteLine("  {");
                    _writer.WriteLine($"   \"attributes\": {{");
                    _writer.WriteLine($"    \"POSITION\": {bufferViewIndex + 0},");
                    _writer.WriteLine($"    \"COLOR_0\": {bufferViewIndex + 1},");
                    _writer.WriteLine($"    \"NORMAL\": {bufferViewIndex + 2},");
                    _writer.WriteLine($"    \"TEXCOORD_0\": {bufferViewIndex + 3}");
                    _writer.WriteLine("   },");
                    //_writer.WriteLine("   \"material\": 0,"); //todo: material
                    _writer.WriteLine($"   \"mode\": {PrimitiveMode_Triangles}");
                    _writer.WriteLine("  }");
                    _writer.WriteLine(" ]");
                    _writer.WriteLine(i == models.Count - 1 ? "}" : "},");
                }
                _writer.WriteLine("],");

                //Write Nodes
                _writer.WriteLine("\"nodes\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    _writer.WriteLine("{");
                    _writer.WriteLine($" \"mesh\": {i},");
                    _writer.WriteLine($" \"name\": \"{model.EntityName}\",");
                    _writer.WriteLine(" \"translation\": [");
                    _writer.WriteLine($"  {F(model.PositionX)},");
                    _writer.WriteLine($"  {F(-model.PositionY)},");
                    _writer.WriteLine($"  {F(-model.PositionZ)}");
                    _writer.WriteLine(" ],");
                    _writer.WriteLine(" \"scale\": [");
                    _writer.WriteLine($"  {F(model.ScaleX)},");
                    _writer.WriteLine($"  {F(model.ScaleY)},");
                    _writer.WriteLine($"  {F(model.ScaleZ)}");
                    _writer.WriteLine(" ]");
                    _writer.WriteLine(i == models.Count - 1 ? "}" : "},");
                }
                _writer.WriteLine("],");

                //Write Buffer
                _writer.WriteLine("\"buffers\": [");
                _writer.WriteLine(" {");
                _writer.WriteLine($"  \"uri\": \"{binaryBufferShortFilename}\",");
                _writer.WriteLine($"  \"byteLength\": {_binaryWriter.BaseStream.Length}");
                _writer.WriteLine(" }");
                _writer.WriteLine("]");

                _binaryWriter.Dispose();
                _binaryWriter = null;

                _writer.WriteLine("}");
                _writer.Dispose();
                _writer = null;
            }
        }

        //todo: are bounds inverted bc of y-z negation?
        private void WriteAccessor(int bufferView, int byteOffset, int componentType, int count, string type, bool final = false, BoundingBox boundingBox = null)
        {
            _writer.WriteLine("{");
            _writer.WriteLine($" \"bufferView\": {bufferView},");
            _writer.WriteLine($" \"byteOffset\": {byteOffset},");
            _writer.WriteLine($" \"componentType\": {componentType},");
            _writer.WriteLine($" \"count\": {count},");
            _writer.WriteLine($" \"type\": \"{type}\"");
            if (boundingBox != null)
            {
                _writer.WriteLine(",");
                _writer.WriteLine(" \"min\": [");
                _writer.WriteLine($"  {F(boundingBox.Min.X)},");
                _writer.WriteLine($"  {F(boundingBox.Min.Y)},");
                _writer.WriteLine($"  {F(boundingBox.Min.Z)}");
                _writer.WriteLine(" ],");
                _writer.WriteLine(" \"max\": [");
                _writer.WriteLine($"  {F(boundingBox.Max.X)},");
                _writer.WriteLine($"  {F(boundingBox.Max.Y)},");
                _writer.WriteLine($"  {F(boundingBox.Max.Z)}");
                _writer.WriteLine(" ]");
            }
            _writer.WriteLine(final ? "}" : "},");
        }

        private void WriteBufferViews(ModelEntity model, ref long offset, long initialOffset, bool final)
        {
            // Write vertex positions
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"target\": {Target_ArrayBuffer},");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryVector3(triangle.Vertices[j]);
                }
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;

            // Write vertex colors
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"target\": {Target_ArrayBuffer},");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryColor(triangle.Colors[j]);
                }
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;

            // Write vertex normals
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"target\": {Target_ArrayBuffer},");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryVector3(triangle.Normals[j]);
                }
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;

            // Write vertex UVs
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"target\": {Target_ArrayBuffer},");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryUV(triangle.Uv[j]);
                }
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine(final ? "}" : "},");
        }

        private void WriteBinaryColor(Color color)
        {
            _binaryWriter.Write(color.R);
            _binaryWriter.Write(color.G);
            _binaryWriter.Write(color.B);
        }

        private void WriteBinaryVector3(Vector3 vector)
        {
            _binaryWriter.Write(vector.X);
            _binaryWriter.Write(-vector.Y);
            _binaryWriter.Write(-vector.Z);
        }

        private void WriteBinaryUV(Vector2 vector)
        {
            _binaryWriter.Write(vector.X);
            _binaryWriter.Write(1f - vector.Y);
        }

        private static string F(float value)
        {
            return value.ToString(GeomMath.FloatFormat, CultureInfo.InvariantCulture);
        }
    }
}