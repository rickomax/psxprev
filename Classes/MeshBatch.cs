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
                        BindMesh(modelEntity, modelEntity.TempMatrix * modelEntity.WorldMatrix, textureBinder, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
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
                    BindMesh(modelEntity, modelEntity.TempMatrix * modelEntity.WorldMatrix, textureBinder, updateMeshData, modelEntity.InitialVertices, modelEntity.InitialNormals, modelEntity.FinalVertices, modelEntity.FinalNormals, modelEntity.Interpolator);
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
                const int elementCount = numTriangles * 3 * 3;
                var baseIndex = 0;
                var positionList = new float[elementCount];
                var normalList = new float[elementCount];
                var colorList = new float[elementCount];
                var uvList = new float[elementCount];
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
                        var index1 = baseIndex++;
                        var index2 = baseIndex++;
                        var index3 = baseIndex++;

                        positionList[index1] = vertices[index1];
                        positionList[index2] = vertices[index2];
                        positionList[index3] = vertices[index3];

                        normalList[index1] = 0f;
                        normalList[index2] = 0f;
                        normalList[index3] = 0f;

                        colorList[index1] = color.R;
                        colorList[index2] = color.G;
                        colorList[index3] = color.B;

                        uvList[index1] = 0f;
                        uvList[index2] = 0f;
                        uvList[index3] = 0f;
                    }
                }
                mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList);
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
            //var rootEntity = modelEntity.ParentEntity as RootEntity; //todo
            mesh.WorldMatrix = matrix ?? modelEntity.WorldMatrix;
            if (updateMeshData)
            {
                var numTriangles = modelEntity.Triangles.Length;
                var elementCount = numTriangles * 3 * 3;
                var baseIndex = 0;
                var positionList = new float[elementCount];
                var normalList = new float[elementCount];
                var colorList = new float[elementCount];
                var uvList = new float[elementCount];
                for (var t = 0; t < numTriangles; t++)
                {
                    var lastVertex = Vector3.Zero;
                    var lastNormal = Vector3.Zero;
                    var lastColor = Color.White;
                    var lastUv = Vector3.Zero;
                    var triangle = modelEntity.Triangles[t];
                    for (var i = 0; i < 3; i++)
                    {
                        var index1 = baseIndex++;
                        var index2 = baseIndex++;
                        var index3 = baseIndex++;

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

                        positionList[index1] = vertex.X;
                        positionList[index2] = vertex.Y;
                        positionList[index3] = vertex.Z;

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

                        normalList[index1] = normal.X;
                        normalList[index2] = normal.Y;
                        normalList[index3] = normal.Z;

                        Color color;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            color = lastColor;
                        }
                        else
                        {
                            color = triangle.Colors[i];
                        }
                        colorList[index1] = color.R;
                        colorList[index2] = color.G;
                        colorList[index3] = color.B;

                        Vector3 uv;
                        if (_scene.VibRibbonWireframe && i == 2)
                        {
                            uv = lastUv;
                        }
                        else
                        {
                            uv = triangle.Uv[i];
                        }
                        uvList[index1] = uv.X;
                        uvList[index2] = uv.Y;
                        uvList[index3] = uv.Z;

                        lastVertex = vertex;
                        lastNormal = normal;
                        lastColor = color;
                        lastUv = uv;
                    }
                }
                mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList);
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

        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix, TextureBinder textureBinder = null, bool wireframe = false)
        {
            if (!IsValid)
            {
                return;
            }
            foreach (var mesh in _meshes)
            {
                if (mesh == null)
                {
                    continue;
                }
                var modelMatrix = mesh.WorldMatrix;
                var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
                GL.UniformMatrix4(Scene.UniformIndexMVP, false, ref mvpMatrix);
                mesh.Draw(textureBinder, wireframe);
            }
        }
    }
}