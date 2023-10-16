using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly RenderPass[] _passes =
        {
            RenderPass.Pass1Opaque,
            RenderPass.Pass2SemiTransparentOpaquePixels,
            RenderPass.Pass3SemiTransparent,
        };

        private static readonly Color3 SkeletonJointColor = new Color3(0.25f, 0.0f, 1.0f);
        private static readonly Color3 SkeletonBoneColor  = new Color3(0.0f, 0.5f, 1.0f);
        private const bool SkeletonBoneUseCylinder = false;


        private readonly Scene _scene;
        private readonly Shader _shader;
        private readonly bool _modelsMeshBatch;
        private readonly bool _sortingEnabled;
        private readonly bool _batchingEnabled;
        private VertexArrayObject[] _vertexArrayObjects;
        private Skin[] _skins;
        private Mesh[] _meshes;
        private Mesh[] _sortedMeshes;
        private int _sortedMeshCount; // Number of visible meshes after sorting and batching

        // Re-usable buffers so that we don't need to constantly allocate arrays that are immediately thrown away.
        private float[] _positionList;
        private float[] _normalList;
        private float[] _normalListEmpty;
        private float[] _colorList;
        //private float[] _colorListEmpty;
        private float[] _uvList;
        private float[] _uvListEmpty;
        private float[] _tiledAreaList;
        private float[] _tiledAreaListEmpty;
        private uint[] _jointList;
        private uint[] _jointListEmpty;
        private float[] _jointMatrixList;

        private Matrix4[] _jointMatrices;
        private bool[] _jointsAssigned;
        private float[] _skeletonMagnitudes;

        private TriangleMeshBuilder _skeletonJointBuilderSphere;
        private TriangleMeshBuilder _skeletonBoneBuilderCylinder;
        private LineMeshBuilder _skeletonBoneBuilderLine;
        private float _skeletonJointRadius = 1f;
        private List<ModelEntity> _skeletonTMDIDModels;

        public bool IsInitialized { get; private set; }
        public int VertexArrayObjectCount { get; private set; }
        public int VertexArrayObjectIndex { get; set; }
        public int SkinCount { get; private set; }
        public int SkinIndex { get; set; }
        public int MeshCount { get; private set; } // Used in-case we have a smaller count than the array sizes
        public int MeshIndex { get; set; } // Manually set this index to handle which mesh to bind

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
        //public bool CalculateNormals { get; set; } // Compute flat normals from triangles, even for unlit faces
        public Vector3? LightDirection { get; set; } // Overrides Scene's light direction
        public float? LightIntensity { get; set; } // Overrides Scene's light intensity
        public Color3? AmbientColor { get; set; } // Overrides Scene's ambient color
        public Color3? SolidColor { get; set; } // Overrides Mesh's vertex colors
        public bool Visible { get; set; } = true;

        public MeshBatch(Scene scene, bool modelsMeshBatch = false, bool sortingEnabled = false, bool batchingEnabled = false)
        {
            _scene = scene;
            _shader = scene.Shader;
            _modelsMeshBatch = modelsMeshBatch;
            _sortingEnabled = sortingEnabled;
            _batchingEnabled = batchingEnabled;
        }

        public void Dispose()
        {
            if (IsInitialized)
            {
                // Dispose of old vertex array objects and skins.
                if (_vertexArrayObjects != null)
                {
                    for (var i = 0; i < VertexArrayObjectCount; i++)
                    {
                        _vertexArrayObjects[i]?.Dispose();
                    }
                }
                if (_skins != null)
                {
                    for (var i = 0; i < SkinCount; i++)
                    {
                        _skins[i]?.Dispose();
                    }
                }
                _vertexArrayObjects = null;
                _skins = null;
                _meshes = null;
                _sortedMeshes = null;
                VertexArrayObjectCount = 0;
                SkinCount = 0;
                MeshCount = 0;
                _sortedMeshCount = 0;
                ResetIndex();
                IsInitialized = false;
            }
        }


        public void SetupMultipleEntityBatch(RootEntity[] checkedEntities = null, RootEntity selectedRootEntity = null, ModelEntity selectedModelEntity = null,
                                             bool updateMeshData = true, SubModelVisibility subModelVisibility = SubModelVisibility.All, bool fastSetup = false)
        {
            selectedRootEntity = selectedRootEntity ?? selectedModelEntity?.GetRootEntity();

            // Count how many models we need to reserve meshes for
            var totalModelCount = 0;
            var entityCount = 0;

            // Count checked entities' models, except for the selected entity
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue; // We'll count models for selectedRootEntity later on in the function.
                    }
                    totalModelCount += checkedEntity.ChildEntities.Length;
                    entityCount++;
                }
            }

            // Count visible selected entity models
            if (selectedRootEntity != null)
            {
                // Even if only a fraction of this entity's models are visible, prepare room for all models
                totalModelCount += selectedRootEntity.ChildEntities.Length;
                entityCount++;
            }

            // Restart or reset the batch
            ResetIndex();
            if (updateMeshData)
            {
                // One VAO per entity, 1 + modelCount skin per entity (main skin + sprite skins)
                Reset(totalModelCount, entityCount, entityCount + totalModelCount);
            }

            // Bind all models to the meshes
            // Bind checked entities' models, except for the selected entity
            if (checkedEntities != null)
            {
                foreach (var checkedEntity in checkedEntities)
                {
                    if (checkedEntity == selectedRootEntity)
                    {
                        continue; // We'll bind selectedRootEntity later on in the function.
                    }
                    BindEntityMeshes(checkedEntity, null, updateMeshData);
                }
            }

            // Bind visible selected entity models
            if (selectedRootEntity != null)
            {
                BindEntityMeshes(selectedRootEntity, selectedModelEntity, updateMeshData, subModelVisibility);
            }

            SortAndBatchMeshes(noSort: fastSetup);
        }

        // Not ready yet
        /*public void UpdateMultipleEntityBatch(RootEntity selectedRootEntity, ModelEntity selectedModelEntity, bool updateMeshData = true, SubModelVisibility subModelVisibility = SubModelVisibility.All, bool fastSetup = false)
        {
            var rootEntity = selectedRootEntity ?? selectedModelEntity?.GetRootEntity();

            BindEntityMeshes(rootEntity, selectedModelEntity, updateMeshData, subModelVisibility);

            SortAndBatchMeshes(noSort: fastSetup);
        }*/

        private void BindEntityMeshes(RootEntity rootEntity, ModelEntity selectedModelEntity, bool updateMeshData, SubModelVisibility subModelVisibility = SubModelVisibility.All)
        {
            var needsBakedJoints = rootEntity.NeedsBakedJoints;
            if (needsBakedJoints && _scene.AttachJointsMode == AttachJointsMode.Attach)
            {
                updateMeshData = true; // Support to make sure that baked joint vertices are always up to date
            }
            var canUseSkin = !needsBakedJoints && rootEntity.Joints != null && _scene.AttachJointsMode == AttachJointsMode.Attach && Shader.JointsSupported;
            Skin mainSkin = null;

            // Optimized:
            // Compute joint matrices ahead of time when computing each mesh matrix, and then use to assign to all skins.
            // It's unexpected/uncommon for sprites to be used with joints, so don't bother allocating the list until needed.
            List<Tuple<ModelEntity, Skin>> spriteSkins = null;
            var jointCount     = canUseSkin ? rootEntity.Joints.Length : 0;
            var jointMatrices  = canUseSkin ? ReuseArray(ref _jointMatrices, jointCount) : null;
            // True argument clears all values to false
            var jointsAssigned = canUseSkin ? ReuseArray(ref _jointsAssigned, jointCount, true) : null;
            var jointsAssignedCount = 0;


            // Use a single vertex array object to hold all vertices for this root entity
            var elementIndex = 0;
            var elementCount = 0;
            if (updateMeshData)
            {
                foreach (ModelEntity model in rootEntity.ChildEntities)
                {
                    // Always reserve 3 vertices per primitive so that in the future,
                    // changing RenderFlags.Line, won't require rebuilding the entire VAO.
                    elementCount += model.Triangles.Length * 3;
                }
                PrepareTriangleMeshDataLists(elementCount);
            }
            // Get the vertex array object used to store vertices for all models in the entity
            var vao = GetVertexArrayObject(VertexArrayObjectIndex++);

            // Always reserve one extra skin index for non-sprite joints
            var mainSkinIndex = SkinIndex++;


            var rootTempMatrix = rootEntity.TempMatrix;
            foreach (ModelEntity model in rootEntity.ChildEntities)
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

                var spriteSkinIndex = SkinIndex++; // Always increment skin index per mesh
                var elementFirst = elementIndex;
                var mesh = BindModelMesh(vao, ref elementIndex, elementFirst, model, ref worldMatrix, updateMeshData);
                if (mesh == null)
                {
                    continue;
                }

                mesh.ResetBatch();
                if (mesh.Visible && selectedModelEntity != null)
                {
                    if (subModelVisibility == SubModelVisibility.Selected)
                    {
                        mesh.Visible = selectedModelEntity == model;
                    }
                    else if (subModelVisibility == SubModelVisibility.WithSameTMDID)
                    {
                        mesh.Visible = selectedModelEntity.TMDID == model.TMDID;
                    }
                }

                // Check if we need to assign a skin to the mesh
                if (canUseSkin && model.NeedsJointTransform)
                {
                    // Only assign skin if the model has non-NoJoints
                    if (model.IsSprite)
                    {
                        // Compute a special set of joints that take sprite rotation/center into account.
                        // This is certainly not efficient, but sprites are used infrequently enough,
                        // let alone together with joints.
                        var spriteSkin = GetSkin(spriteSkinIndex);

                        // Optimized:
                        if (spriteSkins == null)
                        {
                            spriteSkins = new List<Tuple<ModelEntity, Skin>>();
                        }
                        spriteSkins.Add(new Tuple<ModelEntity, Skin>(model, spriteSkin));
                        // Unoptimized:
                        //UpdateJointMatricesData(spriteSkin, rootEntity, model.RenderFlags, model.SpriteCenter);

                        mesh.Skin = spriteSkin;
                    }
                    else
                    {
                        // Compute the standard set of joints, and reuse them for all non-sprite meshes.
                        if (mainSkin == null)
                        {
                            mainSkin = GetSkin(mainSkinIndex);
                            // Unoptimized:
                            //UpdateJointMatricesData(mainSkin, rootEntity);
                        }
                        mesh.Skin = mainSkin;
                    }
                }
                else
                {
                    mesh.Skin = null;
                }
            }

            // Assign all of the root entity's vertices data
            if (updateMeshData)
            {
                vao.SetData(elementCount, _positionList, _normalList, _colorList, _uvList, _tiledAreaList, _jointList);
            }

            // Optimized:
            if (canUseSkin && (mainSkin != null || spriteSkins != null))
            {
                // Assign matrices for any joints that are missing models
                if (jointsAssignedCount < jointCount)
                {
                    var rootTempWorldMatrix = rootEntity.TempWorldMatrix;
                    for (var i = 0; i < jointCount; i++)
                    {
                        if (!jointsAssigned[i])
                        {
                            var jointModel = rootEntity.Joints[i];
                            if (jointModel != null)
                            {
                                // Some joint models weren't included in the models list, add them now
                                var worldMatrix = jointModel.TempWorldMatrix;
                                Matrix4.Mult(ref rootTempMatrix, ref worldMatrix, out jointMatrices[i]);
                            }
                            else
                            {
                                // No model is defined for this joint, use the default transform
                                jointMatrices[i] = rootTempWorldMatrix;
                            }
                        }
                    }
                }

                // Now that we have a pre-computed list of joints, use them for all calls to UpdateJointMatricesData.
                if (mainSkin != null)
                {
                    UpdateJointMatricesData(mainSkin, jointMatrices, jointCount);
                }
                if (spriteSkins != null)
                {
                    foreach (var tuple in spriteSkins)
                    {
                        var spriteSkin = tuple.Item2;
                        var spriteFlags = tuple.Item1.RenderFlags;
                        var spriteCenter = tuple.Item1.SpriteCenter;
                        UpdateJointMatricesData(spriteSkin, jointMatrices, jointCount, spriteFlags, spriteCenter);
                    }
                }
            }
        }


        public void BindEntityBounds(EntityBase entity, Color3? color = null, float thickness = 1f, bool updateMeshData = true)
        {
            if (entity == null)
            {
                return;
            }
            var lineBuilder = new LineMeshBuilder
            {
                Thickness = thickness,
                SolidColor = color ?? Color3.White,
            };
            if (updateMeshData)
            {
                lineBuilder.AddBoundsOutline(entity.Bounds3D);
            }
            BindLineMesh(lineBuilder, null, updateMeshData);
        }

        public void BindTriangleOutline(ModelEntity model, Triangle triangle, Color3? color = null, float thickness = 2f, bool updateMeshData = true)
        {
            if (model == null || triangle == null)
            {
                return;
            }
            var lineBuilder = new LineMeshBuilder
            {
                Thickness = thickness,
                SolidColor = color ?? Color3.White,
            };
            var vertices = new Vector3[3];
            var normals  = new Vector3[3];
            if (updateMeshData)
            {
                var jointMatrices = model.NeedsJointTransform ? model.GetRootEntity().JointMatrices : null;
                var worldMatrix = model.WorldMatrix;
                triangle.TransformPositions(ref worldMatrix, jointMatrices, out vertices[0], out vertices[1], out vertices[2]);
                triangle.TransformNormals(ref worldMatrix, jointMatrices, out normals[0], out normals[1], out normals[2]);

                lineBuilder.AddTriangleOutline(vertices[0], vertices[1], vertices[2]);
            }
            BindLineMesh(lineBuilder, null, updateMeshData);

            // Draw normals for triangle
            lineBuilder.Clear();
            lineBuilder.Thickness = 6f;
            lineBuilder.SolidColor = Color3.Red;
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

        public void SetupEntitySkeleton(RootEntity entity, bool? coordinateBased = null, float? radius = null, float? thickness = null, bool updateMeshData = true)
        {
            if (entity == null || ((coordinateBased ?? false) && entity.Coords == null))
            {
                ResetIndex();
                if (updateMeshData)
                {
                    Reset(0);
                }
                return;
            }

            // Re-use the same spheres and lines for each bind and call to this function
            const RenderFlags renderFlags = RenderFlags.Unlit | RenderFlags.NoAmbient | RenderFlags.SemiTransparent;
            const float alpha = 0.75f;
            if (_skeletonJointBuilderSphere == null)
            {
                _skeletonJointBuilderSphere = new TriangleMeshBuilder
                {
                    RenderFlags = renderFlags,
                    MixtureRate = MixtureRate.Alpha,
                    Alpha = alpha,
                    SolidColor = SkeletonJointColor,
                };
                _skeletonJointBuilderSphere.AddOctaSphere(Vector3.Zero, radius: 1f, subdivision: 3);

                _skeletonBoneBuilderCylinder = new TriangleMeshBuilder
                {
                    RenderFlags = renderFlags,
                    MixtureRate = MixtureRate.Alpha,
                    Alpha = alpha,
                    SolidColor = SkeletonBoneColor,
                };
                _skeletonBoneBuilderCylinder.AddCylinder(2, Vector3.UnitZ * 0.5f, height: 0.5f, radius: 1f, sides: 12);

                _skeletonBoneBuilderLine = new LineMeshBuilder
                {
                    RenderFlags = renderFlags,
                    MixtureRate = MixtureRate.Alpha,
                    Alpha = alpha,
                    SolidColor = SkeletonBoneColor,
                };
                _skeletonBoneBuilderLine.AddLine(Vector3.Zero, Vector3.UnitZ);
            }

            if (!coordinateBased.HasValue)
            {
                coordinateBased = entity?.Coords != null;
            }

            var boneCount = 0;
            var jointCount = 0;
            if (coordinateBased.Value)
            {
                if (updateMeshData)
                {
                    // Bone count is only needed to reset mesh batch
                    foreach (var coord in entity.Coords)
                    {
                        if (coord.HasParent)
                        {
                            boneCount++;
                        }
                    }
                }
                jointCount = entity.Coords.Length;
            }
            else
            {
                if (_skeletonTMDIDModels == null)
                {
                    _skeletonTMDIDModels = new List<ModelEntity>();
                }
                _skeletonTMDIDModels.Clear();

                // Only locate skeleton joints when updating mesh data
                if (updateMeshData)
                {
                    // We only want one joint per TMDID.
                    // We have a bunch of extra logic here to avoid using sprites as the joint model unless we have to.
                    uint lastTMDID = 0;
                    ModelEntity lastModel = null;
                    // We're making an assumption that models with the same TMDID will always be grouped together.
                    // That is to say, a model with a different TMDID will never appear inbetween other models with the same TMDID.
                    foreach (ModelEntity model in entity.ChildEntities)
                    {
                        if (lastTMDID != model.TMDID || lastModel == null)
                        {
                            if (lastModel != null)
                            {
                                // Flush chosen model as the joint
                                _skeletonTMDIDModels.Add(lastModel);
                            }
                            lastTMDID = model.TMDID;
                            lastModel = model;
                        }
                        else if (!lastModel.IsJoint)
                        {
                            if (model.IsJoint)
                            {
                                lastModel = model;
                            }
                            else if (!model.IsSprite && lastModel.IsSprite)
                            {
                                lastModel = model;
                            }
                        }
                    }
                    if (lastModel != null)
                    {
                        // Flush chosen model as the joint
                        _skeletonTMDIDModels.Add(lastModel);
                    }
                }
                jointCount = _skeletonTMDIDModels.Count;
            }

            ResetIndex();
            if (updateMeshData)
            {
                Reset(jointCount + boneCount);
            }

            if (!radius.HasValue && updateMeshData)
            {
                // Only calculate new radius when updating mesh data
                // Use standard deviation to find a good average model magnitude from center,
                // and use that to create a radius for our joints.
                const float MaxDeviation = 2f;

                ReuseArray(ref _skeletonMagnitudes, entity.ChildEntities.Length);

                var averageMagnitude = 0f;
                var magnitudeCount = 0;
                for (var i = 0; i < entity.ChildEntities.Length; i++)
                {
                    var model = (ModelEntity)entity.ChildEntities[i];
                    var magnitude = model.Bounds3D.MagnitudeFromCenter;
                    if (magnitude > 0f)
                    {
                        _skeletonMagnitudes[magnitudeCount++] = magnitude;
                        averageMagnitude += magnitude;
                    }
                }

                if (magnitudeCount > 0)
                {
                    averageMagnitude /= magnitudeCount;

                    var standardDeviation = 0f;
                    for (var i = 0; i < magnitudeCount; i++)
                    {
                        var magnitude = _skeletonMagnitudes[i];
                        standardDeviation += (magnitude - averageMagnitude) * (magnitude - averageMagnitude);
                    }
                    standardDeviation = (float)Math.Sqrt(standardDeviation / magnitudeCount);

                    var trimmedMagnitude = 0f;
                    var trimmedCount = 0;
                    for (var i = 0; i < magnitudeCount; i++)
                    {
                        var magnitude = _skeletonMagnitudes[i];
                        if (Math.Abs(magnitude - averageMagnitude) <= standardDeviation * MaxDeviation)
                        {
                            trimmedMagnitude += magnitude;
                            trimmedCount++;
                        }
                    }
                    var average = (trimmedCount > 0 ? (trimmedMagnitude / trimmedCount) : averageMagnitude);

                    radius = average / 6f;
                }
                else
                {
                    radius = 1f;
                }
                _skeletonJointRadius = radius.Value;
            }
            else if (!radius.HasValue)
            {
                radius = _skeletonJointRadius; // Use the radius we computed when updateMeshData was true
            }
            if (!thickness.HasValue)
            {
                thickness = 4f;
            }

            // Compute the world matrices for each joint and store them for re-use with both bones and joints
            ReuseArray(ref _jointMatrices, jointCount);

            var rootTempMatrix = entity.TempMatrix;
            for (var i = 0; i < jointCount; i++)
            {
                Matrix4 worldMatrix;
                if (coordinateBased.Value)
                {
                    worldMatrix = entity.Coords[i].WorldMatrix;
                }
                else
                {
                    worldMatrix = _skeletonTMDIDModels[i].TempWorldMatrix;
                }
                Matrix4.Mult(ref rootTempMatrix, ref worldMatrix, out _jointMatrices[i]);
            }

            // Add bone lines before joints, so that the semi-transparent bones don't draw over joints
            if (coordinateBased.Value)
            {
                Mesh sourceBoneMesh = null; // Re-use the same mesh data for all bones
                for (var i = 0; i < jointCount; i++)
                {
                    var coord = entity.Coords[i];
                    if (coord.HasParent)
                    {
                        var mesh = BindEntitySkeletonBone(sourceBoneMesh, thickness.Value, radius.Value, ref _jointMatrices[i], ref _jointMatrices[coord.ParentID], updateMeshData);
                        if (sourceBoneMesh == null)
                        {
                            sourceBoneMesh = mesh;
                        }
                    }
                }
            }

            // Add joint nodes.
            Mesh sourceJointMesh = null; // Re-use the same mesh data for all joints
            for (var i = 0; i < jointCount; i++)
            {
                var mesh = BindEntitySkeletonJoint(sourceJointMesh, radius.Value, ref _jointMatrices[i], updateMeshData);
                if (sourceJointMesh == null)
                {
                    sourceJointMesh = mesh;
                }
            }
        }

        private Mesh BindEntitySkeletonJoint(Mesh sourceMesh, float radius, ref Matrix4 worldMatrix, bool updateMeshData)
        {
            _skeletonJointBuilderSphere.Visible = false;
            var jointMatrix = Matrix4.Identity;

            if (worldMatrix.Determinant != 0f)
            {
                var position = worldMatrix.ExtractTranslation();

                var scaleMatrix = Matrix4.CreateScale(radius);
                var translationMatrix = Matrix4.CreateTranslation(position);
                jointMatrix = scaleMatrix * translationMatrix;
                _skeletonJointBuilderSphere.Visible = true;
            }

            return BindTriangleMesh(_skeletonJointBuilderSphere, jointMatrix, updateMeshData, sourceMesh);
        }

        private Mesh BindEntitySkeletonBone(Mesh sourceMesh, float thickness, float radius, ref Matrix4 worldMatrix, ref Matrix4 parentWorldMatrix, bool updateMeshData)
        {
            _skeletonBoneBuilderCylinder.Visible = false;
            _skeletonBoneBuilderLine.Visible = false;
            _skeletonBoneBuilderLine.Thickness = thickness;
            var boneMatrix = Matrix4.Identity;

            if (worldMatrix.Determinant != 0f && parentWorldMatrix.Determinant != 0f)
            {
                var position1 = worldMatrix.ExtractTranslation();
                var position2 = parentWorldMatrix.ExtractTranslation();

                var direction = (position2 - position1);
                if (!direction.IsZero())
                {
                    // Transform UnitZ line so that it connects from position1 to position2.
                    // Get the scale for the line length
                    var lengthScale = direction.Length;
                    // Get the scale for the line thickness
                    var thicknessScale = radius / 4f;
                    // Calculate the rotation for the line direction
                    direction.Normalize();
                    var axis = Vector3.Cross(direction, Vector3.UnitZ);
                    var partialAngle = (float)Math.Asin(axis.Length);
                    var finalAngle = direction.Z < 0f ? ((float)Math.PI - partialAngle) : partialAngle;
                    axis.Normalize();

                    var scaleMatrix = Matrix4.CreateScale(thicknessScale, thicknessScale, lengthScale);
                    var rotationMatrix = Matrix4.CreateFromAxisAngle(axis, -finalAngle);
                    var translationMatrix = Matrix4.CreateTranslation(position1);
                    boneMatrix = scaleMatrix * rotationMatrix * translationMatrix;
                    _skeletonBoneBuilderCylinder.Visible = true;
                    _skeletonBoneBuilderLine.Visible = true;
                }
            }

            if (SkeletonBoneUseCylinder)
            {
                return BindTriangleMesh(_skeletonBoneBuilderCylinder, boneMatrix, updateMeshData, sourceMesh);
            }
            else
            {
                return BindLineMesh(_skeletonBoneBuilderLine, boneMatrix, updateMeshData, sourceMesh);
            }
        }


        private Mesh BindModelMesh(VertexArrayObject vao, ref int elementIndex, int elementFirst, ModelEntity model, ref Matrix4 matrix, bool updateMeshData = true)
        {
            // SkinIndex is incremented in calling function
            if (model.Triangles.Length == 0)
            {
                MeshIndex++; // Still consume the mesh index (unless we change how these are reserved later...)
                return null;
            }

            var mesh = GetMesh(MeshIndex++, vao);
            if (mesh == null)
            {
                return null;
            }

            CopyRenderInfo(mesh, model, ref matrix);

            if (updateMeshData && mesh.SourceMesh == null)
            {
                var isLines = _scene.VibRibbonWireframe || mesh.RenderFlags.HasFlag(RenderFlags.Line);
                var uvConverter = model.TextureLookup;
                UpdateTriangleMeshDataSource(mesh, ref elementIndex, elementFirst, model.Triangles, isLines, uvConverter,
                    model.InitialVertices, model.InitialNormals,
                    model.FinalVertices, model.FinalNormals, model.Interpolator);
            }
            return mesh;
        }

        private void CopyRenderInfo(Mesh mesh, ModelEntity model, ref Matrix4 matrix)
        {
            mesh.CopyFrom(model);
            mesh.WorldMatrix = matrix;
            // Attempting to batch sprites is a waste of time.
            // Don't batch extended render settings that we don't check.
            mesh.IsBatchable = !mesh.IsSprite && model.DebugMeshRenderInfo == null;
            CopyTextureBinderTextureID(mesh);
        }

        private void CopyRenderInfo(Mesh mesh, MeshRenderInfo renderInfo, ref Matrix4? matrix)
        {
            mesh.CopyFrom(renderInfo);
            mesh.WorldMatrix = matrix ?? Matrix4.Identity;
            mesh.IsBatchable = false;
            CopyTextureBinderTextureID(mesh);
        }

        private void CopyTextureBinderTextureID(Mesh mesh)
        {
            if (mesh.IsTextured)
            {
                mesh.TextureID = _scene.TextureBinder.GetTextureID(mesh.TexturePage);
            }
            else
            {
                mesh.TextureID = 0;
            }
        }

        // Used to just update render info if we know we aren't updating mesh data.
        public Mesh BindRenderInfo(MeshRenderInfo renderInfo, Matrix4? matrix = null)
        {
            SkinIndex++; // Always increment skin index per mesh
            var mesh = GetMesh(MeshIndex++, VertexArrayObjectIndex++);
            if (mesh == null)
            {
                return null;
            }

            CopyRenderInfo(mesh, renderInfo, ref matrix);
            return mesh;
        }

        public Mesh BindTriangleMesh(TriangleMeshBuilder triangleBuilder, Matrix4? matrix = null, bool updateMeshData = true, Mesh sourceMesh = null, Skin sourceSkin = null)
        {
            return BindTriangleMesh(triangleBuilder, out _, matrix, updateMeshData, sourceMesh, sourceSkin);
        }

        public Mesh BindTriangleMesh(TriangleMeshBuilder triangleBuilder, out Skin skin, Matrix4? matrix = null, bool updateMeshData = true, Mesh sourceMesh = null, Skin sourceSkin = null)
        {
            skin = null;
            var skinIndex = SkinIndex++; // Always increment skin index per mesh
            var mesh = GetMesh(MeshIndex++, VertexArrayObjectIndex++, sourceMesh);
            if (mesh == null)
            {
                return null;
            }

            CopyRenderInfo(mesh, triangleBuilder, ref matrix);

            if (triangleBuilder.JointMatrices != null && Shader.JointsSupported)
            {
                mesh.Skin = skin = GetSkin(skinIndex, sourceSkin);
                if (sourceSkin != null)
                {
                    UpdateJointMatricesData(mesh.Skin, triangleBuilder.JointMatrices, triangleBuilder.JointMatrices.Length,
                                            triangleBuilder.RenderFlags, triangleBuilder.SpriteCenter);
                }
            }
            if (updateMeshData && mesh.SourceMesh == null)
            {
                var isLines = mesh.RenderFlags.HasFlag(RenderFlags.Line);
                UpdateTriangleMeshData(mesh, triangleBuilder.Triangles, isLines);
            }
            return mesh;
        }

        public Mesh BindLineMesh(LineMeshBuilder lineBuilder, Matrix4? matrix = null, bool updateMeshData = true, Mesh sourceMesh = null)
        {
            SkinIndex++; // Always increment skin index per mesh
            var mesh = GetMesh(MeshIndex++, VertexArrayObjectIndex++, sourceMesh);
            if (mesh == null)
            {
                return null;
            }

            CopyRenderInfo(mesh, lineBuilder, ref matrix);
            // Enforced certain flags for lines
            mesh.RenderFlags |= RenderFlags.Unlit | RenderFlags.DoubleSided;
            mesh.RenderFlags &= ~RenderFlags.Textured;

            if (updateMeshData && mesh.SourceMesh == null)
            {
                UpdateLineMeshData(mesh, lineBuilder.Lines);
            }
            return mesh;
        }

        private void UpdateLineMeshData(Mesh mesh, IReadOnlyList<Line> lines)
        {
            var elementCount = lines.Count * 2;
            var elementIndex = 0;
            var positionList  = ReuseArray(ref _positionList,  elementCount * 3); // Vector3
            var colorList     = ReuseArray(ref _colorList,     elementCount * 3); // Vector3 (Color3)
            // Empty all-zero lists
            var normalList    = ReuseArray(ref _normalListEmpty,    elementCount * 3); // Vector3
            var uvList        = ReuseArray(ref _uvListEmpty,        elementCount * 2); // Vector2
            var tiledAreaList = ReuseArray(ref _tiledAreaListEmpty, elementCount * 4); // Vector4
            var jointList     = ReuseArray(ref _jointListEmpty,     elementCount * 2); // uint[2]
            foreach (var line in lines)
            {
                for (var i = 0; i < 2; i++, elementIndex++)
                {
                    var index3d = elementIndex * 3;

                    var vertex = line.Vertices[i];// i == 0 ? line.P1 : line.P2;
                    positionList[index3d + 0] = vertex.X;
                    positionList[index3d + 1] = vertex.Y;
                    positionList[index3d + 2] = vertex.Z;
                    //VertexArrayObject.AssignVector3(positionList, elementIndex, ref vertex);

                    // Normals are all 0f.

                    var color = line.Colors[i];// i == 0 ? line.Color1 : line.Color2;
                    colorList[index3d + 0] = color.R;
                    colorList[index3d + 1] = color.G;
                    colorList[index3d + 2] = color.B;
                    //VertexArrayObject.AssignColor3(colorList, elementIndex, ref color);

                    // UVs are all 0f.

                    // Tiled areas are all 0f.

                    // Joints are all 0u (no joint, since joints are 1-indexed in the shader).
                }
            }
            mesh.SetData(MeshDataType.Line, elementCount, positionList, normalList, colorList, uvList, tiledAreaList, jointList);
        }

        private void PrepareTriangleMeshDataLists(int elementCount)
        {
            ReuseArray(ref _positionList,  elementCount * 3); // Vector3
            ReuseArray(ref _normalList,    elementCount * 3); // Vector3
            ReuseArray(ref _colorList,     elementCount * 3); // Vector3 (Color3)
            ReuseArray(ref _uvList,        elementCount * 2); // Vector2
            ReuseArray(ref _tiledAreaList, elementCount * 4); // Vector4
            ReuseArray(ref _jointList,     elementCount * 2); // uint[2]
        }

        private void UpdateTriangleMeshDataSource(Mesh mesh, ref int elementIndex, int elementFirst, IReadOnlyList<Triangle> triangles, bool isLines, IUVConverter uvConverter = null,
                                                  Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                                  Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var verticesPerElement = isLines ? 2 : 3;

            //elementFirst = elementIndex;
            var elementCount = triangles.Count * verticesPerElement;
            //PrepareTriangleMeshDataLists(elementIndex + elementCount);
            UpdateTriangleMeshData(ref elementIndex, triangles, isLines, mesh.TexturePage, uvConverter,
                                   initialVertices, initialNormals, finalVertices, finalNormals, interpolator);

            var dataType = isLines ? MeshDataType.Line : MeshDataType.Triangle;
            mesh.SetDataSource(dataType, elementFirst, elementCount);
        }

        private void UpdateTriangleMeshData(Mesh mesh, IReadOnlyList<Triangle> triangles, bool isLines, IUVConverter uvConverter = null,
                                            Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                            Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var verticesPerElement = isLines ? 2 : 3;

            var elementIndex = 0;
            var elementCount = triangles.Count * verticesPerElement;
            PrepareTriangleMeshDataLists(elementCount);
            UpdateTriangleMeshData(ref elementIndex, triangles, isLines, mesh.TexturePage, uvConverter,
                                   initialVertices, initialNormals, finalVertices, finalNormals, interpolator);

            var dataType = isLines ? MeshDataType.Line : MeshDataType.Triangle;
            mesh.SetData(dataType, elementCount, _positionList, _normalList, _colorList, _uvList, _tiledAreaList, _jointList);
        }

        private void UpdateTriangleMeshData(ref int elementIndex, IReadOnlyList<Triangle> triangles, bool isLines,
                                            uint texturePage, IUVConverter uvConverter = null,
                                            Vector3[] initialVertices = null, Vector3[] initialNormals = null,
                                            Vector3[] finalVertices = null, Vector3[] finalNormals = null, float interpolator = 0f)
        {
            var verticesPerElement = isLines ? 2 : 3;

            var positionList  = _positionList;
            var normalList    = _normalList;
            var colorList     = _colorList;
            var uvList        = _uvList;
            var tiledAreaList = _tiledAreaList;
            var jointList     = _jointList;
            foreach (var triangle in triangles)
            {
                var isTiled = triangle.IsTiled;
                var tiledArea = triangle.TiledUv?.Area ?? Vector4.Zero;
                if (isTiled)
                {
                    if (uvConverter != null)
                    {
                        tiledArea = uvConverter.ConvertTiledArea(tiledArea);
                    }
                    tiledArea = TextureBinder.ConvertTiledArea(tiledArea, texturePage);
                }
                for (var i = 0; i < verticesPerElement; i++, elementIndex++)
                {
                    var index2d = elementIndex * 2;
                    var index3d = elementIndex * 3;
                    var index4d = elementIndex * 4;

                    var vertex = triangle.Vertices[i];
                    if (initialVertices != null && finalVertices != null && triangle.OriginalVertexIndices[i] < finalVertices.Length)
                    {
                        var initialVertex = vertex + initialVertices[triangle.OriginalVertexIndices[i]];
                        var finalVertex = vertex + finalVertices[triangle.OriginalVertexIndices[i]];
                        Vector3.Lerp(ref initialVertex, ref finalVertex, interpolator, out vertex);
                    }

                    positionList[index3d + 0] = vertex.X;
                    positionList[index3d + 1] = vertex.Y;
                    positionList[index3d + 2] = vertex.Z;
                    //VertexArrayObject.AssignVector3(positionList, elementIndex, ref vertex);

                    var normal = triangle.Normals[i];
                    if (initialNormals != null && finalNormals != null && triangle.OriginalNormalIndices[i] < finalNormals.Length)
                    {
                        // todo: Why is 4096f used here?
                        var initialNormal = normal + initialNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        var finalNormal = normal + finalNormals[triangle.OriginalNormalIndices[i]] / 4096f;
                        Vector3.Lerp(ref initialNormal, ref finalNormal, interpolator, out normal);
                    }

                    normalList[index3d + 0] = normal.X;
                    normalList[index3d + 1] = normal.Y;
                    normalList[index3d + 2] = normal.Z;
                    //VertexArrayObject.AssignVector3(normalList, elementIndex, ref normal);

                    var color = triangle.Colors[i];
                    colorList[index3d + 0] = color.R;
                    colorList[index3d + 1] = color.G;
                    colorList[index3d + 2] = color.B;
                    //VertexArrayObject.AssignColor3(colorList, elementIndex, ref color);

                    // If we're tiled, then the shader needs the base UV, not the converted UV.
                    var uv = triangle.TiledUv?.BaseUv[i] ?? triangle.Uv[i];
                    if (uvConverter != null)
                    {
                        uv = uvConverter.ConvertUV(uv, isTiled);
                    }
                    uv = TextureBinder.ConvertUV(uv, isTiled, texturePage);
                    uvList[index2d + 0] = uv.X;
                    uvList[index2d + 1] = uv.Y;
                    //VertexArrayObject.AssignVector2(uvList, elementIndex, ref uv);

                    tiledAreaList[index4d + 0] = tiledArea.X; // U offset
                    tiledAreaList[index4d + 1] = tiledArea.Y; // V offset
                    tiledAreaList[index4d + 2] = tiledArea.Z; // U wrap
                    tiledAreaList[index4d + 3] = tiledArea.W; // V wrap
                    //VertexArrayObject.AssignVector4(tiledAreaList, elementIndex, ref tiledArea);

                    // This expects NoJoint to be equal to -1, meaning it will equate to zero after adding 1.
                    // Joints in the shader are 1-indexed, this makes it easier to handle meshes with no joints,
                    // since the default value of zero (for Array.Clear) is what we want as the default value.
                    jointList[index2d + 0] = (triangle.VertexJoints?[i] ?? Triangle.NoJoint) + 1u;
                    jointList[index2d + 1] = (triangle.NormalJoints?[i] ?? Triangle.NoJoint) + 1u;
                    //VertexArrayObject.AssignJoint(jointList, elementIndex, triangle.VertexJoints?[i], triangle.NormalJoints?[i]);
                }
            }

            if (isLines)
            {
                // Always reserve 3 vertices per primitive so that in the future,
                // changing RenderFlags.Line, won't require rebuilding the entire VAO.
                elementIndex += triangles.Count;
            }
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

        private void UpdateJointMatricesSubData(Skin skin, ref Matrix4 jointMatrix, int jointFirst, RenderFlags spriteFlags = RenderFlags.None, Vector3 spriteCenter = default(Vector3))
        {
            var elementFirst = jointFirst;
            const int elementCount = 1;
            var jointMatrixList = ReuseArray(ref _jointMatrixList, elementCount * 32); // Matrix4[2]

            ComputeModelMatrix(ref jointMatrix, out var modelMatrix, spriteFlags, ref spriteCenter);
            ComputeNormalMatrix(ref modelMatrix, out var normalMatrix);

            Skin.AssignModelMatrix(jointMatrixList, 0, ref modelMatrix);
            Skin.AssignNormalMatrix(jointMatrixList, 0, ref normalMatrix);

            skin.SetSubData(elementFirst, elementCount, jointMatrixList);
        }



        public void Reset(int meshCount)
        {
            Reset(meshCount, meshCount, meshCount);
        }

        protected void Reset(int meshCount, int vaoCount, int skinCount)
        {
            IsInitialized = true;
            if (vaoCount == -1)
            {
                vaoCount = meshCount;
            }
            if (skinCount == -1)
            {
                skinCount = meshCount;
            }
            // Dispose of old vertex array objects and skins.
            if (_vertexArrayObjects != null)
            {
                for (var i = 0; i < VertexArrayObjectCount; i++)
                {
                    _vertexArrayObjects[i]?.Dispose();
                }
            }
            if (_skins != null)
            {
                for (var i = 0; i < SkinCount; i++)
                {
                    _skins[i]?.Dispose();
                }
            }
            // Create a new meshes array, or reset the existing one to all null if the lengths match.
            // todo: Should we bother resizing to a smaller array size?...
            // Maybe only if the size is drastically smaller...
            if (_vertexArrayObjects == null || _vertexArrayObjects.Length < vaoCount)
            {
                _vertexArrayObjects = new VertexArrayObject[vaoCount];
            }
            else
            {
                Array.Clear(_vertexArrayObjects, 0, VertexArrayObjectCount);
            }
            if (_skins == null || _skins.Length < skinCount)
            {
                _skins = new Skin[skinCount];
            }
            else
            {
                Array.Clear(_skins, 0, SkinCount);
            }
            if (_meshes == null || _meshes.Length < meshCount)
            {
                _meshes = new Mesh[meshCount];
                if (_sortingEnabled || _batchingEnabled)
                {
                    _sortedMeshes = new Mesh[meshCount];
                }
            }
            else
            {
                Array.Clear(_meshes, 0, MeshCount);
                if (_sortingEnabled || _batchingEnabled)
                {
                    Array.Clear(_sortedMeshes, 0, MeshCount);
                }
            }
            // Assign this in-case we support changing mesh count without changing capacity.
            VertexArrayObjectCount = vaoCount;
            SkinCount = skinCount;
            MeshCount = meshCount;
            _sortedMeshCount = 0;
            ResetIndex();
        }

        public void ResetIndex()
        {
            VertexArrayObjectIndex = 0;
            SkinIndex = 0;
            MeshIndex = 0;
        }

        protected VertexArrayObject GetVertexArrayObject(int index)
        {
            if (index >= VertexArrayObjectCount)
            {
                return null;
            }
            if (_vertexArrayObjects[index] == null)
            {
                _vertexArrayObjects[index] = new VertexArrayObject(index);
            }
            return _vertexArrayObjects[index];
        }

        protected Skin GetSkin(int index, Skin sourceSkin = null)
        {
            if (index >= SkinCount)
            {
                return null;
            }
            if (_skins[index] == null)
            {
                // Unlike meshes, skins don't have any unique information besides their data, so just return the source
                _skins[index] = sourceSkin == null ? new Skin(index) : sourceSkin;
            }
            return _skins[index];
        }

        protected Mesh GetMesh(int index, VertexArrayObject vao)
        {
            if (index >= MeshCount)
            {
                return null;
            }
            if (_meshes[index] == null)
            {
                _meshes[index] = new Mesh(index, vao);
            }
            return _meshes[index];
        }

        protected Mesh GetMesh(int index, int vaoIndex, Mesh sourceMesh = null)
        {
            if (index >= MeshCount)
            {
                return null;
            }
            if (_meshes[index] == null)
            {
                _meshes[index] = sourceMesh == null ? new Mesh(index, GetVertexArrayObject(vaoIndex)) : new Mesh(index, sourceMesh);
            }
            return _meshes[index];
        }

        protected static T[] ReuseArray<T>(ref T[] array, int length, bool clear = false)
        {
            if (array == null || array.Length < length)
            {
                array = new T[length];
                return array;
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
            foreach (var pass in _passes)
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
            if (!IsInitialized || !Visible)
            {
                return;
            }
            if (renderPass != RenderPass.Pass1Opaque && !SemiTransparencyEnabled)
            {
                return; // No semi-transparent passes
            }

            _shader.UniformViewMatrix(ref viewMatrix);
            _shader.UniformProjectionMatrix(ref projectionMatrix);

            if (SolidColor != null)
            {
                _shader.UniformColorMode = 1; // Use solid color
                _shader.UniformSolidColor = SolidColor.Value;
            }

            switch (renderPass)
            {
                case RenderPass.Pass1Opaque:
                    // Pass 1: Draw opaque meshes.

                    foreach (var mesh in GetVisibleMeshes())
                    {
                        if (SemiTransparencyEnabled && !mesh.IsOpaque)
                        {
                            continue; // Not an opaque mesh, or semi-transparency is disabled
                        }

                        // Set state in loop so that it's not set when there are no meshes.
                        _shader.DepthMask = true;
                        _shader.Blend = false;
                        _shader.UniformSemiTransparentPass = 0;

                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
                    }
                    break;

                case RenderPass.Pass2SemiTransparentOpaquePixels when SemiTransparencyEnabled && TexturesEnabled:
                    // Pass 2: Draw opaque pixels when the stp bit is UNSET.

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

                        // Set state in loop so that it's not set when there are no meshes.
                        _shader.DepthMask = true;
                        _shader.Blend = false;
                        _shader.UniformSemiTransparentPass = 1;

                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
                    }
                    break;

                case RenderPass.Pass3SemiTransparent when SemiTransparencyEnabled:
                    // Pass 3: Draw semi-transparent pixels when the stp bit is SET.

                    foreach (var mesh in GetVisibleMeshes())
                    {
                        if (!mesh.IsSemiTransparent)
                        {
                            continue; // Not a semi-transparent mesh
                        }

                        // Set state in loop so that it's not set when there are no meshes.
                        _shader.DepthMask = false; // Disable so that transparent surfaces can show behind other transparent surfaces.
                        _shader.Blend = true;
                        _shader.UniformSemiTransparentPass = 2;

                        if (mesh.MixtureRate == MixtureRate.Alpha)
                        {
                            _shader.Alpha = mesh.Alpha;
                        }
                        _shader.MixtureRate = mesh.MixtureRate;

                        triangleCount += DrawMesh(mesh, ref viewMatrix, ref projectionMatrix);
                        meshCount++;
                        skinCount += (mesh.Skin != null ? 1 : 0);
                    }
                    break;
            }
        }

        protected void SortAndBatchMeshes(bool noSort = false)
        {
            if (_sortingEnabled || _batchingEnabled)
            {
                // Sequentially add all visible meshes to an array
                _sortedMeshCount = 0;
                for (var i = 0; i < MeshCount; i++)
                {
                    var mesh = _meshes[i];
                    if (mesh != null)
                    {
                        mesh.ResetBatch();
                        if (mesh.Visible)
                        {
                            _sortedMeshes[_sortedMeshCount++] = mesh;
                        }
                    }
                }

                // Sort meshes by similar render settings
                if (_sortingEnabled && !noSort)
                {
                    // Todo: Conditional sorting based on the rate at which the mesh batch is being setup.
                    // Disable in the Animations tab, but enable in the Models tab.
                    Array.Sort(_sortedMeshes, 0, _sortedMeshCount);//, Comparer<Mesh>.Default);
                }

                // Now try to batch meshes with identical render settings to reduce draw calls
                if (_batchingEnabled)
                {
                    var batchedMeshCount = 0;
                    for (var i = 0; i < _sortedMeshCount; i++)
                    {
                        var mesh = _sortedMeshes[i];
                        if (mesh.IsBatchable)
                        {
                            for (var j = i + 1; j < _sortedMeshCount; j++, i++)
                            {
                                var nextMesh = _sortedMeshes[j];
                                if (!mesh.CanBatch(nextMesh))
                                {
                                    break;
                                }
                                mesh.AddToBatch(nextMesh);
                            }
                        }
                        _sortedMeshes[batchedMeshCount++] = mesh;
                    }
                    _sortedMeshCount = batchedMeshCount;
                }
            }
        }

        private IEnumerable<Mesh> GetVisibleMeshes()
        {
            if (_sortingEnabled || _batchingEnabled)
            {
                for (var i = 0; i < _sortedMeshCount; i++)
                {
                    yield return _sortedMeshes[i];
                }
            }
            else
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
            /*var sorted = (_sortingEnabled || _batchingEnabled);
            var meshes = sorted ? _sortedMeshes : _meshes;
            var count = sorted ? _sortedMeshCount : MeshCount;
            for (var i = 0; i < count; i++)
            {
                var mesh = meshes[i];
                if (mesh != null && mesh.Visible && !mesh.IsBatched)
                {
                    yield return mesh;
                }
            }*/
        }

        private int DrawMesh(Mesh mesh, ref Matrix4 viewMatrix, ref Matrix4 projectionMatrix)
        {
            var ambient = AmbientEnabled && !mesh.RenderFlags.HasFlag(RenderFlags.NoAmbient);
            var light = LightEnabled && !mesh.RenderFlags.HasFlag(RenderFlags.Unlit);
            if (ambient && light)
            {
                _shader.UniformLightMode = 0; // Enable ambient, enable directional light
            }
            else if (ambient)
            {
                _shader.UniformLightMode = 1; // Enable ambient, disable directional light
            }
            else if (light)
            {
                _shader.UniformLightMode = 2; // Disable ambient, enable directional light
            }
            else
            {
                _shader.UniformLightMode = 3; // Disable ambient, disable directional light
            }

            if (ambient)
            {
                _shader.UniformAmbientColor = mesh.AmbientColor ?? AmbientColor ?? (Color3)_scene.AmbientColor;
            }
            if (light)
            {
                _shader.UniformLightDirection = (mesh.LightDirection ?? LightDirection ?? _scene.LightDirection);
                _shader.UniformLightIntensity = (mesh.LightIntensity ?? LightIntensity ?? _scene.LightIntensity);
            }

            if (SolidColor == null)
            {
                // We only need to assign color mode and solid color if its not handled by this batch.
                if (mesh.SolidColor == null)
                {
                    _shader.UniformColorMode = 0; // Use vertex color
                }
                else
                {
                    _shader.UniformColorMode = 1; // Use solid color
                    _shader.UniformSolidColor = mesh.SolidColor.Value;
                }
            }

            var noMissing = mesh.MissingTexture && !_scene.ShowMissingTextures; // If missing textures should be untextured
            if (TexturesEnabled && !noMissing && mesh.HasTexture)
            {
                if (mesh.MissingTexture)
                {
                    _shader.UniformTextureMode = 2; // Missing texture
                }
                else
                {
                    _shader.UniformTextureMode = 0; // Enable texture
                    _shader.BindTexture(mesh.TextureID);
                }
            }
            else
            {
                _shader.UniformTextureMode = 1; // Disable texture
            }

            if (ForceDoubleSided || mesh.RenderFlags.HasFlag(RenderFlags.DoubleSided))
            {
                _shader.CullFace = false;
            }
            else
            {
                _shader.CullFace = true;
            }

            if (mesh.Skin != null && (!_modelsMeshBatch || _scene.AttachJointsMode == AttachJointsMode.Attach))
            {
                _shader.UniformJointMode = 0; // Enable joint transforms
                _shader.BindSkin(mesh.Skin);
            }
            else if (!_modelsMeshBatch || _scene.AttachJointsMode != AttachJointsMode.Hide)
            {
                _shader.UniformJointMode = 1; // Disable joint transforms (or this mesh has no joints/skin)
            }
            else
            {
                _shader.UniformJointMode = 2; // Hide joint vertices
            }

            // This isn't used at all and is still experimental (requires tiled textures).
            // Currently using % 1.0 is not correct if the tiled texture is not a power of 2.
            var animX = mesh.TextureAnimation.X != 0f ? (float)(_scene.Time * mesh.TextureAnimation.X) % 1f : 0f;
            var animY = mesh.TextureAnimation.Y != 0f ? (float)(_scene.Time * mesh.TextureAnimation.Y) % 1f : 0f;
            _shader.UniformUVOffset = new Vector2(animX, animY);


            var worldMatrix = mesh.WorldMatrix;
            var spriteCenter = mesh.SpriteCenter;
            ComputeModelMatrix(ref worldMatrix, out var modelMatrix, mesh.RenderFlags, ref spriteCenter);
            Matrix4 modelSpriteMatrix;
            Matrix3 normalSpriteMatrix;
            if (mesh.IsSprite)
            {
                ComputeSpriteMatrix(out modelSpriteMatrix, mesh.RenderFlags, ref spriteCenter);
                ComputeNormalMatrix(ref modelSpriteMatrix, out normalSpriteMatrix);
            }
            else
            {
                modelSpriteMatrix = Matrix4.Identity;
                normalSpriteMatrix = Matrix3.Identity;
            }
            ComputeNormalMatrix(ref modelMatrix, out var normalMatrix);

            // Transpose the normal matrix here when setting the uniform (performed by UniformNormalMatrix)
            _shader.UniformNormalMatrix(ref normalMatrix);
            _shader.UniformNormalSpriteMatrix(ref normalSpriteMatrix);
            _shader.UniformModelMatrix(ref modelMatrix);
            _shader.UniformModelSpriteMatrix(ref modelSpriteMatrix);


            _shader.BindMesh(mesh);
            var drawCalls = mesh.Draw(_shader, DrawFaces, DrawWireframe, DrawVertices, WireframeSize, VertexSize);
            var triangleCount = mesh.BatchedPrimitiveCount * drawCalls;

            return triangleCount;
        }

        protected void ComputeModelMatrix(ref Matrix4 worldMatrix, out Matrix4 modelMatrix, RenderFlags spriteFlags, ref Vector3 spriteCenter)
        {
            if (spriteFlags.HasFlag(RenderFlags.Sprite) || spriteFlags.HasFlag(RenderFlags.SpriteNoPitch))
            {
                // Regardless of type, sprites must ALWAYS eliminate rotation applied by the world matrix
                // We can't even preserve roll, because that transform would only look correct when facing forward
                /*var invWorldRotation = worldMatrix.ExtractRotationSafe().Inverted();

                // Remove rotation applied by world matrix around the sprite's center
                // modelMatrix = invSpriteCenterMatrix * invWorldRotationMatrix * spriteCenterMatrix;
                GeomMath.CreateFromQuaternionAroundOrigin(ref invWorldRotation, ref spriteCenter, out modelMatrix);

                // Apply transform before world matrix transform
                // modelMatrix *= worldMatrix;
                Matrix4.Mult(ref modelMatrix, ref worldMatrix, out modelMatrix);*/

                GeomMath.CreateSpriteWorldMatrix(ref worldMatrix, ref spriteCenter, out modelMatrix);
            }
            else
            {
                modelMatrix = worldMatrix;
            }
        }

        protected void ComputeSpriteMatrix(out Matrix4 spriteMatrix, RenderFlags spriteFlags, ref Vector3 spriteCenter)
        {
            if (spriteFlags.HasFlag(RenderFlags.Sprite) || spriteFlags.HasFlag(RenderFlags.SpriteNoPitch))
            {
                // Sprites always face the camera
                Quaternion cameraRotation;
                if (spriteFlags.HasFlag(RenderFlags.Sprite))
                {
                    cameraRotation = _scene.CameraRotation; // Pitch and yaw rotation
                }
                else
                {
                    cameraRotation = _scene.CameraYawRotation; // Just yaw rotation
                }

                // Rotate sprite around its center
                // spriteMatrix = invSpriteCenterMatrix * cameraRotationMatrix * spriteCenterMatrix;
                GeomMath.CreateFromQuaternionAroundOrigin(ref cameraRotation, ref spriteCenter, out spriteMatrix);
            }
            else
            {
                spriteMatrix = Matrix4.Identity;
            }
        }

        // Does not Transpose, this should be handled when setting the uniform or assigning the joint matrix.
        protected static void ComputeNormalMatrix(ref Matrix4 modelMatrix, out Matrix3 normalMatrix)
        {
            // Use Inverted() since it checks determinant to avoid singular matrix exception.
            normalMatrix = new Matrix3(modelMatrix).Inverted();
            // Transpose is handled when assigning the uniform, or assigning the skin matrix.
            //normalMatrix.Transpose();
        }


        public static IEnumerable<RenderPass> GetPasses() => _passes;
    }
}
