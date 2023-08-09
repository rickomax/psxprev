#version 150
in vec3 in_Position;
in vec3 in_Color;
in vec3 in_Normal;
in vec2 in_Uv;
in vec4 in_TiledArea;

out vec2 pass_Uv;
out vec4 pass_TiledArea;
out vec4 pass_Ambient;
out vec4 pass_Color;
out float pass_NormalDotLight;
out float pass_NormalLength;
out float discardPixel;

uniform mat3 normalMatrix;
uniform mat4 modelMatrix;
uniform mat4 mvpMatrix;
uniform vec3 lightDirection;
uniform vec3 maskColor;
uniform vec3 ambientColor;
uniform vec3 solidColor;
uniform int lightMode;
uniform int colorMode;
uniform int textureMode;
uniform int semiTransparentPass;
uniform float lightIntensity;
uniform sampler2D mainTex;

const float discardValue = 100000000;

void main(void) {
	// Check for discarded vertices (coordinates match discardValue)
	discardPixel = step(discardValue, in_Position.x);

	// Process position
	gl_Position = mvpMatrix * vec4(in_Position, 1.0);

	// Process normal and directional lighting
	pass_NormalLength = length(in_Normal);
	vec3 normal = normalMatrix * in_Normal;
	if (dot(normal, normal) != 0.0) {
		normal = normalize(normal);
		// todo: Should we preserve original normal length?
		//normal *= pass_NormalLength;
	}
	pass_NormalDotLight = clamp(dot(normal, lightDirection), 0.0, 1.0) * lightIntensity;

	// Process UVs
	pass_Uv = in_Uv;
	pass_TiledArea = in_TiledArea;

	// Process color
	pass_Ambient = vec4(ambientColor, 1.0);
	if (colorMode == 0) {
		pass_Color = vec4(in_Color, 1.0);
	} else {
		pass_Color = vec4(solidColor, 1.0);
	}
}