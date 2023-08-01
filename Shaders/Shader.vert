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
	gl_Position = mvpMatrix * vec4(in_Position, 1.0);
	discardPixel = step(discardValue, in_Position.x);
	vec3 normal = (modelMatrix * vec4(in_Normal, 0.0)).xyz;
	pass_NormalDotLight = clamp(dot(normal, lightDirection), 0.0, 1.0) * lightIntensity;
	pass_NormalLength = length(in_Normal);
	pass_Uv = in_Uv;
	pass_TiledArea = in_TiledArea;
	pass_Ambient = vec4(ambientColor, 1.0);
	if (colorMode == 0) {
		pass_Color = vec4(in_Color, 1.0);
	} else {
		pass_Color = vec4(solidColor, 1.0);
	}
}