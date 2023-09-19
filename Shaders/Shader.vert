#version 430
//FALLBACK_VERSION 150
#define JOINTS_SUPPORTED

in vec3 in_Position;
in vec3 in_Color;
in vec3 in_Normal;
in vec2 in_Uv;
in vec4 in_TiledArea;
#ifdef JOINTS_SUPPORTED
in uvec2 in_Joint;

layout(std140, binding = 0) buffer jointBuffer
{
	mat4 joints[]; // vertex,normal joint pairs
};
#endif

out vec2 pass_Uv;
out vec4 pass_TiledArea;
out vec4 pass_Ambient;
out vec4 pass_Color;
out float pass_NormalDotLight;
out float pass_NormalLength;
out float pass_DiscardPixel;

uniform mat3 normalMatrix;
uniform mat4 modelMatrix;
uniform mat4 mvpMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;
uniform vec3 lightDirection;
uniform vec3 maskColor;
uniform vec3 ambientColor;
uniform vec3 solidColor;
uniform vec2 uvOffset;
uniform int jointMode;
uniform int lightMode;
uniform int colorMode;
uniform int textureMode;
uniform int semiTransparentPass;
uniform float lightIntensity;
uniform sampler2D mainTex;

const float DISCARD_VALUE = 100000000;
const uint NO_JOINT = 0u;

const int ColorMode_VertexColor = 0;
const int ColorMode_SolidColor  = 1;

const int JointMode_Enabled  = 0;
const int JointMode_Disabled = 1;

mat4 getModelMatrix() {
	#ifdef JOINTS_SUPPORTED
	if (jointMode == 0 && in_Joint.x != NO_JOINT) {
		return joints[(in_Joint.x - 1) * 2];
	}
	#endif
	return modelMatrix;
}

mat3 getNormalMatrix() {
	#ifdef JOINTS_SUPPORTED
	if (jointMode == 0 && in_Joint.y != NO_JOINT) {
		return mat3(joints[(in_Joint.y - 1) * 2 + 1]);
	}
	#endif
	return normalMatrix;
}

void main(void) {
	// Check for discarded vertices (coordinates match DISCARD_VALUE):
	pass_DiscardPixel = step(DISCARD_VALUE, in_Position.x);

	// Process position:
	#ifdef JOINTS_SUPPORTED
	mat4 jointModelMatrix = getModelMatrix();
	vec4 worldPosition = jointModelMatrix * vec4(in_Position, 1.0);
	vec4 cameraPosition = viewMatrix * worldPosition;
	gl_Position = projectionMatrix * cameraPosition;
	#else
	// Skip combining matrices and just use pre-defined mvpMatrix when joints aren't in use
	gl_Position = mvpMatrix * vec4(in_Position, 1.0);
	#endif

	// Process normal and directional lighting:
	mat3 jointNormalMatrix = getNormalMatrix();
	pass_NormalLength = length(in_Normal);
	vec3 normal = jointNormalMatrix * in_Normal;
	if (dot(normal, normal) != 0.0) {
		normal = normalize(normal);
		// todo: Should we preserve original normal length?
		//normal *= pass_NormalLength;
	}
	pass_NormalDotLight = clamp(dot(normal, lightDirection), 0.0, 1.0) * lightIntensity;

	// Process UVs:
	pass_Uv = in_Uv + uvOffset;
	pass_TiledArea = in_TiledArea;

	// Process color:
	pass_Ambient = vec4(ambientColor, 1.0);
	if (colorMode == 0) {
		pass_Color = vec4(in_Color, 1.0);
	} else {
		pass_Color = vec4(solidColor, 1.0);
	}
}