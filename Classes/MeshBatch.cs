using OpenTK;
using OpenTK.Graphics.OpenGL;

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

        public void SetupEntityBatch(RootEntity rootEntity, bool focus = false)
        {
            var models = rootEntity.ChildEntities;
            Reset(models.Length);
            for (var m = 0; m < models.Length; m++)
            {
                var model = (ModelEntity)models[m];
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

        public void BindModelBatch(ModelEntity modelEntity, int index, Matrix4 matrix)
        {
            BindMesh(modelEntity, index, matrix);
        }

        public void BindTestMesh(Matrix4 matrix, int index, bool isSelected)
        {
            const int numTriangles = 12;
            const int elementCount = numTriangles * 3 * 3;
            const float cubeSize = 100f;
            var baseIndex = 0;
            //var indexList = new int[elementCount];
            var positionList = new float[elementCount];
            var normalList = new float[elementCount];
            var colorList = new float[elementCount];
            var uvList = new float[elementCount];
            var vertices = new[]
            {
                -cubeSize, -cubeSize, -cubeSize    ,
                -cubeSize, -cubeSize, cubeSize     ,
                cubeSize, -cubeSize, cubeSize      ,
                -cubeSize, -cubeSize, -cubeSize    ,
                cubeSize, -cubeSize, cubeSize      ,
                cubeSize, -cubeSize, -cubeSize     ,
                -cubeSize, cubeSize, -cubeSize     ,
                cubeSize, cubeSize, -cubeSize      ,
                cubeSize, cubeSize, cubeSize       ,
                -cubeSize, cubeSize, -cubeSize     ,
                cubeSize, cubeSize, cubeSize       ,
                -cubeSize, cubeSize, cubeSize      ,
                cubeSize, -cubeSize, cubeSize      ,
                -cubeSize, -cubeSize, cubeSize     ,
                -cubeSize, cubeSize, cubeSize      ,
                cubeSize, -cubeSize, cubeSize      ,
                -cubeSize, cubeSize, cubeSize      ,
                cubeSize, cubeSize, cubeSize       ,
                -cubeSize, -cubeSize, -cubeSize    ,
                cubeSize, -cubeSize, -cubeSize     ,
                cubeSize, cubeSize, -cubeSize      ,
                -cubeSize, -cubeSize, -cubeSize    ,
                cubeSize, cubeSize, -cubeSize      ,
                -cubeSize, cubeSize, -cubeSize     ,
                cubeSize, -cubeSize, -cubeSize     ,
                cubeSize, -cubeSize, cubeSize      ,
                cubeSize, cubeSize, cubeSize       ,
                cubeSize, -cubeSize, -cubeSize     ,
                cubeSize, cubeSize, cubeSize       ,
                cubeSize, cubeSize, -cubeSize      ,
                -cubeSize, -cubeSize, cubeSize     ,
                -cubeSize, -cubeSize, -cubeSize    ,
                -cubeSize, cubeSize, -cubeSize     ,
                -cubeSize, -cubeSize, cubeSize     ,
                -cubeSize, cubeSize, -cubeSize     ,
                -cubeSize, cubeSize, cubeSize      
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

                    if (isSelected)
                    {
                        colorList[index1] = 1f;
                        colorList[index2] = 0f;
                    }
                    else
                    {
                        colorList[index1] = 0f;
                        colorList[index2] = 1f;
                    }

                    colorList[index3] = 0f;

                    uvList[index1] = 0f;
                    uvList[index2] = 0f;
                    uvList[index3] = 0f;

                    //indexList[index1] = t;
                    //indexList[index2] = t;
                    //indexList[index3] = t;
                }
            }
            var mesh = GetMesh(index);
            mesh.WorldMatrix = matrix;
            mesh.Texture = _scene.TextureBinder.GetTexture(0);
            mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList);
        }

        private void BindMesh(ModelEntity modelEntity, int index, Matrix4? matrix = null)
        {
            var numTriangles = modelEntity.Triangles.Length;
            var elementCount = numTriangles * 3 * 3;
            var baseIndex = 0;
            //var indexList = new int[elementCount];
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

                    //indexList[index1] = t;
                    //indexList[index2] = t;
                    //indexList[index3] = t;
                }
            }
            var mesh = GetMesh(index);
            mesh.WorldMatrix = matrix ?? modelEntity.WorldMatrix;
            mesh.Visible = modelEntity.Visible;
            mesh.SetData(numTriangles * 3, positionList, normalList, colorList, uvList);
            mesh.Texture = modelEntity.Texture != null ? _scene.TextureBinder.GetTexture(modelEntity.TexturePage) : 0;
        }

        //public void BindLine(Vector3 p1, Vector3 p2, int index, Matrix4 worldMatrix)
        //{
        //    var line = LineMesh.GetLine(index);
        //    line.WorldMatrix = worldMatrix;
        //}

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
            _meshes = new Mesh[nMeshes];
            _ids = new uint[nMeshes];
            GL.GenVertexArrays(nMeshes, _ids);
        }

        private Mesh GetMesh(int index)
        {
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh(_ids[index]);
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
                if (mesh == null || !mesh.Visible)
                {
                    continue;
                }

                var modelMatrix = mesh.WorldMatrix;
                var mvpMatrix = modelMatrix * viewMatrix * projectionMatrix;
                GL.UniformMatrix4(Scene.UniformIndexMVP, false, ref mvpMatrix);

                mesh.Draw(textureBinder);
            }
        }

    }
}