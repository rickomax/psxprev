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
        public bool ShowWireframe { get; set; }
        public bool ShowVertices { get; set; }
        public float WireframeSize { get; set; } = 1f;
        public float VertexSize { get; set; } = 1f;
        public bool AmbientEnabled { get; set; }
        public bool LightEnabled { get; set; }
        public bool TexturesEnabled { get; set; }
        public bool SemiTransparencyEnabled { get; set; } = true;
        public bool ForceDoubleSided { get; set; }
        public Color SolidColor { get; set; }

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


        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, ModelEntity selectedModelEntity = null, RootEntity selectedRootEntity = null, bool updateMeshData = true, bool focus = false, bool hasAnimation = false)
        {
            if (selectedModelEntity == null && selectedRootEntity == null)
            {
                return;
            }
            var bounds = focus ? new BoundingBox() : null;

            selectedRootEntity = selectedRootEntity ?? selectedModelEntity.GetRootEntity();

            //count the selected entity
            var modelCount = 1;
            //count checked, except, the selected
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue;
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
                foreach (var subEntity in selectedRootEntity.ChildEntities)
                {
                    modelCount++;
                    if (focus)
                    {
                        bounds.AddBounds(subEntity.Bounds3D);
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
                        continue;
                    }
                    foreach (ModelEntity modelEntity in entity.ChildEntities)
                    {
                        BindModelMesh(modelEntity, modelEntity.TempWorldMatrix, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
                    }
                }
            }
            //if not animating
            //if (!hasAnimation)
            //{
            //root entity
            if (selectedRootEntity != null)
            {
                foreach (ModelEntity modelEntity in selectedRootEntity.ChildEntities)
                {
                    BindModelMesh(modelEntity, selectedRootEntity.TempMatrix * modelEntity.TempWorldMatrix, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
                }
            }
            //}
            // do focus
            if (focus)
            {
                _scene.FocusOnBounds(bounds);
            }
        }

        public void BindModelBatch(ModelEntity modelEntity, Matrix4 matrix, Vector3[] initialVertices = null, Vector3[] initialNormals = null, Vector3[] finalVertices = null, Vector3[] finalNormals = null, float? interpolator = null)
        {
            BindModelMesh(modelEntity, matrix, finalVertices != null || finalNormals != null, initialVertices, initialNormals, finalVertices, finalNormals, interpolator);
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
                lineBuilder.AddEntityBounds(entity);
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
        }


        private void BindModelMesh(ModelEntity modelEntity, Matrix4? matrix = null, bool updateMeshData = true, Vector3[] initialVertices = null, Vector3[] initialNormals = null, Vector3[] finalVertices = null, Vector3[] finalNormals = null, float? interpolator = null)
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

            // Copy render info.
            mesh.RenderFlags = modelEntity.RenderFlags;
            mesh.MixtureRate = modelEntity.MixtureRate;

            //var rootEntity = modelEntity.ParentEntity as RootEntity; //todo
            mesh.WorldMatrix = matrix ?? modelEntity.WorldMatrix;

            if (updateMeshData)
            {
                var numTriangles = modelEntity.Triangles.Length;
                var numElements = numTriangles * 3;
                var baseIndex = 0;
                var positionList  = new float[numElements * 3]; // Vector3
                var normalList    = new float[numElements * 3]; // Vector3
                var colorList     = new float[numElements * 3]; // Vector3 (Color)
                var uvList        = new float[numElements * 2]; // Vector2
                var tiledAreaList = new float[numElements * 4]; // Vector4
                for (var t = 0; t < numTriangles; t++)
                {
                    var lastVertex    = Vector3.Zero;
                    var lastNormal    = Vector3.Zero;
                    var lastColor     = Color.White;
                    var lastUv        = Vector2.Zero;
                    var lastTiledArea = Vector4.Zero;
                    var triangle = modelEntity.Triangles[t];
                    for (var i = 0; i < 3; i++)
                    {
                        var index2d = baseIndex * 2;
                        var index3d = baseIndex * 3;
                        var index4d = baseIndex * 4;
                        baseIndex++;

                        var sourceVertex = triangle.Vertices[i];
                        if (triangle.AttachedIndices != null)
                        {
                            var attachedIndex = triangle.AttachedIndices[i];
                            if (attachedIndex != Triangle.NoAttachment)
                            {
                                if (!_scene.AutoAttach)
                                {
                                    sourceVertex = new Vector3(DiscardValue, DiscardValue, DiscardValue);
                                }
                            }
                        }

                        Vector3 vertex;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            vertex = lastVertex;
                        }
                        else
                        {
                            if (initialVertices != null && finalVertices != null && triangle.OriginalVertexIndices[i] < finalVertices.Length)
                            {
                                var initialVertex = sourceVertex + initialVertices[triangle.OriginalVertexIndices[i]];
                                var finalVertex = sourceVertex + finalVertices[triangle.OriginalVertexIndices[i]];
                                vertex = Vector3.Lerp(initialVertex, finalVertex, interpolator.GetValueOrDefault());
                            }
                            else
                            {
                                vertex = sourceVertex;
                            }
                        }

                        positionList[index3d + 0] = vertex.X;
                        positionList[index3d + 1] = vertex.Y;
                        positionList[index3d + 2] = vertex.Z;

                        Vector3 normal;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            normal = lastNormal;
                        }
                        else
                        {
                            if (initialNormals != null && finalNormals != null && triangle.OriginalNormalIndices[i] < finalNormals.Length)
                            {
                                var initialNormal = triangle.Normals[i] + initialNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                                var finalNormal = triangle.Normals[i] + finalNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                                normal = Vector3.Lerp(initialNormal, finalNormal, interpolator.GetValueOrDefault());
                            }
                            else
                            {
                                normal = triangle.Normals[i];
                            }
                        }

                        normalList[index3d + 0] = normal.X;
                        normalList[index3d + 1] = normal.Y;
                        normalList[index3d + 2] = normal.Z;

                        Color color;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            color = lastColor;
                        }
                        else
                        {
                            color = triangle.Colors[i];
                        }
                        colorList[index3d + 0] = color.R;
                        colorList[index3d + 1] = color.G;
                        colorList[index3d + 2] = color.B;

                        Vector2 uv;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            uv = lastUv;
                        }
                        else
                        {
                            // If we're tiled, then the shader needs the base UV, not the converted UV.
                            uv = triangle.TiledUv?.BaseUv[i] ?? triangle.Uv[i];
                        }
                        uvList[index2d + 0] = uv.X;
                        uvList[index2d + 1] = uv.Y;

                        Vector4 tiledArea;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            tiledArea = lastTiledArea;
                        }
                        else
                        {
                            tiledArea = triangle.TiledUv?.Area ?? Vector4.Zero;
                        }
                        tiledAreaList[index4d + 0] = tiledArea.X; // U offset
                        tiledAreaList[index4d + 1] = tiledArea.Y; // V offset
                        tiledAreaList[index4d + 2] = tiledArea.Z; // U wrap
                        tiledAreaList[index4d + 3] = tiledArea.W; // V wrap


                        lastVertex = vertex;
                        lastNormal = normal;
                        lastColor = color;
                        lastUv = uv;
                        lastTiledArea = tiledArea;
                    }
                }
                mesh.SetData(MeshDataType.Triangle, numElements, positionList, normalList, colorList, uvList, tiledAreaList);
            }

            if (TextureBinder != null && modelEntity.HasTexture)
            {
                mesh.Texture = TextureBinder.GetTexture((int)modelEntity.TexturePage);
            }
            else
            {
                mesh.Texture = 0;
            }
        }

        private void CopyRenderInfo(Mesh mesh, MeshRenderInfo renderInfo, Matrix4? matrix = null)
        {
            mesh.TexturePage = renderInfo.TexturePage; // Debug information only

            mesh.RenderFlags = renderInfo.RenderFlags;
            mesh.MixtureRate = renderInfo.MixtureRate;
            mesh.Alpha = renderInfo.Alpha;
            mesh.Thickness = renderInfo.Thickness;
            mesh.SolidColor = renderInfo.SolidColor;
            mesh.Visible = renderInfo.Visible;
            mesh.WorldMatrix = matrix ?? Matrix4.Identity;

            if (TextureBinder != null && renderInfo.RenderFlags.HasFlag(RenderFlags.Textured))
            {
                mesh.Texture = TextureBinder.GetTexture((int)renderInfo.TexturePage);
            }
            else
            {
                mesh.Texture = 0;
            }
        }

        public void BindRenderInfo(MeshRenderInfo renderInfo, Matrix4? matrix = null)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, renderInfo, matrix);
        }

        public void BindTriangleMesh(TriangleMeshBuilder triangleBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, triangleBuilder, matrix);

            if (updateMeshData)
            {
                var triangles = triangleBuilder.Triangles;
                var numTriangles = triangles.Count;
                var numElements = numTriangles * 3;
                var baseIndex = 0;
                var positionList  = new float[numElements * 3]; // Vector3
                var normalList    = new float[numElements * 3]; // Vector3
                var colorList     = new float[numElements * 3]; // Vector3 (Color)
                var uvList        = new float[numElements * 2]; // Vector2
                var tiledAreaList = new float[numElements * 4]; // Vector4
                for (var t = 0; t < numTriangles; t++)
                {
                    var triangle = triangles[t];
                    for (var i = 0; i < 3; i++)
                    {
                        var index2d = baseIndex * 2;
                        var index3d = baseIndex * 3;
                        var index4d = baseIndex * 4;
                        baseIndex++;

                        var vertex = triangle.Vertices[i];
                        positionList[index3d + 0] = vertex.X;
                        positionList[index3d + 1] = vertex.Y;
                        positionList[index3d + 2] = vertex.Z;

                        var normal = triangle.Normals[i];
                        normalList[index3d + 0] = normal.X;
                        normalList[index3d + 1] = normal.Y;
                        normalList[index3d + 2] = normal.Z;

                        var color = triangle.Colors[i];
                        colorList[index3d + 0] = color.R;
                        colorList[index3d + 1] = color.G;
                        colorList[index3d + 2] = color.B;

                        var uv = triangle.TiledUv?.BaseUv[i] ?? triangle.Uv[i];
                        uvList[index2d + 0] = uv.X;
                        uvList[index2d + 1] = uv.Y;

                        var tiledArea = triangle.TiledUv?.Area ?? Vector4.Zero;
                        tiledAreaList[index4d + 0] = tiledArea.X; // U offset
                        tiledAreaList[index4d + 1] = tiledArea.Y; // V offset
                        tiledAreaList[index4d + 2] = tiledArea.Z; // U wrap
                        tiledAreaList[index4d + 3] = tiledArea.W; // V wrap
                    }
                }
                mesh.SetData(MeshDataType.Triangle, numElements, positionList, normalList, colorList, uvList, tiledAreaList);
            }
        }

        public void BindLineMesh(LineMeshBuilder lineBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, lineBuilder, matrix);
            mesh.RenderFlags |= RenderFlags.Unlit | RenderFlags.DoubleSided; // Enforced flags

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
            if (!IsValid)
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
                GL.Uniform3(Scene.UniformSolidColor, SolidColor.Vector);
            }

            switch (renderPass)
            {
                case RenderPass.Pass1Opaque:
                    // Pass 1: Draw opaque meshes.
                    GL.DepthMask(true);
                    GL.Disable(EnableCap.Blend);
                    GL.Uniform1(Scene.UniformSemiTransparentPass, 0);

                    foreach (var mesh in GetMeshes())
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

                    foreach (var mesh in GetMeshes())
                    {
                        if (!mesh.IsSemiTransparent)
                        {
                            continue; // Not a semi-transparent mesh
                        }
                        if (!mesh.RenderFlags.HasFlag(RenderFlags.Textured))
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

                    foreach (var mesh in GetMeshes())
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

        private IEnumerable<Mesh> GetMeshes()
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
                    GL.Uniform3(Scene.UniformSolidColor, (SolidColor ?? mesh.SolidColor).Vector);
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
            var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
            GL.UniformMatrix4(Scene.UniformModelMatrix, false, ref modelMatrix);
            GL.UniformMatrix4(Scene.UniformMVPMatrix, false, ref mvpMatrix);
            mesh.Draw(TextureBinder, ShowWireframe, ShowVertices, WireframeSize, VertexSize);
        }


        public static IEnumerable<RenderPass> GetPasses() => _passes;
    }
}
