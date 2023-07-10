using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using Collada141;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class MeshBatch
    {
        private const float DiscardValue = 100000000f;

        private readonly Scene _scene;
        private uint[] _ids;
        private Mesh[] _meshes;
        private int _modelIndex;

        private bool IsValid { get; set; }

        public MeshBatch(Scene scene)
        {
            _scene = scene;
        }

        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, ModelEntity selectedModelEntity = null, RootEntity selectedRootEntity = null, TextureBinder textureBinder = null, bool updateMeshData = true, bool focus = false, bool hasAnimation = false)
        {
            if (selectedModelEntity == null && selectedRootEntity == null)
            {
                return;
            }
            var bounds = focus ? new BoundingBox() : null;

            selectedRootEntity = selectedRootEntity ?? selectedModelEntity.GetRootEntity();

            //count the selected entity
            var modelCount = 1;
            //count checked, excecpt, the selected
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
                        bounds.AddPoints(checkedEntity.Bounds3D.Corners);
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
                        bounds.AddPoints(subEntity.Bounds3D.Corners);
                    }
                }
            }
            //reset
            ResetModelIndex();
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
                        BindMesh(modelEntity, modelEntity.TempWorldMatrix, textureBinder, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
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
                    BindMesh(modelEntity, selectedRootEntity.TempMatrix * modelEntity.TempWorldMatrix, textureBinder, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
                }
            }
            //}
            // do focus
            if (focus)
            {
                _scene.FocusOnBounds(bounds);
            }
        }

        public void BindModelBatch(ModelEntity modelEntity, Matrix4 matrix, TextureBinder textureBinder = null, Vector3[] initialVertices = null, Vector3[] initialNormals = null, Vector3[] finalVertices = null, Vector3[] finalNormals = null, float? interpolator = null)
        {
            BindMesh(modelEntity, matrix, textureBinder, finalVertices != null || finalNormals != null, initialVertices, initialNormals, finalVertices, finalNormals, interpolator);
        }

        public void BindCube(Matrix4 matrix, Color color, Vector3 center, Vector3 size, int index, TextureBinder textureBinder = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(index);
            if (mesh == null)
            {
                return;
            }
            if (updateMeshData)
            {
                const int numTriangles = 12;
                const int numElements = numTriangles * 3;
                var baseIndex = 0;
                var positionList = new float[numElements * 3]; // Vector3
                var colorList    = new float[numElements * 3]; // Vector3 (Color)
                var vertices = new[]
                {
                center.X-size.X, center.Y-size.Y, center.Z-size.Z,
                center.X-size.X, center.Y-size.Y, center.Z+size.Z,
                center.X+size.X, center.Y-size.Y, center.Z+size.Z ,
                center.X-size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y-size.Y, center.Z+size.Z ,
                center.X+size.X, center.Y-size.Y, center.Z-size.Z,
                center.X-size.X, center.Y+size.Y, center.Z-size.Z,
                center.X+size.X, center.Y+size.Y, center.Z-size.Z ,
                center.X+size.X, center.Y+size.Y, center.Z+size.Z,
                center.X-size.X, center.Y+size.Y, center.Z-size.Z,
                center.X+size.X, center.Y+size.Y, center.Z+size.Z,
                center.X-size.X, center.Y+size.Y, center.Z+size.Z ,
                center.X+size.X, center.Y-size.Y, center.Z+size.Z ,
                center.X-size.X, center.Y-size.Y, center.Z+size.Z,
                center.X-size.X, center.Y+size.Y, center.Z+size.Z ,
                center.X+size.X, center.Y-size.Y, center.Z+size.Z ,
                center.X-size.X, center.Y+size.Y, center.Z+size.Z ,
                center.X+size.X, center.Y+size.Y, center.Z+size.Z,
                center.X-size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y+size.Y, center.Z-size.Z ,
                center.X-size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y+size.Y, center.Z-size.Z ,
                center.X-size.X, center.Y+size.Y, center.Z-size.Z,
                center.X+size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y-size.Y, center.Z+size.Z ,
                center.X+size.X, center.Y+size.Y, center.Z+size.Z,
                center.X+size.X, center.Y-size.Y, center.Z-size.Z,
                center.X+size.X, center.Y+size.Y, center.Z+size.Z,
                center.X+size.X, center.Y+size.Y, center.Z-size.Z ,
                center.X-size.X, center.Y-size.Y, center.Z+size.Z,
                center.X-size.X, center.Y-size.Y, center.Z-size.Z,
                center.X-size.X, center.Y+size.Y, center.Z-size.Z,
                center.X-size.X, center.Y-size.Y, center.Z+size.Z,
                center.X-size.X, center.Y+size.Y, center.Z-size.Z,
                center.X-size.X, center.Y+size.Y, center.Z+size.Z
            };
                for (var t = 0; t < numTriangles; t++)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var index3d = baseIndex * 3;
                        baseIndex++;

                        positionList[index3d + 0] = vertices[index3d + 0];
                        positionList[index3d + 1] = vertices[index3d + 1];
                        positionList[index3d + 2] = vertices[index3d + 2];

                        // Normals are all 0f (passing null will default to a zeroed list).

                        colorList[index3d + 0] = color.R;
                        colorList[index3d + 1] = color.G;
                        colorList[index3d + 2] = color.B;

                        // UVs are all 0f (passing null will default to a zeroed list).
                    }
                }
                mesh.SetData(numElements, positionList, null, colorList, null);
            }
            mesh.WorldMatrix = matrix;
            if (textureBinder != null)
            {
                mesh.Texture = textureBinder.GetTexture(0);
            }
        }

        private void BindMesh(ModelEntity modelEntity, Matrix4? matrix = null, TextureBinder textureBinder = null, bool updateMeshData = true, Vector3[] initialVertices = null, Vector3[] initialNormals = null, Vector3[] finalVertices = null, Vector3[] finalNormals = null, float? interpolator = null)
        {
            if (!modelEntity.Visible)
            {
                return;
            }
            var mesh = GetMesh(_modelIndex++);
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
                            if (attachedIndex != uint.MaxValue)
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
                mesh.SetData(numElements, positionList, normalList, colorList, uvList, tiledAreaList);
            }
            if (textureBinder != null)
            {
                mesh.Texture = modelEntity.Texture != null ? textureBinder.GetTexture((int)modelEntity.TexturePage) : 0;
            }
        }

        public void Reset(int nMeshes)
        {
            IsValid = true;
            if (_meshes != null)
            {
                foreach (var mesh in _meshes)
                {
                    mesh?.Delete();
                }
            }
            _meshes = new Mesh[nMeshes];
            if (_ids != null) GL.DeleteVertexArrays(_ids.Length, _ids);
            _ids = new uint[nMeshes];
            ResetModelIndex();
            GL.GenVertexArrays(nMeshes, _ids);
        }

        public void ResetModelIndex()
        {
            _modelIndex = 0;
        }

        private Mesh GetMesh(int index)
        {
            if (index >= _meshes.Length)
            {
                return null;
            }
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh(_ids[index]);
            }
            return _meshes[index];
        }

        private void DrawMesh(Mesh mesh, Matrix4 viewMatrix, Matrix4 projectionMatrix, TextureBinder textureBinder, bool wireframe, bool standard)
        {
            if (standard)
            {
                if (!_scene.LightEnabled || mesh.RenderFlags.HasFlag(RenderFlags.Unlit))
                {
                    GL.Uniform1(Scene.UniformRenderMode, 1); // Disable lighting
                }
                else
                {
                    GL.Uniform1(Scene.UniformRenderMode, 0); // Enable lighting
                }
            }
            if (_scene.ForceDoubleSided || mesh.RenderFlags.HasFlag(RenderFlags.DoubleSided))
            {
                GL.Disable(EnableCap.CullFace);
            }
            else
            {
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Front);
            }
            var modelMatrix = mesh.WorldMatrix;
            var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
            GL.UniformMatrix4(Scene.UniformIndexMVP, false, ref mvpMatrix);
            mesh.Draw(textureBinder, wireframe);
        }

        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix, TextureBinder textureBinder = null, bool wireframe = false, bool standard = false)
        {
            if (!IsValid)
            {
                return;
            }

            // Pass 1: Draw opaque meshes.
            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);
            GL.Uniform1(Scene.UniformSemiTransparentMode, 0);
            foreach (var mesh in _meshes)
            {
                if (mesh == null || (_scene.SemiTransparencyEnabled && mesh.RenderFlags.HasFlag(RenderFlags.SemiTransparent)))
                {
                    continue; // Not an opaque mesh
                }
                DrawMesh(mesh, viewMatrix, projectionMatrix, textureBinder, wireframe, standard);
            }

            // Draw semi-transparent meshes.
            if (standard && _scene.SemiTransparencyEnabled)
            {
                // Pass 2: Draw opaque pixels when the stp bit is UNSET.
                GL.Uniform1(Scene.UniformSemiTransparentMode, 1);
                foreach (var mesh in _meshes)
                {
                    if (mesh == null || !mesh.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
                    {
                        continue; // Not a semi-transparent mesh
                    }
                    DrawMesh(mesh, viewMatrix, projectionMatrix, textureBinder, wireframe, standard);
                }

                // Pass 3: Draw semi-transparent pixels when the stp bit is SET.
                GL.DepthMask(false); // Disable so that transparent surfaces can show behind other transparent surfaces.
                GL.Enable(EnableCap.Blend);
                GL.Uniform1(Scene.UniformSemiTransparentMode, 2);
                foreach (var mesh in _meshes)
                {
                    if (mesh == null || !mesh.RenderFlags.HasFlag(RenderFlags.SemiTransparent))
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
                    }
                    DrawMesh(mesh, viewMatrix, projectionMatrix, textureBinder, wireframe, standard);
                }

                // Restore settings.
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
                GL.Uniform1(Scene.UniformSemiTransparentMode, 0);
            }
            // Restore settings.
            GL.Disable(EnableCap.CullFace);
            if (standard)
            {
                GL.Uniform1(Scene.UniformRenderMode, (_scene.LightEnabled ? 0 : 1)); // Restore lighting
            }
        }
    }
}
