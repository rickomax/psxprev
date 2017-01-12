using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PSXPrev.Classes.Entities;
using PSXPrev.Classes.Texture;

namespace PSXPrev.Classes.Batchers
{
    public class MeshBatch
    {
        const float CubeSize = 50f;

        private readonly Scene.Scene _scene;
        private uint[] _ids;
        private Mesh.Mesh[] _meshes;

        public bool IsValid { get; private set; }

        public MeshBatch(Scene.Scene scene)
        {
            _scene = scene;
        }

        public void SetupEntityBatch(RootEntity rootEntity, bool focus = false)
        {
            var models = rootEntity.ChildEntities;
            Reset(models.Count);
            for (var m = 0; m < models.Count; m++)
            {
                var model = models[m];
                BindMesh(model, m);
            }
            if (focus)
            {
                _scene.FocusOnBounds(rootEntity.Bounds3D);
            }
        }

        public void SetupModelBatch(ModelEntity modelEntity, bool focus = false)
        {
            Reset(1);
            BindMesh(modelEntity, 0);
            if (focus)
            {
                _scene.FocusOnBounds(modelEntity.Bounds3D);
            }
        }

        public void BindTestMesh(Matrix4 matrix, int index)
        {
            const int numTriangles = 12;
            const int elementCount = numTriangles * 3 * 3;
            var baseIndex = 0;
            var indexList = new int[elementCount];
            var positionList = new float[elementCount];
            var normalList = new float[elementCount];
            var colorList = new float[elementCount];
            var uvList = new float[elementCount];
            var vertices = new[]
            {
                -CubeSize, -CubeSize, -CubeSize    ,
                -CubeSize, -CubeSize, CubeSize     ,
                CubeSize, -CubeSize, CubeSize      ,
                -CubeSize, -CubeSize, -CubeSize    ,
                CubeSize, -CubeSize, CubeSize      ,
                CubeSize, -CubeSize, -CubeSize     ,
                -CubeSize, CubeSize, -CubeSize     ,
                CubeSize, CubeSize, -CubeSize      ,
                CubeSize, CubeSize, CubeSize       ,
                -CubeSize, CubeSize, -CubeSize     ,
                CubeSize, CubeSize, CubeSize       ,
                -CubeSize, CubeSize, CubeSize      ,
                CubeSize, -CubeSize, CubeSize      ,
                -CubeSize, -CubeSize, CubeSize     ,
                -CubeSize, CubeSize, CubeSize      ,
                CubeSize, -CubeSize, CubeSize      ,
                -CubeSize, CubeSize, CubeSize      ,
                CubeSize, CubeSize, CubeSize       ,
                -CubeSize, -CubeSize, -CubeSize    ,
                CubeSize, -CubeSize, -CubeSize     ,
                CubeSize, CubeSize, -CubeSize      ,
                -CubeSize, -CubeSize, -CubeSize    ,
                CubeSize, CubeSize, -CubeSize      ,
                -CubeSize, CubeSize, -CubeSize     ,
                CubeSize, -CubeSize, -CubeSize     ,
                CubeSize, -CubeSize, CubeSize      ,
                CubeSize, CubeSize, CubeSize       ,
                CubeSize, -CubeSize, -CubeSize     ,
                CubeSize, CubeSize, CubeSize       ,
                CubeSize, CubeSize, -CubeSize      ,
                -CubeSize, -CubeSize, CubeSize     ,
                -CubeSize, -CubeSize, -CubeSize    ,
                -CubeSize, CubeSize, -CubeSize     ,
                -CubeSize, -CubeSize, CubeSize     ,
                -CubeSize, CubeSize, -CubeSize     ,
                -CubeSize, CubeSize, CubeSize      
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

                    colorList[index1] = 0f;
                    colorList[index2] = 1f;
                    colorList[index3] = 0f;

                    uvList[index1] = 0f;
                    uvList[index2] = 0f;
                    uvList[index3] = 0f;

                    indexList[index1] = t;
                    indexList[index2] = t;
                    indexList[index3] = t;
                }
            }
            var mesh = GetMesh(index);
            mesh.WorldMatrix = matrix;
            mesh.Texture = _scene.TextureBinder.GetTexture(0);
            mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList, indexList);
        }

        private void BindMesh(ModelEntity modelEntity, int index)
        {
            var numTriangles = modelEntity.Triangles.Count;
            var elementCount = numTriangles * 3 * 3;
            var baseIndex = 0;
            var indexList = new int[elementCount];
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

                    indexList[index1] = t;
                    indexList[index2] = t;
                    indexList[index3] = t;
                }
            }
            var mesh = GetMesh(index);
            mesh.WorldMatrix = modelEntity.WorldMatrix;
            mesh.Visible = modelEntity.Visible;
            mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList, indexList);
            mesh.Texture = modelEntity.Texture != null ? _scene.TextureBinder.GetTexture(modelEntity.TexturePage) : 0;
        }

        public void Reset(int nMeshes)
        {
            IsValid = true;
            if (_meshes != null)
            {
                foreach (var mesh in _meshes)
                {
                    if (mesh == null)
                    {
                        continue;
                    }
                    mesh.Delete();
                }
            }
            _meshes = new Mesh.Mesh[nMeshes];
            _ids = new uint[nMeshes];
            GL.GenVertexArrays(nMeshes, _ids);
        }

        private Mesh.Mesh GetMesh(int index)
        {
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh.Mesh(_ids[index]);
            }
            return _meshes[index];
        }

        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix, TextureBinder textureBinder)
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
                
                if (!mesh.Visible)
                {
                    continue;
                }

                var modelMatrix = mesh.WorldMatrix;
                var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
                GL.UniformMatrix4(Scene.Scene.UniformIndexMvp, false, ref mvpMatrix);

                mesh.Draw(textureBinder);
            }
        }
    }
}