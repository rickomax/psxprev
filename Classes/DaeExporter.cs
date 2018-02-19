using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Collada141;
using PSXPrev.Classes;

namespace PSXPrev
{
    public class DaeExporter
    {
        public DaeExporter()
        {

        }

        public void Export(RootEntity[] entities, string selectedPath)
        {
            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var modelCount = entity.ChildEntities.Length;
                var geometries = new library_geometries
                {
                    geometry = new geometry[modelCount]
                };
                var visualSceneNodes = new node[modelCount];
                const string visualSceneName = "visual-scene";
                var visualScenes = new library_visual_scenes
                {
                    visual_scene = new[]
                    {
                        new visual_scene
                        {
                            id = visualSceneName,
                            name = visualSceneName,
                            node = visualSceneNodes
                        }
                    }
                };
                for (int j = 0; j < modelCount; j++)
                {
                    var model = (ModelEntity) entity.ChildEntities[j];
                    var modelName = string.Format("model-{0}-lib", j);
                    var materialName = string.Format("{0}-material", modelName);
                    var triangleCount = model.Triangles.Count();
                    var elementGroupCount = triangleCount * 3;
                    var elementCount = elementGroupCount * 3;

                    var acessorParams = new[]
                    {
                        new param
                        {
                            name = "X",
                            type = "float"
                        },
                        new param
                        {
                            name = "Y",
                            type = "float"
                        },
                        new param
                        {
                            name = "Z",
                            type = "float"
                        }
                    };

                    #region Position

                    var positionArrayName = string.Format("{0}-positions-array", modelName);
                    var positionAccessor = new accessor
                    {
                        count = (ulong)elementGroupCount,
                        offset = 0,
                        source = string.Format("#{0}", positionArrayName),
                        stride = 3,
                        param = acessorParams
                    };
                    var positionTechnique = new sourceTechnique_common
                    {
                        accessor = positionAccessor
                    };
                    var positionArrayValues = new double[elementCount];
                    var positionArray = new float_array
                    {
                        id = positionArrayName,
                        count = (ulong)elementCount,
                        Values = positionArrayValues
                    };
                    var positionName = string.Format("{0}-positions", modelName);
                    var positionSource = new source
                    {
                        id = positionName,
                        name = "position",
                        Item = positionArray,
                        technique_common = positionTechnique
                    };
                    #endregion

                    #region Normal
                    var normalArrayName = string.Format("{0}-normals-array", modelName);
                    var normalAccessor = new accessor
                    {
                        count = (ulong)elementGroupCount,
                        offset = 0,
                        source = string.Format("#{0}", normalArrayName),
                        stride = 3,
                        param = acessorParams
                    };
                    var normalTechinique =
                        new sourceTechnique_common
                        {
                            accessor = normalAccessor
                        };
                    var normalArrayValues = new double[elementCount];
                    var normalArray = new float_array
                    {
                        id = normalArrayName,
                        count = (ulong)elementCount,
                        Values = normalArrayValues
                    };
                    var normalName = string.Format("{0}-normals", modelName);
                    var normalSource = new source
                    {
                        id = normalName,
                        name = "normal",
                        Item = normalArray,
                        technique_common = normalTechinique
                    };
                    #endregion

                    #region Processing
                    var triangleIndices = new StringBuilder();
                    for (int l = 0; l < model.Triangles.Length; l++)
                    {
                        var triangle = model.Triangles[l];
                        for (var k = 0; k < 3; k++)
                        {
                            var elementIndex = l * 3 + k;

                            var vertex = triangle.Vertices[k];
                            var normal = triangle.Normals[k];
                            var color = triangle.Colors[k];
                            var uv = triangle.Uv[k];

                            positionArrayValues[elementIndex] = vertex.X;
                            positionArrayValues[elementIndex + 1] = vertex.Y;
                            positionArrayValues[elementIndex + 2] = vertex.Z;

                            normalArrayValues[elementIndex] = normal.X;
                            normalArrayValues[elementIndex + 1] = normal.Y;
                            normalArrayValues[elementIndex + 2] = normal.Z;

                            triangleIndices.Append(elementIndex);
                            triangleIndices.Append(" ");
                        }
                    }

                    #endregion

                    #region Vertices
                    var verticesName = string.Format("{0}-vertices", modelName);
                    var triangles = new triangles 
                    {
                        count = (ulong)triangleCount,
                        material = materialName,
                        input = new[]
                        {
                            new InputLocalOffset
                            {
                                offset = 0,
                                semantic = "VERTEX",
                                source = string.Format("#{0}", verticesName)
                            }//,
                            //new InputLocalOffset
                            //{
                            //    offset = 1,
                            //    semantic = "NORMAL",
                            //    source = string.Format("#{0}", normalName)
                            //}
                        },
                        p = triangleIndices.ToString()
                    };
                    #endregion

                    #region Mesh
                    var mesh = new mesh
                    {
                        source = new[] { positionSource, normalSource },
                        vertices = new vertices
                        {
                            id = verticesName,
                            input = new[]
                            {
                                new InputLocal
                                {
                                    semantic = "POSITION",
                                    source = string.Format("#{0}", positionName)
                                }
                            }
                        },
                        Items = new object[] { triangles }
                    };
                    #endregion

                    #region Geometry
                    var geometryName = string.Format("{0}-geometry", modelName);
                    var geometry = new geometry
                    {
                        id = geometryName,
                        name = geometryName,
                        Item = mesh
                    };
                    geometries.geometry[j] = geometry;
                    #endregion

                    #region Visual Node
                    var visualSceneNodeName = string.Format("{0}-node", modelName);
                    visualSceneNodes[j] = new node
                    {
                        name = visualSceneNodeName,
                        id = visualSceneNodeName,
                        instance_geometry = new[]
                        {
                            new instance_geometry
                            {
                                url = string.Format("#{0}", geometryName)
                            }
                        }
                    };
                    #endregion
                }
                var collada = new COLLADA
                {
                    Items = new Object[]
                    {
                        geometries,
                        visualScenes
                    },
                    scene = new COLLADAScene
                    {
                        instance_visual_scene = new InstanceWithExtra
                        {
                            url = string.Format("#{0}", visualSceneName)
                        }
                    }
                };
                var fileName = string.Format("{0}/dae{1}.dae", selectedPath, i);
                collada.Save(fileName);
            }
        }
    }
}