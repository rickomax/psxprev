using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using PSXPrev.Common.Utils;

namespace PSXPrev.Common.Exporters.glTF2Schema
{
    // Use the following attribute on properties that should be written, even when null.
    // [JsonProperty(NullValueHandling = NullValueHandling.Include)]

    internal class glTF
    {
        public const string ExtensionUnlit = "KHR_materials_unlit";

        public asset asset;
        public int scene;
        public List<scene> scenes;
        public List<bufferView> bufferViews;
        public List<sampler> samplers;
        public List<image> images;
        public List<texture> textures;
        public List<material> materials;
        public List<accessor> accessors;
        public List<mesh> meshes;
        public List<skin> skins;
        //public List<camera> cameras;
        public List<animation> animations;
        public List<node> nodes;
        public List<buffer> buffers;
        public List<string> extensionsUsed;
    }


    internal class accessor
    {
        public string name;
        public int bufferView;
        [DefaultValue(0L), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long byteOffset = 0;
        // Serialize as int
        public accessor_componentType componentType;
        [DefaultValue(false), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool normalized = false;
        public int count;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public accessor_type type;
        public float[] min;
        public float[] max;
        //public animation_sparse sparse;
    }

    internal enum accessor_componentType
    {
        BYTE           = 5120,
        UNSIGNED_BYTE  = 5121,
        SHORT          = 5122,
        UNSIGNED_SHORT = 5123,
        UNSIGNED_INT   = 5125,
        FLOAT          = 5126,
    }

    internal enum accessor_type
    {
        SCALAR,
        VEC2,
        VEC3,
        VEC4,
        MAT2,
        MAT3,
        MAT4,
    }

    internal class animation
    {
        public string name;
        public List<animation_sampler> samplers;
        public List<animation_channel> channels;
    }

    internal class animation_channel
    {
        public int sampler;
        public animation_channel_target target;
    }

    internal class animation_channel_target
    {
        public int node;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public animation_channel_target_path path;
    }

    internal enum animation_channel_target_path
    {
        translation,
        rotation,
        scale,
        weights,
    }

    internal class animation_sampler
    {
        public int input;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [DefaultValue(animation_sampler_interpolation.LINEAR), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public animation_sampler_interpolation interpolation = animation_sampler_interpolation.LINEAR;
        public int output;
    }

    internal enum animation_sampler_interpolation
    {
        LINEAR,
        STEP,
        CUBICSPLINE,
    }

    internal class asset
    {
        public string copyright;
        public string generator;
        public string version;
        public string minVersion;
    }

    internal class buffer
    {
        public string name;
        public string uri;
        public long byteLength;
    }

    internal class bufferView
    {
        public string name;
        public int buffer;
        [DefaultValue(0L), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long byteOffset = 0;
        public long byteLength;
        public long? byteStride;
        public bufferView_target? target;
    }

    internal enum bufferView_target
    {
        ARRAY_BUFFER         = 34962,
        ELEMENT_ARRAY_BUFFER = 34963,
    }

    internal class image
    {
        public string name;
        public string uri;
    }

    internal class material
    {
        public string name;
        public material_pbrMetallicRoughness pbrMetallicRoughness;
        public material_normalTextureInfo normalTexture;
        public material_occlusionTextureInfo occlusionTexture;
        public textureInfo emissiveTexture;
        public float[] emissiveFactor;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [DefaultValue(material_alphaMode.OPAQUE), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public material_alphaMode alphaMode = material_alphaMode.OPAQUE;
        [DefaultValue(0.5f), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float alphaCutoff = 0.5f;
        [DefaultValue(false), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool doubleSided = false;
        public Dictionary<string, object> extensions;
        // extensions: KHR_materials_unlit
    }

    internal enum material_alphaMode
    {
        OPAQUE,
        MASK,
        BLEND,
    }

    internal class material_normalTextureInfo : textureInfo
    {
        [DefaultValue(1.0f), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float scale = 1.0f;
    }

    internal class material_occlusionTextureInfo : textureInfo
    {
        [DefaultValue(1.0f), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float strength = 1.0f;
    }

    internal class material_pbrMetallicRoughness
    {
        public float[] baseColorFactor;
        public textureInfo baseColorTexture;
        [DefaultValue(1.0f), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float metallicFactor = 1.0f;
        [DefaultValue(1.0f), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public float roughnessFactor = 1.0f;
        public textureInfo metallicRoughnessTexture;
    }

    internal class mesh
    {
        public string name;
        public List<mesh_primitive> primitives;
        public List<float> weights;
    }

    internal class mesh_primitive
    {
        public mesh_primitive_attributes attributes;
        public int? indices;
        public int? material;
        [DefaultValue(mesh_primitive_mode.TRIANGLES), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public mesh_primitive_mode mode = mesh_primitive_mode.TRIANGLES;
        public List<int> targets;
    }

    internal class mesh_primitive_attributes
    {
        public int POSITION;
        public int? NORMAL;
        public int? COLOR_0;
        public int? TEXCOORD_0;
        public int? JOINTS_0;
        public int? WEIGHTS_0;
    }

    internal enum mesh_primitive_mode
    {
        POINTS         = 0,
        LINES          = 1,
        LINE_LOOP      = 2,
        LINE_STRIP     = 3,
        TRIANGLES      = 4,
        TRIANGLE_STRIP = 5,
        TRIANGLE_FAN   = 6,
    }

    internal class node
    {
        public string name;
        public List<int> children;
        public int? camera;
        public int? mesh;
        public int? skin;
        public float[] translation;
        public float[] rotation;
        public float[] scale;
        public float[] matrix;
        public List<float> weights;
    }

    internal class sampler
    {
        public string name;
        public sampler_filter magFilter;
        public sampler_filter minFilter;
        [DefaultValue(sampler_wrap.REPEAT), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public sampler_wrap wrapS = sampler_wrap.REPEAT;
        [DefaultValue(sampler_wrap.REPEAT), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public sampler_wrap wrapT = sampler_wrap.REPEAT;
    }

    internal enum sampler_filter
    {
        NEAREST                = 9728,
        LINEAR                 = 9729,
        // minFilter only
        NEAREST_MIPMAP_NEAREST = 9984,
        LINEAR_MIPMAP_NEAREST  = 9985,
        NEAREST_MIPMAP_LINEAR  = 9986,
        LINEAR_MIPMAP_LINEAR   = 9987,
    }

    internal enum sampler_wrap
    {
        REPEAT          = 10497,
        CLAMP_TO_EDGE   = 33071,
        MIRRORED_REPEAT = 33648,
    }

    internal class scene
    {
        public string name;
        public List<int> nodes;
    }

    internal class skin
    {
        public string name;
        public List<int> joints;
        public int? inverseBindMatrices;
        public int? skeleton;
    }

    internal class texture
    {
        public string name;
        public int sampler;
        public int source;
    }

    internal class textureInfo
    {
        public int index;
        [DefaultValue(0), JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int texCoord = 0;
    }
}
