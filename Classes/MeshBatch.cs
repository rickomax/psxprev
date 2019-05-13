using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace PSXPrev
{
    public class MeshBatch
    {
        private readonly Scene _scene;
        private uint[] _ids;
        private Mesh[] _meshes;

        public bool IsValid { get; private set; }

        public MeshBatch(Scene scene)
        {
            _scene = scene;
        }

        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, ModelEntity selectedModel = null, RootEntity selectedEntity = null, TextureBinder textureBinder = null, bool updateMeshData = true, bool focus = false)
        {
            var bounds = focus ? new BoundingBox() : null;
            var modelCount = checkedEntities != null || selectedEntity != null || selectedModel != null ? 1 : 0;
            if (checkedEntities != null)
            {
                foreach (var entity in checkedEntities)
                {
                    if (entity == selectedEntity)
                    {
                        continue;
                    }
                    modelCount += entity.ChildEntities.Length;
                }
            }
            if (selectedEntity != null)
            {
                foreach (var subEntity in selectedEntity.ChildEntities)
                {
                    modelCount++;
                    if (focus)
                    {
                        bounds.AddPoints(subEntity.Bounds3D.Corners);
                    }
                }
            }
            if (updateMeshData)
            {
                Reset(modelCount);
            }
            var modelIndex = 0;
            if (checkedEntities != null)
            {
                foreach (var entity in checkedEntities)
                {
                    if (entity == selectedEntity)
                    {
                        continue;
                    }
                    foreach (var subEntity in entity.ChildEntities)
                    {
                        if (subEntity == selectedModel)
                        {
                            continue;
                        }
                        BindMesh((ModelEntity)subEntity, modelIndex++, null, textureBinder, updateMeshData);
                    }
                }
            }
            if (selectedEntity != null)
            {
                foreach (var subEntity in selectedEntity.ChildEntities)
                {
                    if (subEntity == selectedModel)
                    {
                        continue;
                    }
                    BindMesh((ModelEntity)subEntity, modelIndex++, null, textureBinder, updateMeshData);
                }
            }
            if (selectedModel != null)
            {
                BindMesh(selectedModel, modelIndex, null, textureBinder, updateMeshData);
            }
            if (focus)
            {
                _scene.FocusOnBounds(bounds);
            }
        }

        public void BindModelBatch(ModelEntity modelEntity, int index, Matrix4 matrix, TextureBinder textureBinder = null)
        {
            BindMesh(modelEntity, index, matrix, textureBinder);
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

        private void BindMesh(ModelEntity modelEntity, int index, Matrix4? matrix = null, TextureBinder textureBinder = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(index);
            if (mesh == null)
            {
                return;
            }
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
                    var triangle = modelEntity.Triangles[t];
                    for (var i = 0; i < 3; i++)
                    {
                        var index1 = baseIndex++;
                        var index2 = baseIndex++;
                        var index3 = baseIndex++;

                        var vertex = triangle.Vertices[i];
                        positionList[index1] = vertex.X;
                        positionList[index2] = vertex.Y;
                        positionList[index3] = vertex.Z;

                        var normal = triangle.Normals[i];
                        normalList[index1] = normal.X;
                        normalList[index2] = normal.Y;
                        normalList[index3] = normal.Z;

                        var color = triangle.Colors[i];
                        colorList[index1] = color.R;
                        colorList[index2] = color.G;
                        colorList[index3] = color.B;

                        var uv = triangle.Uv[i];
                        uvList[index1] = uv.X;
                        uvList[index2] = uv.Y;
                        uvList[index3] = uv.Z;
                    }
                }
                mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList);
            }
            mesh.WorldMatrix = matrix ?? modelEntity.WorldMatrix;
            if (textureBinder != null)
            {
                mesh.Texture = modelEntity.Texture != null ? textureBinder.GetTexture(modelEntity.TexturePage) : 0;
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
            GL.GenVertexArrays(nMeshes, _ids);
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