using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace PSXPrev.Common.Renderer
{
    public enum SubModelVisibility
    {
        All,
        Selected,
        WithSameTMDID,
    }

    public enum RenderPass
    {
        Pass1Opaque,
        Pass2SemiTransparentOpaquePixels,
        Pass3SemiTransparent,
    }

    // todo
    public struct DrawStatistics
    {
        public int TriangleCount;  // Unique triangle count
        public int TrianglesDrawn; // Drawn triangle count
        public int MeshCount;      // Unique mesh count
        public int MeshBinds;      // Number of times meshes have been bound
        public int MeshesDrawn;    // Number of times meshes have been drawn
        public int JointCount;     // Unique skin joint count
        public int SkinCount;      // Unique skin count
        public int SkinBinds;      // Number of times skins have been bound
        public int TextureBinds;   // Number of times textures have been bound
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
        private uint[] _meshIds;
        private Mesh[] _meshes;
        private Skin[] _skins;

        // Re-usable buffers so that we don't need to constantly allocate arrays that are immediately thrown away.
        private float[] _positionList;
        private float[] _normalList;
        private float[] _normalListEmpty;
        private float[] _colorList;
        private float[] _colorListEmpty;
        private float[] _uvList;
        private float[] _uvListEmpty;
        private float[] _tiledAreaList;
        private float[] _tiledAreaListEmpty;
        private uint[] _jointList;
        private uint[] _jointListEmpty;
        private float[] _jointMatrixList;
        private Matrix4[] _jointMatrices;
        private bool[] _jointsAssigned;

        public bool IsValid { get; private set; }
        public int MeshCount { get; private set; } // Used in-case we have a smaller count than the array sizes
        public int MeshIndex { get; set; } // Manually set this index to handle which mesh to bind
        public int SkinIndex { get; set; }

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
                if (_meshes != null)
                {
                    for (var i = 0; i < MeshCount; i++)
                    {
                        _meshes[i]?.Dispose();
                        _skins[i]?.Dispose();
                    }
                }
                GL.DeleteVertexArrays(MeshCount, _meshIds);
                _meshes = null;
                _skins = null;
                _meshIds = null;
                MeshCount = 0;
                MeshIndex = 0;
            }
        }


        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, ModelEntity selectedModelEntity = null, RootEntity selectedRootEntity = null, bool updateMeshData = true, SubModelVisibility subModelVisibility = SubModelVisibility.All)
        {
            // Allow clearing selection and not drawing anything
            //if (selectedModelEntity == null && selectedRootEntity == null)
            //{
            //    return;
            //}
            selectedRootEntity = selectedRootEntity ?? selectedModelEntity?.GetRootEntity();

            // Count how many models we need to reserve meshes for
            var modelCount = 0;

            // Count checked entities' models, except for the selected entity
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue; // We'll count models for selectedRootEntity later on in the function.
                    }
                    modelCount += checkedEntity.ChildEntities.Length;
                }
            }

            // Count visible selected entity models
            IReadOnlyList<EntityBase> selectedVisibleModels = null;
            if (selectedRootEntity != null)
            {
                if (selectedModelEntity != null && subModelVisibility != SubModelVisibility.All)
                {
                    if (subModelVisibility == SubModelVisibility.Selected)
                    {
                        selectedVisibleModels = new ModelEntity[] { selectedModelEntity };
                    }
                    else if (subModelVisibility == SubModelVisibility.WithSameTMDID)
                    {
                        selectedVisibleModels = selectedRootEntity.GetModelsWithTMDID(selectedModelEntity.TMDID);
                    }
                    else
                    {
                        selectedVisibleModels = new ModelEntity[0];
                    }
                }
                else
                {
                    selectedVisibleModels = selectedRootEntity.ChildEntities;
                }
                modelCount += selectedVisibleModels.Count;
            }

            // Restart or reset the batch
            ResetMeshIndex();
            if (updateMeshData)
            {
                Reset(modelCount);
            }

            // Bind all models to the meshes
            // Bind checked entities' models, except for the selected entity
            if (checkedEntities != null)
            {
                foreach (var entity in checkedEntities)
                {
                    if (entity == selectedRootEntity)
                    {
                        continue; // We'll bind selectedRootEntity later on in the function.
                    }
                    BindRootModelMeshes(entity, null, updateMeshData);
                }
            }

            // Bind visible selected entity models
            if (selectedRootEntity != null)
            {
                BindRootModelMeshes(selectedRootEntity, selectedVisibleModels, updateMeshData);
            }
        }

        private void BindRootModelMeshes(RootEntity rootEntity, IReadOnlyList<EntityBase> models, bool updateMeshData)
        {
            if (models == null)
            {
                models = rootEntity.ChildEntities; // We're not cherry-picking which models to render, use all of them
            }

            var canUseSkin = rootEntity.Joints != null && _scene.AttachJointsMode == AttachJointsMode.Attach && Scene.JointsSupported;
            Skin skin = null;

            // Optimized:
            // Compute joint matrices ahead of time when computing each mesh matrix, and then use to assign to all skins.
            // It's unexpected/uncommon for sprites to be used with joints, so don't bother allocating the list until needed.
            List<Tuple<Skin, RenderFlags, Vector3>> spriteSkins = null; // skin, spriteFlags, spriteCenter
            var jointCount     = canUseSkin ? rootEntity.Joints.Length : 0;
            var jointMatrices  = canUseSkin ? ReuseArray(ref _jointMatrices, jointCount) : null;
            // True argument clears all values to false
            var jointsAssigned = canUseSkin ? ReuseArray(ref _jointsAssigned, jointCount, true) : null;
            var jointsAssignedCount = 0;


            var rootTempMatrix = rootEntity.TempMatrix;

            foreach (ModelEntity model in models)
            {
                var worldMatrix = model.TempWorldMatrix;
                Matrix4.Mult(ref rootTempMatrix, ref worldMatrix, out worldMatrix);
                // Optimized:
                // Assign matrix for joint, since we already have it computed for the mesh
                if (canUseSkin && model.IsJoint)
                {
                    jointMatrices[model.JointID] = worldMatrix;
                    jointsAssigned[model.JointID] = true; // If assignedCount doesn't match, then any false values need a default matrix
                    jointsAssignedCount++; // Track how many joints are assigned, to see if any were missed
                }

                var mesh = BindModelMesh(model, worldMatrix, updateMeshData);

                // Check if we need to assign a skin to the mesh
                if (mesh != null && canUseSkin && model.NeedsJointTransform)
                {
                    // Only assign skin if the model has non-NoJoints
                    if (model.IsSprite)
                    {
                        // Compute a special set of joints that take sprite rotation/center into account.
                        // This is certainly not efficient, but sprites are used infrequently enough,
                        // let alone together with joints.
                        var spriteSkin = GetSkin(SkinIndex++);

                        // Optimized:
                        if (spriteSkins == null)
                        {
                            spriteSkins = new List<Tuple<Skin, RenderFlags, Vector3>>();
                        }
                        spriteSkins.Add(new Tuple<Skin, RenderFlags, Vector3>(spriteSkin, model.RenderFlags, model.SpriteCenter));
                        // Unoptimized:
                        //UpdateJointMatricesData(spriteSkin, rootEntity, model.RenderFlags, model.SpriteCenter);

                        mesh.Skin = spriteSkin;
                    }
                    else
                    {
                        // Compute the standard set of joints, and reuse them for all non-sprite meshes.
                        if (skin == null)
                        {
                            skin = GetSkin(SkinIndex++);
                            // Unoptimized:
                            //UpdateJointMatricesData(skin, rootEntity);
                        }
                        mesh.Skin = skin;
                    }
                }
            }

            // Optimized:
            if (canUseSkin && (skin != null || spriteSkins != null))
            {
                // Assign matrices for any joints that are missing models
                if (jointsAssignedCount < jointCount)
                {
                    var rootTempWorldMatrix = rootEntity.TempWorldMatrix;
                    for (var i = 0; i < jointCount; i++)
                    {
                        if (!jointsAssigned[i])
                        {
                            jointMatrices[i] = rootTempWorldMatrix;
                        }
                    }
                }

                // Now that we have a pre-computed list of joints, use them for all calls to UpdateJointMatricesData.
                if (skin != null)
                {
                    UpdateJointMatricesData(skin, jointMatrices, jointCount);
                }
                if (spriteSkins != null)
                {
                    foreach (var tuple in spriteSkins)
                    {
                        var spriteSkin = tuple.Item1;
                        var spriteFlags = tuple.Item2;
                        var spriteCenter = tuple.Item3;
                        UpdateJointMatricesData(spriteSkin, jointMatrices, jointCount, spriteFlags, spriteCenter);
                    }
                }
            }
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

        public void BindTriangleOutline(ModelEntity model, Triangle triangle, Color color = null, float thickness = 2f, bool updateMeshData = true)
        {
            if (model == null || triangle == null)
            {
                return;
            }
            var lineBuilder = new LineMeshBuilder
            {
                Thickness = thickness,
                SolidColor = color ?? Color.White,
            };
            var vertices = new Vector3[3];
            var normals  = new Vector3[3];
            if (updateMeshData)
            {
                var jointMatrices = _scene.AttachJointsMode == AttachJointsMode.Attach ? model.GetRootEntity().JointMatrices : null;
                var worldMatrix = model.WorldMatrix;
                triangle.TransformPositions(ref worldMatrix, jointMatrices, out vertices[0], out vertices[1], out vertices[2]);
                triangle.TransformNormals(ref worldMatrix, jointMatrices, out normals[0], out normals[1], out normals[2]);

                lineBuilder.AddTriangleOutline(vertices[0], vertices[1], vertices[2]);
            }
            BindLineMesh(lineBuilder, null, updateMeshData);

            // Draw normals for triangle
            lineBuilder.Clear();
            lineBuilder.Thickness = 6f;
            lineBuilder.SolidColor = Color.Red;
            if (updateMeshData)
            {
                var maxLength = 0f;
                var totalLength = 0f;
                for (var j = 0; j < 3; j++)
                {
                    var edgeLength = (vertices[(j + 1) % 3] - vertices[j]).Length;
                    maxLength = Math.Max(maxLength, edgeLength);
                    totalLength += edgeLength;
                }
                var normalLength = (totalLength / 3f) / 2f;

                for (var j = 0; j < 3; j++)
                {
                    var center = vertices[j];
                    var normal = center + normals[j] * normalLength;
                    lineBuilder.AddLine(center, normal);
                }
            }
            BindLineMesh(lineBuilder, null, updateMeshData);
        }


        private Mesh BindModelMesh(ModelEntity model, Matrix4? matrix = null, bool updateMeshData = true)
        {
            if (model.Triangles.Length == 0)
            {
                MeshIndex++; // Still consume the mesh index (unless we change how these are reserved later...)
                return null;
            }

            var mesh = GetMesh(MeshIndex++);
            if (mesh == null)
            {
                return null;
            }

            CopyRenderInfo(mesh, model, ref matrix);

            if (updateMeshData)
            {
                var isLines = _scene.VibRibbonWireframe || mesh.RenderFlags.HasFlag(RenderFlags.Line);
                var uvConverter = model.TextureLookup;
                UpdateTriangleMeshData(mesh, model.Triangles, isLines, uvConverter,
                    model.InitialVertices, model.InitialNormals,
                    model.FinalVertices, model.FinalNormals, model.Interpolator);
            }
            return mesh;
        }

        private void CopyRenderInfo(Mesh mesh, ModelEntity model, ref Matrix4? matrix)
        {
            mesh.CopyFrom(model);
            mesh.WorldMatrix = matrix ?? model.WorldMatrix;
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
                mesh.TextureID = TextureBinder.GetTextureID((int)mesh.TexturePage);
            }
            else
            {
                mesh.TextureID = 0;
            }
        }

        // Used to just update render info if we know we aren't updating mesh data.
        public void BindRenderInfo(MeshRenderInfo renderInfo, Matrix4? matrix = null)
        {
            var mesh = GetMesh(MeshIndex++);//, out var skin, 2);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, renderInfo, ref matrix);
        }

        public void BindTriangleMesh(TriangleMeshBuilder triangleBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);//, out var skin, 2);
            if (mesh == null)
            {
                return;
            }

            CopyRenderInfo(mesh, triangleBuilder, ref matrix);

            if (triangleBuilder.JointMatrices != null && Scene.JointsSupported)
            {
                mesh.Skin = GetSkin(SkinIndex++);
                UpdateJointMatricesData(mesh.Skin, triangleBuilder.JointMatrices, triangleBuilder.JointMatrices.Length,
                                        triangleBuilder.RenderFlags, triangleBuilder.SpriteCenter);
            }
            if (updateMeshData)
            {
                var isLines = mesh.RenderFlags.HasFlag(RenderFlags.Line);
                UpdateTriangleMeshData(mesh, triangleBuilder.Triangles, isLines);
            }
        }

        public void BindLineMesh(LineMeshBuilder lineBuilder, Matrix4? matrix = null, bool updateMeshData = true)
        {
            var mesh = GetMesh(MeshIndex++);//, out var skin, 2);
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
                UpdateLineMeshData(mesh, lineBuilder.Lines);
            }
        }

        private void UpdateLineMeshData(Mesh mesh, IReadOnlyList<Line> lines)
        {
            var elementCount = lines.Count * 2;
            var baseIndex = 0;
            var positionList  = ReuseArray(ref _positionList,  elementCount * 3); // Vector3
            var colorList     = ReuseArray(ref _colorList,     elementCount * 3); // Vector3 (Colorf)
            // Empty all-zero lists
            var normalList    = ReuseArray(ref _normalListEmpty,    elementCount * 3); // Vector3
            var uvList        = ReuseArray(ref _uvListEmpty,        elementCount * 2); // Vector2
            var tiledAreaList = ReuseArray(ref _tiledAreaListEmpty, elementCount * 4); // Vector4
            var jointList     = ReuseArray(ref _jointListEmpty,     elementCount * 2); // uint[2]
            foreach (var line in lines)
            {
                for (var i = 0; i < 2; i++, baseIndex++)
                {
                    var index3d = baseIndex * 3;

                    var vertex = line.Vertices[i];// i == 0 ? line.P1 : line.P2;
                    positionList[index3d + 0] = vertex.X;
                    positionList[index3d + 1] = vertex.Y;
                    positionList[index3d + 2] = vertex.Z;
                    //Mesh.AssignVector3(positionList, baseIndex, ref vertex);

                    // Normals are all 0f.

                    var color = line.Colors[i];// i == 0 ? line.Color1 : line.Color2;
                    colorList[index3d + 0] = color.R;
                    colorList[index3d + 1] = color.G;
                    colorList[index3d + 2] = color.B;
                    //Mesh.AssignColor(colorList, baseIndex, color);

                    // UVs are all 0f.

                    // Tiled areas are all 0f.

                    // Joints are all 0u (no joint, since joints are 1-indexed in the shader).
                }
            }
            mesh.SetData(MeshDataType.Line, elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);
        }

        private void UpdateTriangleMeshData(Mesh mesh, IReadOnlyList<Triangle> triangles, bool isLines, IUVConverter uvConverter = null,
                                            Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                            Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var verticesPerElement = isLines ? 2 : 3;

            var elementCount = triangles.Count * verticesPerElement;
            var baseIndex = 0;
            var positionList  = ReuseArray(ref _positionList,  elementCount * 3); // Vector3
            var normalList    = ReuseArray(ref _normalList,    elementCount * 3); // Vector3
            var colorList     = ReuseArray(ref _colorList,     elementCount * 3); // Vector3 (Colorf)
            var uvList        = ReuseArray(ref _uvList,        elementCount * 2); // Vector2
            var tiledAreaList = ReuseArray(ref _tiledAreaList, elementCount * 4); // Vector4
            var jointList     = ReuseArray(ref _jointList,     elementCount * 2); // uint[2]
            foreach (var triangle in triangles)
            {
                var isTiled = triangle.IsTiled;
                var tiledArea = triangle.TiledUv?.Area ?? Vector4.Zero;
                if (isTiled && uvConverter != null)
                {
                    tiledArea = uvConverter.ConvertTiledArea(tiledArea);
                }
                for (var i = 0; i < verticesPerElement; i++, baseIndex++)
                {
                    var index2d = baseIndex * 2;
                    var index3d = baseIndex * 3;
                    var index4d = baseIndex * 4;

                    var vertex = triangle.Vertices[i];
                    if (_scene.AttachJointsMode == AttachJointsMode.Hide && triangle.VertexJoints != null && triangle.VertexJoints[i] != Triangle.NoJoint)
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
                    //Mesh.AssignVector3(positionList, baseIndex, ref vertex);

                    var normal = triangle.Normals[i];
                    if (initialNormals != null && finalNormals != null && triangle.OriginalNormalIndices[i] < finalNormals.Length)
                    {
                        // todo: Why is 4096f used here?
                        var initialNormal = normal + initialNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        var finalNormal = normal + finalNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        normal = Vector3.Lerp(initialNormal, finalNormal, interpolator);
                    }

                    normalList[index3d + 0] = normal.X;
                    normalList[index3d + 1] = normal.Y;
                    normalList[index3d + 2] = normal.Z;
                    //Mesh.AssignVector3(normalList, baseIndex, ref normal);

                    var color = triangle.Colors[i];
                    colorList[index3d + 0] = color.R;
                    colorList[index3d + 1] = color.G;
                    colorList[index3d + 2] = color.B;
                    //Mesh.AssignColor(colorList, baseIndex, color);

                    // If we're tiled, then the shader needs the base UV, not the converted UV.
                    var uv = triangle.TiledUv?.BaseUv[i] ?? triangle.Uv[i];
                    if (uvConverter != null)
                    {
                        uv = uvConverter.ConvertUV(uv, isTiled);
                    }
                    uvList[index2d + 0] = uv.X;
                    uvList[index2d + 1] = uv.Y;
                    //Mesh.AssignVector2(uvList, baseIndex, ref uv);

                    tiledAreaList[index4d + 0] = tiledArea.X; // U offset
                    tiledAreaList[index4d + 1] = tiledArea.Y; // V offset
                    tiledAreaList[index4d + 2] = tiledArea.Z; // U wrap
                    tiledAreaList[index4d + 3] = tiledArea.W; // V wrap
                    //Mesh.AssignVector4(tiledAreaList, baseIndex, ref tiledArea);

                    // This expects NoJoint to be equal to -1, meaning it will equate to zero after adding 1.
                    // Joints in the shader are 1-indexed, this makes it easier to handle meshes with no joints,
                    // since the default value of zero (for Array.Clear) is what we want as the default value.
                    jointList[index2d + 0] = (triangle.VertexJoints?[i] ?? Triangle.NoJoint) + 1u;
                    jointList[index2d + 1] = (triangle.NormalJoints?[i] ?? Triangle.NoJoint) + 1u;
                    //Mesh.AssignJoint(jointList, baseIndex, triangle.VertexJoints?[i], triangle.NormalJoints?[i]);
                }
            }

            var dataType = isLines ? MeshDataType.Line : MeshDataType.Triangle;
            mesh.SetData(dataType, elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);
        }

        private void UpdateJointMatricesData(Skin skin, RootEntity rootEntity, RenderFlags spriteFlags = RenderFlags.None, Vector3 spriteCenter = default(Vector3))
        {
            //var jointMatrices = rootEntity.AbsoluteAnimatedJointMatrices;
            //UpdateJointMatricesData(skin, jointMatrices, jointMatrices?.Length ?? 0, spriteFlags, spriteCenter);

            /*var rootTempMatrix = rootEntity.TempMatrix;

            var jointCount = rootEntity.Joints?.Length ?? 0;
            var jointMatrices = ReuseArray(ref _jointMatrices, jointCount);

            for (var i = 0; i < jointCount; i++)
            {
                var model = rootEntity.Joints[i];
                if (model == null)
                {
                    jointMatrices[i] = rootEntity.TempWorldMatrix;
                }
                else
                {
                    var modelTempWorldMatrix = model.TempWorldMatrix;
                    Matrix4.Mult(ref rootTempMatrix, ref modelTempWorldMatrix, out jointMatrices[i]);
                }
            }
            UpdateJointMatricesData(skin, jointMatrices, jointCount, spriteFlags, spriteCenter);*/

            var rootTempMatrix = rootEntity.TempMatrix;

            var elementCount = rootEntity.Joints?.Length ?? 0;
            var jointMatrixList = ReuseArray(ref _jointMatrixList, elementCount * 32); // Matrix4[2]
            for (var i = 0; i < elementCount; i++)
            {
                var model = rootEntity.Joints[i];
                Matrix4 worldMatrix;
                if (model == null)
                {
                    worldMatrix = rootEntity.TempWorldMatrix;
                }
                else
                {
                    worldMatrix = model.TempWorldMatrix;
                    Matrix4.Mult(ref rootTempMatrix, ref worldMatrix, out worldMatrix);
                }

                ComputeModelMatrix(ref worldMatrix, out var modelMatrix, spriteFlags, ref spriteCenter);
                ComputeNormalMatrix(ref modelMatrix, out var normalMatrix);

                Skin.AssignModelMatrix(jointMatrixList, i, ref modelMatrix);
                Skin.AssignNormalMatrix(jointMatrixList, i, ref normalMatrix);
            }
            skin.SetData(elementCount, jointMatrixList);
        }

        private void UpdateJointMatricesData(Skin skin, Matrix4[] jointMatrices, int jointCount, RenderFlags spriteFlags = RenderFlags.None, Vector3 spriteCenter = default(Vector3))
        {
            var elementCount = jointCount;
            var jointMatrixList = ReuseArray(ref _jointMatrixList, elementCount * 32); // Matrix4[2]
            for (var i = 0; i < jointCount; i++)
            {
                ComputeModelMatrix(ref jointMatrices[i], out var modelMatrix, spriteFlags, ref spriteCenter);
                ComputeNormalMatrix(ref modelMatrix, out var normalMatrix);

                Skin.AssignModelMatrix(jointMatrixList, i, ref modelMatrix);
                Skin.AssignNormalMatrix(jointMatrixList, i, ref normalMatrix);
            }
            skin.SetData(elementCount, jointMatrixList);
        }


        public void Reset(int meshCount)
        {
            IsValid = true;
            // Dispose of old meshes and skins.
            if (_meshes != null)
            {
                for (var i = 0; i < MeshCount; i++)
                {
                    _meshes[i]?.Dispose();
                    _skins[i]?.Dispose();
                }
            }
            // Create a new meshes array, or reset the existing one to all null if the lengths match.
            // todo: Should we bother resizing to a smaller array size?...
            // Maybe only if the size is drastically smaller...
            if (_meshes == null || _meshes.Length != meshCount)
            {
                _meshes = new Mesh[meshCount];
                _skins  = new Skin[meshCount];
            }
            else
            {
                Array.Clear(_meshes, 0, MeshCount);
                Array.Clear(_skins,  0, MeshCount);
            }
            // Create and setup a new mesh IDs array if the existing one's length doesn't match.
            if (_meshIds == null || _meshIds.Length != meshCount)
            {
                if (_meshIds != null)
                {
                    // Delete old MeshCount
                    GL.DeleteVertexArrays(MeshCount, _meshIds);
                }
                _meshIds = new uint[meshCount];
                GL.GenVertexArrays(meshCount, _meshIds);
            }
            // Assign this in-case we support changing mesh count without changing capacity.
            MeshCount = meshCount;
            ResetMeshIndex();
        }

        public void ResetMeshIndex()
        {
            MeshIndex = 0;
            SkinIndex = 0;
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

        private Mesh NextMesh() => GetMesh(MeshIndex++);

        private Skin NextSkin() => GetSkin(SkinIndex++);

        private Mesh GetMesh(int index)
        {
            if (index >= MeshCount)
            {
                return null;
            }
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh(_meshIds[index]);
            }
            return _meshes[index];
        }

        private Skin GetSkin(int index)
        {
            if (index >= MeshCount)
            {
                return null;
            }
            if (_skins[index] == null)
            {
                _skins[index] = new Skin();
            }
            return _skins[index];
        }

        private static T[] ReuseArray<T>(ref T[] array, int length, bool clear = false)
        {
            if (array == null || array.Length < length)
            {
                return new T[length];
            }
            if (clear)
            {
                Array.Clear(array, 0, length);
            }
            return array;
        }


        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            var triangleCount = 0;
            var meshCount = 0;
            var skinCount = 0;
            Draw(viewMatrix, projectionMatrix, ref triangleCount, ref meshCount, ref skinCount);
        }

        public void Draw(Matrix4 viewMatrix, Matrix4 projectionMatrix, ref int triangleCount, ref int meshCount, ref int skinCount)
        {
            foreach (var pass in _passes)//MeshBatch.GetPasses())
            {
                DrawPass(pass, viewMatrix, projectionMatrix, ref triangleCount, ref meshCount, ref skinCount);
            }
        }

        public void DrawPass(RenderPass renderPass, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            var triangleCount = 0;
            var meshCount = 0;
            var skinCount = 0;
            DrawPass(renderPass, viewMatrix, projectionMatrix, ref triangleCount, ref meshCount, ref skinCount);
        }

        public void DrawPass(RenderPass renderPass, Matrix4 viewMatrix, Matrix4 projectionMatrix, ref int triangleCount, ref int meshCount, ref int skinCount)
        {
            if (!IsValid || !Visible)
            {
                return;
            }
            if (renderPass != RenderPass.Pass1Opaque && !SemiTransparencyEnabled)
            {
                return; // No semi-transparent passes
            }

            GL.UniformMatrix4(Scene.UniformViewMatrix, false, ref viewMatrix);
            GL.UniformMatrix4(Scene.UniformProjectionMatrix, false, ref projectionMatrix);

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
                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
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
                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
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
                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
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
            for (var i = 0; i < MeshCount; i++)
            {
                var mesh = _meshes[i];
                if (mesh != null && mesh.Visible)
                {
                    yield return mesh;
                }
            }
        }

        private int DrawMesh(Mesh mesh, ref Matrix4 viewMatrix, ref Matrix4 projectionMatrix)
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

            var noMissing = mesh.MissingTexture && !_scene.ShowMissingTextures;
            var textureEnabled = TexturesEnabled && !noMissing && mesh.RenderFlags.HasFlag(RenderFlags.Textured);
            if (textureEnabled)
            {
                if (mesh.MissingTexture)
                {
                    GL.Uniform1(Scene.UniformTextureMode, 2); // Missing texture
                }
                else
                {
                    GL.Uniform1(Scene.UniformTextureMode, 0); // Enable texture
                }
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

            var jointsEnabled = mesh.Skin != null && _scene.AttachJointsMode == AttachJointsMode.Attach;
            if (jointsEnabled)
            {
                GL.Uniform1(Scene.UniformJointMode, 0); // Enable joints
            }
            else
            {
                GL.Uniform1(Scene.UniformJointMode, 1); // Disable joints
            }

            /*var modelMatrix = mesh.WorldMatrix;
            if (mesh.RenderFlags.HasFlag(RenderFlags.Sprite) || mesh.RenderFlags.HasFlag(RenderFlags.SpriteNoPitch))
            {
                // Sprites always face the camera
                // THIS MATH IS NOT 100% CORRECT! It's not accurate when we have parent transforms, and I think
                // also local transforms. But it will still correctly face the camera regardless. I think.
                var center = mesh.SpriteCenter;
                Quaternion quaternion;
                if (mesh.RenderFlags.HasFlag(RenderFlags.Sprite))
                {
                    quaternion = _scene.CameraRotation;
                }
                else
                {
                    quaternion = _scene.CameraYawRotation;
                }
                var spriteRotation = Matrix4.CreateFromQuaternion(quaternion);
                // Rotate sprite around its center
                var spriteMatrix = Matrix4.CreateTranslation(-center) * spriteRotation * Matrix4.CreateTranslation(center);
                // Remove rotation applied by world matrix
                //if (mesh.RenderFlags.HasFlag(RenderFlags.Sprite))
                {
                    spriteMatrix *= Matrix4.CreateFromQuaternion(modelMatrix.ExtractRotationSafe().Inverted());
                }
                //else
                //{
                //    // todo: How to remove everything but pitch rotation?
                //    spriteMatrix *= Matrix4.CreateFromQuaternion(modelMatrix.ExtractRotationSafe().Inverted());
                //}
                // Apply transform before world matrix transform
                modelMatrix = spriteMatrix * modelMatrix;
            }*/

            var worldMatrix = mesh.WorldMatrix;
            var spriteCenter = mesh.SpriteCenter;
            ComputeModelMatrix(ref worldMatrix, out var modelMatrix, mesh.RenderFlags, ref spriteCenter);
            ComputeNormalMatrix(ref modelMatrix, out var normalMatrix);

            Matrix4.Mult(ref modelMatrix, ref viewMatrix, out var mvpMatrix);
            Matrix4.Mult(ref mvpMatrix, ref projectionMatrix, out mvpMatrix);

            // Transpose the normal matrix here when setting the uniform (true parameter)
            GL.UniformMatrix3(Scene.UniformNormalMatrix, true, ref normalMatrix);
            GL.UniformMatrix4(Scene.UniformModelMatrix, false, ref modelMatrix);
            GL.UniformMatrix4(Scene.UniformMVPMatrix, false, ref mvpMatrix);

            if (!mesh.TextureAnimation.IsZero())
            {
                // This isn't used at all and is still experimental (requires tiled textures).
                // Currently using % 1.0 is not correct if the tiled texture is not a power of 2.
                var animX = mesh.TextureAnimation.X != 0f ? (float)(_scene.Time * mesh.TextureAnimation.X) % 1f : 0f;
                var animY = mesh.TextureAnimation.Y != 0f ? (float)(_scene.Time * mesh.TextureAnimation.Y) % 1f : 0f;
                GL.Uniform2(Scene.UniformUVOffset, new Vector2(animX, animY));
            }

            var drawCalls = mesh.Draw(TextureBinder, DrawFaces, DrawWireframe, DrawVertices, WireframeSize, VertexSize); //, textureEnabled, jointsEnabled);
            var triangleCount = mesh.PrimitiveCount * drawCalls;

            if (!mesh.TextureAnimation.IsZero())
            {
                GL.Uniform2(Scene.UniformUVOffset, Vector2.Zero);
            }

            return triangleCount;
        }

        private void ComputeModelMatrix(ref Matrix4 worldMatrix, out Matrix4 modelMatrix, RenderFlags spriteFlags, ref Vector3 spriteCenter)
        {
            if (spriteFlags.HasFlag(RenderFlags.Sprite) || spriteFlags.HasFlag(RenderFlags.SpriteNoPitch))
            {
                // Sprites always face the camera
                // THIS MATH IS NOT 100% CORRECT! It's not accurate when we have parent transforms, and I think
                // also local transforms. But it will still correctly face the camera regardless. I think.
                Quaternion quaternion;
                if (spriteFlags.HasFlag(RenderFlags.Sprite))
                {
                    quaternion = _scene.CameraRotation;
                }
                else
                {
                    quaternion = _scene.CameraYawRotation;
                }
                var spriteMatrix = Matrix4.CreateFromQuaternion(quaternion);

                // todo: Should the inverse rotation be applied inside the origin translation, like the spriteMatrix rotation?
                Matrix4 invWorldRotationMatrix;
                //if (renderFlags.HasFlag(RenderFlags.Sprite))
                {
                    invWorldRotationMatrix = Matrix4.CreateFromQuaternion(worldMatrix.ExtractRotationSafe().Inverted());
                }
                //else
                //{
                //    // todo: How to remove everything but pitch rotation?
                //    invWorldRotationMatrix = Matrix4.CreateFromQuaternion(worldMatrix.ExtractRotationSafe().Inverted());
                //}

                // Rotate sprite around its center
                // spriteMatrix = Matrix4.CreateTranslation(-spriteCenter) * spriteMatrix * Matrix4.CreateTranslation(spriteCenter);
                var spriteTranslationMatrix = Matrix4.CreateTranslation(-spriteCenter);
                Matrix4.Mult(ref spriteTranslationMatrix, ref spriteMatrix, out spriteMatrix);
                spriteTranslationMatrix = Matrix4.CreateTranslation(spriteCenter);
                Matrix4.Mult(ref spriteMatrix, ref spriteTranslationMatrix, out spriteMatrix);

                // Remove rotation applied by world matrix
                // spriteMatrix *= invWorldRotationMatrix;
                Matrix4.Mult(ref spriteMatrix, ref invWorldRotationMatrix, out spriteMatrix);

                // Apply transform before world matrix transform
                // modelMatrix = spriteMatrix * worldMatrix;
                Matrix4.Mult(ref spriteMatrix, ref worldMatrix, out modelMatrix);
            }
            else
            {
                modelMatrix = worldMatrix;
            }
        }

        // Does not Transpose, this should be handled when setting the uniform or assigning the joint matrix.
        private static void ComputeNormalMatrix(ref Matrix4 modelMatrix, out Matrix3 normalMatrix)
        {
            // Use Inverted() since it checks determinant to avoid singular matrix exception.
            normalMatrix = new Matrix3(modelMatrix).Inverted();
            // Transpose is handled when assigning the uniform, or assigning the skin matrix.
            //normalMatrix.Transpose();
        }


        public static IEnumerable<RenderPass> GetPasses() => _passes;
    }
}
