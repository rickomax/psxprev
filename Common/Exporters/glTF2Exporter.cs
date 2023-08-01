using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using OpenTK;
using PSXPrev.Common.Animator;

namespace PSXPrev.Common.Exporters
{
    // todo: Model is upside down as simply changing the vertices Y and Z as we do in the viewer is screwing up the results. Figuring out what is going on here
    public class glTF2Exporter
    {
        private StreamWriter _writer;
        private BinaryWriter _binaryWriter;

        private PNGExporter _pngExporter;
        private ModelPreparerExporter _modelPreparer;
        private string _baseName;
        private string _baseTextureName;
        private ExportModelOptions _options;
        private string _selectedPath;

        private const int ComponentType_Float = 5126;
        private const int MagFilter_Linear = 9728;
        private const int WrapMode_Repeat = 10497;
        private const int PrimitiveMode_Triangles = 4;
        private const int Target_ArrayBuffer = 34962;

        private const string AssetTemplate = " \"asset\" : {\r\n  \"generator\" : \"PSXPREV\",\r\n  \"version\" : \"2.0\"\r\n },";

        public void Export(RootEntity[] entities, Animation[] animations, AnimationBatch animationBatch, string selectedPath, ExportModelOptions options = null)
        {
            _options = options?.Clone() ?? new ExportModelOptions();
            // Force any required options for this format here, before calling Validate.
            _options.MergeEntities = false;
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
                    ExportEntities(i, animations, animationBatch, entities[i]);
                }
            }

            _pngExporter = null;
            _modelPreparer.Dispose();
            _modelPreparer = null;
        }

        private void ExportEntities(int index, Animation[] animations, AnimationBatch animationBatch, params RootEntity[] entities)
        {
            var exportAnimations = _options.ExportAnimations && animations?.Length > 0;

            _baseName = $"gltf{index}";
            _baseTextureName = (_options.ShareTextures ? "gltfshared" : _baseName) + "_";

            // Prepare the state for the current model being exported.
            _modelPreparer.PrepareCurrent(entities);

            for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                var entity = _modelPreparer.GetPreparedRootEntity(entities[entityIndex], out var models);

                _writer = new StreamWriter($"{_selectedPath}/{_baseName}_{entityIndex}.gltf");
                _writer.WriteLine("{");

                // Binary buffer creation
                var binaryBufferShortFilename = $"{_baseName}_{entityIndex}.bin";
                var binaryBufferFilename = $"{_selectedPath}/{binaryBufferShortFilename}";
                _binaryWriter = new BinaryWriter(File.OpenWrite(binaryBufferFilename));

                // Write Asset
                _writer.WriteLine(AssetTemplate);

                // Write Scenes
                {
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
                }

                // Write Buffer Views
                {
                    var initialOffset = _binaryWriter.BaseStream.Position;
                    var offset = initialOffset;
                    _writer.WriteLine("\"bufferViews\": [");
                    {
                        // Meshes
                        for (var i = 0; i < models.Count; i++)
                        {
                            var model = models[i];
                            WriteMeshBufferViews(model, ref offset, initialOffset, !exportAnimations && i == models.Count - 1);
                        }
                    }
                    // Animations
                    if (exportAnimations)
                    {
                        for (var i = 0; i < animations.Length; i++)
                        {
                            var animation = animations[i];
                            var totalTime = animation.FrameCount / animation.FPS;
                            var timeStep = 1f / animation.FPS;
                            WriteAnimationTimeBufferView(totalTime, timeStep, ref offset, initialOffset);

                            // Compute animation frames
                            var totalFrames = (int)Math.Ceiling(totalTime / timeStep) + 1; // +1 to include last frame
                            var translations = new Vector3[models.Count, totalFrames];
                            var rotations = new Quaternion[models.Count, totalFrames];
                            var scales = new Vector3[models.Count, totalFrames];
                            var frame = 0;
                            var oldLoopMode = animationBatch.LoopMode;
                            animationBatch.SetupAnimationBatch(animation, simulate: true);
                            animationBatch.LoopMode = AnimationLoopMode.Once;
                            for (var t = 0f; t < totalTime; t += timeStep, frame++)
                            {
                                animationBatch.Time = t;
                                if (animationBatch.SetupAnimationFrame(null, entity, null, simulate: true))
                                {
                                    for (var j = 0; j < models.Count; j++)
                                    {
                                        var model = models[j];
                                        var matrix = model.TempMatrix * model.TempLocalMatrix;
                                        translations[j, frame] = matrix.ExtractTranslation();
                                        rotations[j, frame] = matrix.ExtractRotationSafe();
                                        scales[j, frame] = matrix.ExtractScale();
                                    }
                                }
                                else
                                {
                                    totalFrames = frame + 1;
                                }
                            }
                            animationBatch.LoopMode = oldLoopMode;

                            // Write animation frames for each model
                            for (var j = 0; j < models.Count; j++)
                            {
                                var model = models[j];
                                WriteAnimationDataBufferViews(model, j, translations, rotations, scales, totalFrames, ref offset, initialOffset, j == models.Count - 1 && i == animations.Length - 1);
                            }
                        }
                    }
                    _writer.WriteLine("],");
                }

                var modelImages = new OrderedDictionary();

                // Write Textures
                if (_options.ExportTextures)
                {
                    // Sampler
                    _writer.WriteLine("\"samplers\": [");
                    _writer.WriteLine(" {");
                    _writer.WriteLine($"  \"magFilter\": {MagFilter_Linear},");
                    _writer.WriteLine($"  \"minFilter\": {MagFilter_Linear},");
                    _writer.WriteLine($"  \"wrapS\": {WrapMode_Repeat},");
                    _writer.WriteLine($"  \"wrapT\": {WrapMode_Repeat}");
                    _writer.WriteLine(" }");
                    _writer.WriteLine("],");

                    // Images
                    var textureImages = new OrderedDictionary();

                    _writer.WriteLine("\"images\": [");
                    for (var i = 0; i < models.Count; i++)
                    {
                        var model = models[i];
                        if (NeedsTexture(model))
                        {
                            int imageId;
                            if (!textureImages.Contains(model.Texture))
                            {
                                imageId = textureImages.Count;
                                textureImages.Add(model.Texture, imageId);
                                var uri = $"{_baseTextureName}{imageId}";
                                _writer.WriteLine(modelImages.Count > 0 ? ", {" : "{");
                                _pngExporter.Export(model.Texture, uri, _selectedPath);
                                _writer.WriteLine($" \"uri\": \"{uri}.png\"");
                                _writer.WriteLine("}");
                            }
                            else
                            {
                                imageId = (int)textureImages[model.Texture];
                            }
                            modelImages.Add(model, imageId);
                        }
                    }
                    _writer.WriteLine("],");

                    // Textures
                    _writer.WriteLine("\"textures\": [");
                    for (var i = 0; i < textureImages.Count; i++)
                    {
                        var imageId = (int)textureImages[i];
                        _writer.WriteLine("{");
                        _writer.WriteLine($" \"source\": {imageId},");
                        _writer.WriteLine(" \"sampler\": 0");
                        _writer.WriteLine(i == textureImages.Count - 1 ? "}" : "}, ");
                    }
                    _writer.WriteLine("],");
                }

                // Write Materials
                {
                    _writer.WriteLine("\"materials\": [");
                    for (var i = 0; i < models.Count; i++)
                    {
                        var model = models[i];
                        _writer.WriteLine("{");
                        _writer.WriteLine("\"pbrMetallicRoughness\" : {");
                        if (modelImages.Contains(model))
                        {
                            var imageId = (int)modelImages[model];
                            _writer.WriteLine("\"baseColorTexture\" : {");
                            _writer.WriteLine($"\"index\" : {imageId}");
                            _writer.WriteLine("},");
                        }
                        _writer.WriteLine("\"metallicFactor\" : 0.0,");
                        _writer.WriteLine("\"roughnessFactor\" : 1.0");
                        _writer.WriteLine("}");
                        _writer.WriteLine(i == models.Count - 1 ? "}" : "}, ");
                    }
                    _writer.WriteLine("],");
                }

                // Write Accessors
                {
                    _writer.WriteLine("\"accessors\": [");

                    var bufferViewIndex = 0;
                    // Meshes
                    {
                        for (var i = 0; i < models.Count; i++)
                        {
                            var model = models[i];
                            var vertexCount = model.Triangles.Length * 3;

                            // Compute the local min/max vertex positions. We can't use Bounds3D because that's transformed.
                            var triangles = model.Triangles;
                            var minPos = triangles.Length > 0 ? triangles[0].Vertices[0] : Vector3.Zero;
                            var maxPos = minPos;
                            for (var j = 0; j < triangles.Length; j++)
                            {
                                var triangle = triangles[j];
                                for (var k = 0; k < 3; k++)
                                {
                                    var vertex = triangle.Vertices[k];
                                    minPos = Vector3.ComponentMin(minPos, vertex);
                                    maxPos = Vector3.ComponentMax(maxPos, vertex);
                                }
                            }
                            // Swap min/max and negate for Y and Z to fix handiness.
                            var boundsMin = new[] { minPos.X, -maxPos.Y, -maxPos.Z };
                            var boundsMax = new[] { maxPos.X, -minPos.Y, -minPos.Z };

                            var final = !exportAnimations && i == models.Count - 1;
                            var noTextureFinal = final && !NeedsTexture(model);
                            WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, vertexCount, "VEC3", false, boundsMin, boundsMax); // Vertex positions
                            WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, vertexCount, "VEC3"); // Vertex colors
                            WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, vertexCount, "VEC3", noTextureFinal); // Vertex normals
                            if (NeedsTexture(model))
                            {
                                WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, vertexCount, "VEC2", final); // Vertex uvs
                            }
                        }
                    }
                    // Animations
                    if (exportAnimations)
                    {
                        for (var i = 0; i < animations.Length; i++)
                        {
                            var animation = animations[i];
                            var totalTime = animation.FrameCount / animation.FPS;
                            var timeStep = 1f / animation.FPS;
                            var stepCount = (int)Math.Ceiling(totalTime / timeStep);
                            var timeMin = new [] { 0f };
                            var timeMax = new [] { totalTime };
                            WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, stepCount, "SCALAR", false, timeMin, timeMax); // Frame times
                            for (var j = 0; j < models.Count; j++)
                            {
                                var final = i == animations.Length - 1 && j == models.Count - 1;
                                WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, stepCount, "VEC3"); // Object translation
                                WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, stepCount, "VEC4"); // Object rotation
                                WriteAccessor(bufferViewIndex++, 0, ComponentType_Float, stepCount, "VEC3", final); // Object scale
                            }
                        }
                    }
                    _writer.WriteLine("],");
                }

                var accessorIndex = 0;

                // Write Meshes
                {
                    _writer.WriteLine("\"meshes\": [");
                    for (var i = 0; i < models.Count; i++)
                    {
                        var model = models[i];
                        _writer.WriteLine("{");
                        _writer.WriteLine($" \"name\": \"{model.EntityName}\",");
                        _writer.WriteLine(" \"primitives\": [");
                        _writer.WriteLine("  {");
                        _writer.WriteLine($"   \"attributes\": {{");
                        _writer.WriteLine($"    \"POSITION\": {accessorIndex++},");
                        _writer.WriteLine($"    \"COLOR_0\": {accessorIndex++},");
                        _writer.WriteLine($"    \"NORMAL\": {accessorIndex++}" + (NeedsTexture(model) ? "," : string.Empty));
                        if (NeedsTexture(model))
                        {
                            _writer.WriteLine($"    \"TEXCOORD_0\": {accessorIndex++}");
                        }
                        _writer.WriteLine("   },");
                        _writer.WriteLine($"   \"material\": {i},");
                        _writer.WriteLine($"   \"mode\": {PrimitiveMode_Triangles}");
                        _writer.WriteLine("  }");
                        _writer.WriteLine(" ]");
                        _writer.WriteLine(i == models.Count - 1 ? "}" : "},");
                    }
                    _writer.WriteLine("],");
                }

                // Write Animations 
                if (exportAnimations)
                {
                    _writer.WriteLine("\"animations\": [");
                    for (var i = 0; i < animations.Length; i++)
                    {
                        var animationSamplerIndex = 0;
                        var timeAccessorIndex = accessorIndex++;

                        _writer.WriteLine("{");
                        // Samplers
                        _writer.WriteLine(" \"samplers\" : [");
                        for (var j = 0; j < models.Count; j++)
                        {
                            WriteAnimationSampler(timeAccessorIndex, "LINEAR", accessorIndex++); // object translation
                            WriteAnimationSampler(timeAccessorIndex, "LINEAR", accessorIndex++); // object rotation
                            WriteAnimationSampler(timeAccessorIndex, "LINEAR", accessorIndex++, j == models.Count - 1); // object scale
                        }
                        _writer.WriteLine("],");

                        // Channels
                        _writer.WriteLine(" \"channels\" : [");
                        for (var j = 0; j < models.Count; j++)
                        {
                            WriteAnimationChannel(animationSamplerIndex++, "translation", j); // object translation
                            WriteAnimationChannel(animationSamplerIndex++, "rotation", j); // object rotation
                            WriteAnimationChannel(animationSamplerIndex++, "scale", j, j == models.Count - 1); // object scale
                        }
                        _writer.WriteLine("  ]");
                        _writer.WriteLine(i == animations.Length - 1 ? "}" : "}, ");
                    }
                    _writer.WriteLine("],");
                }

                // Write Nodes
                _writer.WriteLine("\"nodes\": [");
                for (var i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    _writer.WriteLine("{");
                    _writer.WriteLine($" \"mesh\": {i},");
                    _writer.WriteLine($" \"name\": \"{model.EntityName}\",");
                    _writer.WriteLine(" \"translation\": [");
                    WriteVector3(model.WorldMatrix.ExtractTranslation(), true);
                    _writer.WriteLine(" ],");
                    _writer.WriteLine(" \"rotation\": [");
                    WriteQuaternion(model.WorldMatrix.ExtractRotationSafe(), true);
                    _writer.WriteLine(" ],");
                    _writer.WriteLine(" \"scale\": [");
                    WriteVector3(model.WorldMatrix.ExtractScale(), false);
                    _writer.WriteLine(" ]");
                    _writer.WriteLine(i == models.Count - 1 ? "}" : "},");
                }
                _writer.WriteLine("],");

                //Write Buffers
                _writer.WriteLine("\"buffers\": [");
                _writer.WriteLine(" {");
                _writer.WriteLine($"  \"uri\": \"{binaryBufferShortFilename}\",");
                _writer.WriteLine($"  \"byteLength\": {_binaryWriter.BaseStream.Length}");
                _writer.WriteLine(" }");
                _writer.WriteLine("]");

                _writer.WriteLine("}");

                _binaryWriter.Dispose();
                _binaryWriter = null;

                _writer.Dispose();
                _writer = null;
            }
        }
        private void WriteAnimationChannel(int sampler, string path, int node, bool final = false)
        {
            _writer.WriteLine("{");
            _writer.WriteLine($" \"sampler\": {sampler},");
            _writer.WriteLine(" \"target\": {");
            _writer.WriteLine($" \"node\": {node},");
            _writer.WriteLine($" \"path\": \"{path}\"");
            _writer.WriteLine(" }");
            _writer.WriteLine(final ? "}" : "}, ");
        }

        private void WriteAnimationSampler(int input, string interpolation, int output, bool final = false)
        {
            _writer.WriteLine("{");
            _writer.WriteLine($" \"input\": {input},");
            _writer.WriteLine($" \"interpolation\": \"{interpolation}\",");
            _writer.WriteLine($" \"output\": {output}");
            _writer.WriteLine(final ? "}" : "}, ");
        }

        private void WriteAccessor(int bufferView, int byteOffset, int componentType, int count, string type, bool final = false, float[] min = null, float[] max = null)
        {
            _writer.WriteLine("{");
            _writer.WriteLine($" \"bufferView\": {bufferView},");
            _writer.WriteLine($" \"byteOffset\": {byteOffset},");
            _writer.WriteLine($" \"componentType\": {componentType},");
            _writer.WriteLine($" \"count\": {count},");
            _writer.WriteLine($" \"type\": \"{type}\"");
            if (min != null && max != null)
            {
                _writer.WriteLine(",");
                _writer.WriteLine(" \"min\": [");
                for (var i = 0; i < min.Length; i++)
                {
                    _writer.WriteLine($"  {F(min[i])}");
                    if (i < min.Length - 1)
                    {
                        _writer.Write(",");
                    }
                }
                _writer.WriteLine(" ],");
                _writer.WriteLine(" \"max\": [");
                for (var i = 0; i < max.Length; i++)
                {
                    _writer.WriteLine($"  {F(max[i])}");
                    if (i < max.Length - 1)
                    {
                        _writer.Write(",");
                    }
                }
                _writer.WriteLine(" ]");
            }
            _writer.WriteLine(final ? "}" : "},");
        }

        private void WriteAnimationTimeBufferView(float totalTime, float timeStep, ref long offset, long initialOffset)
        {
            // Write time
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            for (var time = 0f; time < totalTime; time += timeStep)
            {
                WriteBinaryFloat(time);
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;
        }

        // todo: we could avoid so many loops by intercalating data
        private void WriteAnimationDataBufferViews(ModelEntity model, int modelIndex, Vector3[,] translations, Quaternion[,] rotations, Vector3[,] scales, int totalFrames, ref long offset, long initialOffset, bool final)
        {
            // Write position
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var translation = translations[modelIndex, frame];
                WriteBinaryVector3(translation, true);
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;

            // Write rotation
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var rotation = rotations[modelIndex, frame];
                WriteBinaryQuaternion(rotation, true);
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine("},");

            offset = _binaryWriter.BaseStream.Position;

            // Write scale
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            for (var frame = 0; frame < totalFrames; frame++)
            {
                var scale = scales[modelIndex, frame];
                WriteBinaryVector3(scale, false);
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine(final ? "}" : "},");

            offset = _binaryWriter.BaseStream.Position;
        }

        private void WriteMeshBufferViews(ModelEntity model, ref long offset, long initialOffset, bool final = false)
        {
            var noTextureFinal = final && !NeedsTexture(model);

            // Write vertex positions
            _writer.WriteLine("{");
            _writer.WriteLine(" \"buffer\": 0,");
            _writer.WriteLine($" \"target\": {Target_ArrayBuffer},");
            _writer.WriteLine($" \"byteOffset\": {_binaryWriter.BaseStream.Position - initialOffset},");
            foreach (var triangle in model.Triangles)
            {
                for (var j = 2; j >= 0; j--)
                {
                    WriteBinaryVector3(triangle.Vertices[j], true);
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
                    WriteBinaryVector3(triangle.Normals[j], true);
                }
            }
            _writer.WriteLine($" \"byteLength\": {_binaryWriter.BaseStream.Position - offset}");
            _writer.WriteLine(noTextureFinal ? "}" : "},");

            offset = _binaryWriter.BaseStream.Position;

            // Write vertex UVs
            if (NeedsTexture(model))
            {
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

            offset = _binaryWriter.BaseStream.Position;
        }

        private void WriteVector3(Vector3 vector, bool fixHandiness = false)
        {
            _writer.WriteLine($"  {F(vector.X)},");
            if (fixHandiness)
            {
                _writer.WriteLine($"  {F(-vector.Y)},");
                _writer.WriteLine($"  {F(-vector.Z)}");
            }
            else
            {
                _writer.WriteLine($"  {F(vector.Y)},");
                _writer.WriteLine($"  {F(vector.Z)}");
            }
        }

        private void WriteQuaternion(Quaternion quaternion, bool fixHandiness = false)
        {
            _writer.WriteLine($"  {F(quaternion.X)},");
            if (fixHandiness)
            {
                _writer.WriteLine($"  {F(-quaternion.Y)},");
                _writer.WriteLine($"  {F(-quaternion.Z)},");
            }
            else
            {
                _writer.WriteLine($"  {F(quaternion.Y)},");
                _writer.WriteLine($"  {F(quaternion.Z)},");
            }
            _writer.WriteLine($"  {F(quaternion.W)}");
        }

        private void WriteBinaryColor(Color color)
        {
            _binaryWriter.Write(color.R);
            _binaryWriter.Write(color.G);
            _binaryWriter.Write(color.B);
        }

        private void WriteBinaryFloat(float value)
        {
            _binaryWriter.Write(value);
        }

        private void WriteBinaryVector3(Vector3 vector, bool fixHandiness = false)
        {
            _binaryWriter.Write(vector.X);
            if (fixHandiness)
            {
                _binaryWriter.Write(-vector.Y);
                _binaryWriter.Write(-vector.Z);
            }
            else
            {
                _binaryWriter.Write(vector.Y);
                _binaryWriter.Write(vector.Z);
            }
        }

        private void WriteBinaryQuaternion(Quaternion quaternion, bool fixHandiness = false)
        {
            _binaryWriter.Write(quaternion.X);
            if (fixHandiness)
            {
                _binaryWriter.Write(-quaternion.Y);
                _binaryWriter.Write(-quaternion.Z);
            }
            else
            {
                _binaryWriter.Write(quaternion.Y);
                _binaryWriter.Write(quaternion.Z);
            }
            _binaryWriter.Write(quaternion.W);
        }

        private void WriteBinaryUV(Vector2 vector)
        {
            _binaryWriter.Write(vector.X);
            _binaryWriter.Write(vector.Y);
        }

        private bool NeedsTexture(ModelEntity model)
        {
            return _options.ExportTextures && model.HasTexture;
        }

        private static string F(float value)
        {
            return value.ToString(GeomMath.FloatFormat, CultureInfo.InvariantCulture);
        }
    }
}