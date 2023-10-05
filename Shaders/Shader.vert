#version 430
//FALLBACK_VERSION 150
#define SSBO_SUPPORTED
#define JOINTS_SUPPORTED

in vec3 in_Position;
in vec3 in_Normal;
in vec3 in_Color;
in vec2 in_Uv;
in vec4 in_TiledArea;
in uvec2 in_Joint; // Always defined in order to handle DiscardPixel

out vec2 pass_Uv;
out vec4 pass_TiledArea;
out vec3 pass_Color;
out float pass_NormalDotLight;
out float pass_NormalLength;
out float pass_DiscardPixel; // > 0.0 when the vertex is jointed, and we want to hide it

#ifdef JOINTS_SUPPORTED
#ifdef SSBO_SUPPORTED
layout(std140, binding = 0) buffer buffer_JointTransforms
{
	mat4 b_JointTransforms[]; // vertex,normal joint pairs
};
#else
#define MAX_JOINTS 512
layout(std140) uniform buffer_JointTransforms
{
	mat4 b_JointTransforms[MAX_JOINTS * 2]; // vertex,normal joint pairs
};
#endif
#endif

uniform sampler2D u_MainTexture;
uniform mat3 u_NormalMatrix;
uniform mat3 u_NormalSpriteMatrix;
uniform mat4 u_ModelMatrix;
uniform mat4 u_ModelSpriteMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;
uniform vec3 u_LightDirection;
uniform vec3 u_MaskColor;
uniform vec3 u_AmbientColor;
uniform vec3 u_SolidColor;
uniform vec2 u_UvOffset;
uniform int u_JointMode;
uniform int u_LightMode;
uniform int u_ColorMode;
uniform int u_TextureMode;
uniform int u_SemiTransparentPass;
uniform float u_LightIntensity;

const uint NO_JOINT = 0u;

const int ColorMode_VertexColor = 0;
const int ColorMode_SolidColor  = 1;

const int JointMode_Enabled  = 0;
const int JointMode_Disabled = 1;
const int JointMode_Hide     = 2;


mat4 getModelMatrix() {
	#ifdef JOINTS_SUPPORTED
	if (u_JointMode == 0 && in_Joint.x != NO_JOINT) {
		return b_JointTransforms[(in_Joint.x - 1u) * 2u];
	}
	#endif
	return u_ModelMatrix;
}

mat3 getNormalMatrix() {
	#ifdef JOINTS_SUPPORTED
	if (u_JointMode == 0 && in_Joint.y != NO_JOINT) {
		return mat3(b_JointTransforms[(in_Joint.y - 1u) * 2u + 1u]);
	}
	#endif
	return u_NormalMatrix;
}


void main(void) {
	// Check for discarded vertices:
	// We only discard jointed vertices for JointMode 2, normals are ignored
	pass_DiscardPixel = mix(0.0, 1.0, u_JointMode == 2 && in_Joint.x != NO_JOINT);

	// Process position:
	mat4 jointModelMatrix = getModelMatrix();
	vec4 worldPosition = jointModelMatrix * (u_ModelSpriteMatrix * vec4(in_Position, 1.0));
	vec4 cameraPosition = u_ViewMatrix * worldPosition;
	gl_Position = u_ProjectionMatrix * cameraPosition;

	// Process normal and directional lighting:
	mat3 jointNormalMatrix = getNormalMatrix();
	pass_NormalLength = length(in_Normal);
	vec3 normal = jointNormalMatrix * (u_NormalSpriteMatrix * in_Normal);
	if (dot(normal, normal) != 0.0) {
		normal = normalize(normal);
		// todo: Should we preserve original normal length?
		//normal *= pass_NormalLength;
	}
	pass_NormalDotLight = clamp(dot(normal, u_LightDirection), 0.0, 1.0) * u_LightIntensity;

	// Process UVs:
	pass_Uv = in_Uv + u_UvOffset;
	pass_TiledArea = in_TiledArea;

	// Process color:
	if (u_ColorMode == 0) {
		pass_Color = in_Color;
	} else {
		pass_Color = u_SolidColor;
	}
}