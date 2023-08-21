using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public enum RenderPass
    {
        Pass1Opaque,
        Pass2SemiTransparentOpaquePixels,
        Pass3SemiTransparent,
    }

    public class MeshBatch : IDisposable
    {
        private const float DiscardValue = 100000000f;

        private static readonly RenderPass[] _passes =
        {
            RenderPass.Pass1Opaque,
            RenderPass.Pass2SemiTransparentOpaquePixels,
            RenderPass.Pass3SemiTransparent,
        };


        private readonly Scene _scene;
        private uint[] _ids;
        private Mesh[] _meshes;

        public bool IsValid { get; private set; }
        public int MeshCount { get; private set; } // Used in-case we have a smaller count than the array sizes
        //public int MeshCount => _meshes?.Length ?? 0;
        public int MeshIndex { get; set; } // Manually set this index to handle which mesh to bind

        public TextureBinder TextureBinder { get; set; }
        public bool DrawFaces { get; set; } = true;
        public bool DrawWireframe { get; set; }
        public bool DrawVertices { get; set; }
        private float _wireframeSize = 1f;
        public float WireframeSize
        {
            get => _wireframeSize;
            set => _wireframeSize = Math.Max(0f, value);
        }
        private float _vertexSize = 1f;
        public float VertexSize
        {
            get => _vertexSize;
            set => _vertexSize = Math.Max(0f, value);
        }
        public bool AmbientEnabled { get; set; }
        public bool LightEnabled { get; set; }
        public bool TexturesEnabled { get; set; }
        public bool SemiTransparencyEnabled { get; set; } = true;
        public bool ForceDoubleSided { get; set; }
        public Vector3? LightDirection { get; set; } // Overrides Scene's light direction
        public float? LightIntensity { get; set; } // Overrides Scene's light intensity
        public Color AmbientColor { get; set; } // Overrides Scene's ambient color
        public Color SolidColor { get; set; } // Overrides Mesh's vertex colors
        public bool Visible { get; set; } = true;

        public MeshBatch(Scene scene)
        {
            _scene = scene;
        }

        public void Dispose()
        {
            if (IsValid)
            {
                IsValid = false;
                GL.DeleteVertexArrays(MeshCount, _ids);
                _meshes = null;
                _ids = null;
                MeshCount = 0;
                MeshIndex = 0;
            }
        }


        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, ModelEntity selectedModelEntity = null, RootEntity selectedRootEntity = null, bool updateMeshData = true, bool focus = false, bool tmdidOfSelected = false)
        {
            if (selectedModelEntity == null && selectedRootEntity == null)
            {
                return;
            }
            selectedRootEntity = selectedRootEntity ?? selectedModelEntity.GetRootEntity();

            var bounds = focus ? new BoundingBox() : null;

            //count the selected entity
            var modelCount = 1;
            //count checked, except, the selected
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue; // We'll count models and add bounds for selectedRootEntity later on in the function.
                    }
                    modelCount += checkedEntity.ChildEntities.Length;
                    if (focus)
                    {
                        bounds.AddBounds(checkedEntity.Bounds3D);
                    }
                }
            }
            //focus
            if (selectedRootEntity != null)
            {
                if (selectedModelEntity != null && tmdidOfSelected)
                {
                    var tmdidModels = selectedRootEntity.GetModelsWithTMDID(selectedModelEntity.TMDID);
                    modelCount += tmdidModels.Count;
                    foreach (var modelEntity in tmdidModels)
                    {
                        if (focus && (modelEntity.Triangles.Length > 0 && (!modelEntity.AttachedOnly || modelEntity.IsAttached)))
                        {
                            bounds.AddBounds(modelEntity.Bounds3D);
                        }
                    }
                }
                else
                {
                    modelCount += selectedRootEntity.ChildEntities.Length;
                    if (focus)
                    {
                        bounds.AddBounds(selectedRootEntity.Bounds3D);
                    }
                }
            }
            //reset
            ResetMeshIndex();
            if (updateMeshData)
            {
                Reset(modelCount);
            }
            //bindings

            //checked entities, except selected root
            if (checkedEntities != null)
            {
                foreach (var entity in checkedEntities)
                {
                    if (entity == selectedRootEntity)
                    {
                        continue; // We'll bind selectedRootEntity later on in the function.
                    }
                    foreach (ModelEntity modelEntity in entity.ChildEntities)
                    {
                        BindModelMesh(modelEntity, modelEntity.TempWorldMatrix, updateMeshData,
                            modelEntity.InitialVertices, modelEntity.InitialNormals,
                            modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
                    }
                }
            }
            //if not animating
            //if (!hasAnimation)
            //{
            //root entity
            if (selectedRootEntity != null)
            {
                IEnumerable<EntityBase> models;
                if (selectedModelEntity != null && tmdidOfSelected)
                {
                    models = selectedRootEntity.GetModelsWithTMDID(selectedModelEntity.TMDID);
                }
                else
                {
                    models = selectedRootEntity.ChildEntities;
                }
                foreach (ModelEntity modelEntity in models)
                {
                    BindModelMesh(modelEntity, selectedRootEntity.TempMatrix * modelEntity.TempWorldMatrix, updateMeshData,
                        modelEntity.InitialVertices, modelEntity.InitialNormals,
                        modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
                }
            }
            //}
            // do focus
            if (focus)
            {
                _scene.FocusOnBounds(bounds);
            }
        }

        public void BindModelBatch(ModelEntity modelEntity, Matrix4 matrix, Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                   Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var updateMeshData = finalVertices != null || finalNormals != null;
            BindModelMesh(modelEntity, matrix, updateMeshData,
                initialVertices, initialNormals, finalVertices, finalNormals, interpolator);
        }


        public void BindEntityBounds(EntityBase entity, Color color = null, float thickness = 1f, bool updateMeshData = true)
        {
            if (entity == null)
            {
                return;
            }
            var lineBuilder = new LineMeshBuilder
            {
                Thickness = thickness,
                SolidColor = color ?? Color.White,
            };
            if (updateMeshData)
            {
                lineBuilder.AddBoundsOutline(entity.Bounds3D);
            }
            BindLineMesh(lineBuilder, null, updateMeshData);
        }

        public void BindTriangleOutline(Matrix4 matrix, Triangle triangle, Color color = null, float thickness = 2f, bool updateMeshData = true)
        {
            if (triangle == null)
            {
                return;
            }
            var lineBuilder = new LineMeshBuilder
            {
                Thickness = thickness,
                SolidColor = color ?? Color.White,
            };
            if (updateMeshData)
            {
                lineBuilder.AddTriangleOutline(triangle);
            }
            BindLineMesh(lineBuilder, matrix, updateMeshData);

            // Draw normals for triangle
            var maxLength = 0f;
            var totalLength = 0f;
            for (var j = 0; j < 3; j++)
            {
                var edgeLength = (triangle.Vertices[(j + 1) % 3] - triangle.Vertices[j]).Length;
                maxLength = Math.Max(maxLength, edgeLength);
                totalLength += edgeLength;
            }
            var normalLength = (totalLength / 3) / 2;
            //var normalLength = maxLength / 2;
            lineBuilder.Clear();
            lineBuilder.Thickness = 6f;
            lineBuilder.SolidColor = Color.Red;
            for (var j = 0; j < 3; j++)
            {
                var center = triangle.Vertices[j];
                var normal = center + triangle.Normals[j] * normalLength;
                lineBuilder.AddLine(center, normal);
            }
            BindLineMesh(lineBuilder, matrix, updateMeshData);
        }


        private void BindModelMesh(ModelEntity modelEntity, Matrix4? matrix = null, bool updateMeshData = true,
                                   Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                   Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            if (!modelEntity.Visible)
            {
                return;
            }
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, modelEntity, ref matrix);

            if (updateMeshData)
            {
                var isLines = _scene.VibRibbonWireframe || mesh.RenderFlags.HasFlag(RenderFlags.Line);
                UpdateTriangleMeshData(mesh, modelEntity.Triangles, isLines,
                    initialVertices, initialNormals, finalVertices, finalNormals, interpolator);
            }
        }

        private void CopyRenderInfo(Mesh mesh, ModelEntity modelEntity, ref Matrix4? matrix)
        {
            mesh.CopyFrom(modelEntity);
            mesh.WorldMatrix = matrix ?? modelEntity.WorldMatrix;
            CopyTextureBinderTexture(mesh);
        }

        private void CopyRenderInfo(Mesh mesh, MeshRenderInfo renderInfo, ref Matrix4? matrix)
        {
            mesh.CopyFrom(renderInfo);
            mesh.WorldMatrix = matrix ?? Matrix4.Identity;
            CopyTextureBinderTexture(mesh);
        }

        private void CopyTextureBinderTexture(Mesh mesh)
        {
            if (TextureBinder != null && mesh.IsTextured)
            {
                mesh.Texture = TextureBinder.GetTexture((int)mesh.TexturePage);
            }
            else
            {
                mesh.Texture = 0;
            }
        }

        // Used to just update render info if we know we aren't updating mesh data.
        public void BindRenderInfo(MeshRenderInfo renderInfo, Matrix4? matrix = null)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, renderInfo, ref matrix);
        }

        public void BindTriangleMesh(TriangleMeshBuilder triangleBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, triangleBuilder, ref matrix);

            if (updateMeshData)
            {
                var isLines = mesh.RenderFlags.HasFlag(RenderFlags.Line);
                UpdateTriangleMeshData(mesh, triangleBuilder.Triangles, isLines);
            }
        }

        public void BindLineMesh(LineMeshBuilder lineBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, lineBuilder, ref matrix);
            // Enforced certain flags for lines
            mesh.RenderFlags |= RenderFlags.Unlit | RenderFlags.DoubleSided;
            mesh.RenderFlags &= ~RenderFlags.Textured;

            if (updateMeshData)
            {
                var lines = lineBuilder.Lines;
                var numLines = lines.Count;
                var numElements = numLines * 2;
                var baseIndex = 0;
                var positionList = new float[numElements * 3]; // Vector3
                var colorList    = new float[numElements * 3]; // Vector3 (Color)
                for (var l = 0; l < numLines; l++)
                {
                    var line = lines[l];
                    for (var i = 0; i < 2; i++)
                    {
                        var index3d = baseIndex * 3;
                        baseIndex++;

                        var vertex = line.Vertices[i];// i == 0 ? line.P1 : line.P2;
                        positionList[index3d + 0] = vertex.X;
                        positionList[index3d + 1] = vertex.Y;
                        positionList[index3d + 2] = vertex.Z;

                        // Normals are all 0f (passing null will default to a zeroed list).

                        var color = line.Colors[i];// i == 0 ? line.Color1 : line.Color2;
                        colorList[index3d + 0] = color.R;
                        colorList[index3d + 1] = color.G;
                        colorList[index3d + 2] = color.B;

                        // UVs are all 0f (passing null will default to a zeroed list).
                    }
                }
                mesh.SetData(MeshDataType.Line, numElements, positionList, null, colorList, null);
            }
        }

        private void UpdateTriangleMeshData(Mesh mesh, IReadOnlyList<Triangle> triangles, bool isLines,
                                            Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                            Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var verticesPerElement = isLines ? 2 : 3;

            var numTriangles = triangles.Count;
            var numElements = numTriangles * verticesPerElement;
            var baseIndex = 0;
            var positionList  = new float[numElements * 3]; // Vector3
            var normalList    = new float[numElements * 3]; // Vector3
            var colorList     = new float[numElements * 3]; // Vector3 (Color)
            var uvList        = new float[numElements * 2]; // Vector2
            var tiledAreaList = new float[numElements * 4]; // Vector4
            foreach (var triangle in triangles)
            {
                var tiledArea = triangle.TiledUv?.Area ?? Vector4.Zero;
                for (var i = 0; i < verticesPerElement; i++)
                {
                    var index2d = baseIndex * 2;
                    var index3d = baseIndex * 3;
                    var index4d = baseIndex * 4;
                    baseIndex++;

                    var vertex = triangle.Vertices[i];
                    if (!_scene.AutoAttach && triangle.AttachedIndices != null && triangle.AttachedIndices[i] != Triangle.NoAttachment)
                    {
                        vertex = new Vector3(DiscardValue);
                    }
                    else if (initialVertices != null && finalVertices != null && triangle.OriginalVertexIndices[i] < finalVertices.Length)
                    {
                        var initialVertex = vertex + initialVertices[triangle.OriginalVertexIndices[i]];
                        var finalVertex = vertex + finalVertices[triangle.OriginalVertexIndices[i]];
                        vertex = Vector3.Lerp(initialVertex, finalVertex, interpolator);
                    }

                    positionList[index3d + 0] = vertex.X;
                    positionList[index3d + 1] = vertex.Y;
                    positionList[index3d + 2] = vertex.Z;

                    var normal = triangle.Normals[i];
                    if (initialNormals != null && finalNormals != null && triangle.OriginalNormalIndices[i] < finalNormals.Length)
                    {
                        var initialNormal = normal + initialNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        var finalNormal = normal + finalNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        normal = Vector3.Lerp(initialNormal, finalNormal, interpolator);
                    }

                    normalList[index3d + 0] = normal.X;
                    normalList[index3d + 1] = normal.Y;
                    normalList[index3d + 2] = normal.Z;

                    var color = triangle.Colors[i];
                    colorList[index3d + 0] = color.R;
                    colorList[index3d + 1] = color.G;
                    colorList[index3d + 2] = color.B;

                    // If we're tiled, then the shader needs the base UV, not the converted UV.
                    var uv = triangle.TiledUv?.BaseUv[i] ?? triangle.Uv[i];
                    uvList[index2d + 0] = uv.X;
                    uvList[index2d + 1] = uv.Y;

                    tiledAreaList[index4d + 0] = tiledArea.X; // U offset
                    tiledAreaList[index4d + 1] = tiledArea.Y; // V offset
                    tiledAreaList[index4d + 2] = tiledArea.Z; // U wrap
                    tiledAreaList[index4d + 3] = tiledArea.W; // V wrap
                }
            }

            var dataType = isLines ? MeshDataType.Line : MeshDataType.Triangle;
            mesh.SetData(dataType, numElements, positionList, normalList, colorList, uvList, tiledAreaList);
        }


        public void Reset(int meshCount)
        {
            IsValid = true;
            // Dispose of old meshes.
            if (_meshes != null)
            {
                foreach (var mesh in _meshes)
                {
                    mesh?.Dispose();
                }
            }
            // Create a new meshes array, or reset the existing one to all null if the lengths match.
            // todo: Should we bother resizing to a smaller array size?...
            // Maybe only if the size is drastically smaller...
            if (_meshes == null || _meshes.Length != meshCount)
            {
                _meshes = new Mesh[meshCount];
            }
            else
            {
                for (var i = 0; i < _meshes.Length; i++)
                {
                    _meshes[i] = null;
                }
            }
            // Create and setup a new mesh IDs array if the existing one's length doesn't match.
            if (_ids == null || _ids.Length != meshCount)
            {
                if (_ids != null)
                {
                    // Delete old MeshCount
                    GL.DeleteVertexArrays(MeshCount, _ids);
                }
                _ids = new uint[meshCount];
                GL.GenVertexArrays(meshCount, _ids);
            }
            // Assign this in-case we support changing mesh count without changing capacity.
            MeshCount = meshCount;
            ResetMeshIndex();
        }

        public void ResetMeshIndex()
        {
            MeshIndex = 0;
        }

        public bool IsMeshIndexBound() => IsMeshIndexBound(MeshIndex);

        public bool IsMeshIndexBound(int index)
        {
            if (index >= MeshCount) //_meshes.Length)
            {
                return false;
            }
            return _meshes[index] != null;
        }

        private Mesh GetMesh(int index)
        {
            if (index >= MeshCount) //_meshes.Length)
            {
                return null;
            }
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh(_ids[index]);
            }
            return _meshes[index];
        }


        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            foreach (var pass in _passes)//MeshBatch.GetPasses())
            {
                DrawPass(pass, viewMatrix, projectionMatrix);
            }
        }

        public void DrawPass(RenderPass renderPass, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            if (!IsValid || !Visible)
            {
                return;
            }
            if (renderPass != RenderPass.Pass1Opaque && !SemiTransparencyEnabled)
            {
                return; // No semi-transparent passes
            }

            if (SolidColor != null)
            {
                GL.Uniform1(Scene.UniformColorMode, 1); // Use solid color
                GL.Uniform3(Scene.UniformSolidColor, (Vector3)SolidColor);
            }

            switch (renderPass)
            {
                case RenderPass.Pass1Opaque:
                    // Pass 1: Draw opaque meshes.
                    GL.DepthMask(true);
                    GL.Disable(EnableCap.Blend);
                    GL.Uniform1(Scene.UniformSemiTransparentPass, 0);

                    foreach (var mesh in GetVisibleMeshes())
                    {
                        if (SemiTransparencyEnabled && !mesh.IsOpaque)
                        {
                            continue; // Not an opaque mesh, or semi-transparency is disabled
                        }
                        DrawMesh(mesh, viewMatrix, projectionMatrix);
                    }
                    break;

                case RenderPass.Pass2SemiTransparentOpaquePixels when SemiTransparencyEnabled:
                    // Pass 2: Draw opaque pixels when the stp bit is UNSET.
                    GL.DepthMask(true);
                    GL.Disable(EnableCap.Blend);
                    GL.Uniform1(Scene.UniformSemiTransparentPass, 1);

                    foreach (var mesh in GetVisibleMeshes())
                    {
                        if (!mesh.IsSemiTransparent)
                        {
                            continue; // Not a semi-transparent mesh
                        }
                        if (!mesh.IsTextured)
                        {
                            continue; // Untextured surfaces always have stp bit SET.
                        }
                        DrawMesh(mesh, viewMatrix, projectionMatrix);
                    }
                    break;

                case RenderPass.Pass3SemiTransparent when SemiTransparencyEnabled:
                    // Pass 3: Draw semi-transparent pixels when the stp bit is SET.
                    GL.DepthMask(false); // Disable so that transparent surfaces can show behind other transparent surfaces.
                    GL.Enable(EnableCap.Blend);
                    GL.Uniform1(Scene.UniformSemiTransparentPass, 2);

                    foreach (var mesh in GetVisibleMeshes())
                    {
                        if (!mesh.IsSemiTransparent)
                        {
                            continue; // Not a semi-transparent mesh
                        }
                        switch (mesh.MixtureRate)
                        {
                            case MixtureRate.Back50_Poly50:    //  50% back +  50% poly
                                GL.BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.ConstantColor); // C poly, C back
                                GL.BlendColor(0.50f, 0.50f, 0.50f, 1.0f); // C = 50%
                                GL.BlendEquation(BlendEquationMode.FuncAdd);
                                break;
                            case MixtureRate.Back100_Poly100:  // 100% back + 100% poly
                                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // 100% poly, 100% back
                                GL.BlendEquation(BlendEquationMode.FuncAdd);
                                break;
                            case MixtureRate.Back100_PolyM100: // 100% back - 100% poly
                                GL.BlendFunc(BlendingFactor.One, BlendingFactor.One);    // 100% poly, 100% back
                                GL.BlendEquation(BlendEquationMode.FuncReverseSubtract); // back - poly
                                break;
                            case MixtureRate.Back100_Poly25:   // 100% back +  25% poly
                                GL.BlendFunc(BlendingFactor.ConstantColor, BlendingFactor.One); // C poly, 100% back
                                GL.BlendColor(0.25f, 0.25f, 0.25f, 1.0f); // C = 25%
                                GL.BlendEquation(BlendEquationMode.FuncAdd);
                                break;
                            case MixtureRate.Alpha:            // 1-A% back +   A% poly
                                GL.BlendFunc(BlendingFactor.ConstantAlpha, BlendingFactor.OneMinusConstantAlpha);
                                GL.BlendColor(1.0f, 1.0f, 1.0f, mesh.Alpha); // C = A%
                                GL.BlendEquation(BlendEquationMode.FuncAdd);
                                break;
                        }
                        DrawMesh(mesh, viewMatrix, projectionMatrix);
                    }

                    // Restore settings.
                    GL.DepthMask(true);
                    GL.Disable(EnableCap.Blend);
                    break;
            }
            // Restore settings.
            GL.Disable(EnableCap.CullFace);
        }

        private IEnumerable<Mesh> GetVisibleMeshes()
        {
            foreach (var mesh in _meshes)
            {
                if (mesh != null && mesh.Visible)
                {
                    yield return mesh;
                }
            }
        }

        private void DrawMesh(Mesh mesh, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            var ambient = AmbientEnabled && !mesh.RenderFlags.HasFlag(RenderFlags.NoAmbient);
            var light = LightEnabled && !mesh.RenderFlags.HasFlag(RenderFlags.Unlit);
            if (ambient && light)
            {
                GL.Uniform1(Scene.UniformLightMode, 0); // Enable ambient, enable directional light
            }
            else if (ambient)
            {
                GL.Uniform1(Scene.UniformLightMode, 1); // Enable ambient, disable directional light
            }
            else if (light)
            {
                GL.Uniform1(Scene.UniformLightMode, 2); // Disable ambient, enable directional light
            }
            else
            {
                GL.Uniform1(Scene.UniformLightMode, 3); // Disable ambient, disable directional light
            }

            if (ambient)
            {
                GL.Uniform3(Scene.UniformAmbientColor, (Vector3)(mesh.AmbientColor ?? AmbientColor ?? (Color)_scene.AmbientColor));
            }
            if (light)
            {
                GL.Uniform3(Scene.UniformLightDirection, (mesh.LightDirection ?? LightDirection ?? _scene.LightDirection));
                GL.Uniform1(Scene.UniformLightIntensity, (mesh.LightIntensity ?? LightIntensity ?? _scene.LightIntensity));
            }

            if (SolidColor == null)
            {
                // We only need to assign color mode and solid color if its not handled by this batch.
                if (mesh.SolidColor == null)
                {
                    GL.Uniform1(Scene.UniformColorMode, 0); // Use vertex color
                }
                else
                {
                    GL.Uniform1(Scene.UniformColorMode, 1); // Use solid color
                    GL.Uniform3(Scene.UniformSolidColor, (Vector3)mesh.SolidColor);
                }
            }

            if (TexturesEnabled && mesh.RenderFlags.HasFlag(RenderFlags.Textured))
            {
                GL.Uniform1(Scene.UniformTextureMode, 0); // Enable texture
            }
            else
            {
                GL.Uniform1(Scene.UniformTextureMode, 1); // Disable texture
            }

            if (ForceDoubleSided || mesh.RenderFlags.HasFlag(RenderFlags.DoubleSided))
            {
                GL.Disable(EnableCap.CullFace); // Double-sided
            }
            else
            {
                GL.Enable(EnableCap.CullFace);  // Single-sided
                GL.CullFace(CullFaceMode.Front);
            }

            var modelMatrix = mesh.WorldMatrix;
            if (mesh.RenderFlags.HasFlag(RenderFlags.Sprite))
            {
                // Sprites always face the camera
                // THIS MATH IS NOT 100% CORRECT! It's not accurate when we have parent transforms, and I think
                // also local transforms. But it will still correctly face the camera regardless. I think.
                var center = mesh.SpriteCenter;
                var spriteRotation = Matrix4.CreateFromQuaternion(_scene.CameraRotation);
                // Rotate sprite around its center
                var spriteMatrix = Matrix4.CreateTranslation(-center) * spriteRotation * Matrix4.CreateTranslation(center);
                // Remove rotation applied by world matrix
                spriteMatrix *= Matrix4.CreateFromQuaternion(modelMatrix.ExtractRotationSafe().Inverted());
                // Apply transform before world matrix transform
                modelMatrix = spriteMatrix * modelMatrix;
            }
            // Use Inverted() since it checks determinant to avoid singular matrix exception.
            var normalMatrix = new Matrix3(modelMatrix).Inverted();
            normalMatrix.Transpose();
            var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
            GL.UniformMatrix3(Scene.UniformNormalMatrix, false, ref normalMatrix);
            GL.UniformMatrix4(Scene.UniformModelMatrix, false, ref modelMatrix);
            GL.UniformMatrix4(Scene.UniformMVPMatrix, false, ref mvpMatrix);

            mesh.Draw(TextureBinder, DrawFaces, DrawWireframe, DrawVertices, WireframeSize, VertexSize);
        }


        public static IEnumerable<RenderPass> GetPasses() => _passes;
    }
}
