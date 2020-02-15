#version 150
in vec2 pass_Uv;
in vec4 pass_Ambient;
in vec4 pass_Color;

in float pass_NormalDotLight;
in float pass_NormalLength;
in float discardPixel;

out vec4 out_Color;

uniform mat4 mvpMatrix;
uniform vec3 lightDirection;
uniform vec3 maskColor;
uniform vec3 ambientColor;
uniform int renderMode;
uniform float lightIntensity;
uniform sampler2D mainTex;

void main(void) {
	if (discardPixel > 0.0) {
		discard;
	}
	vec4 finalColor;
	if (renderMode == 0) {
		vec4 tex2D = texture(mainTex, pass_Uv);		
		if (tex2D.xyz == maskColor) {
			discard;
		}
		finalColor = (pass_Ambient + pass_NormalDotLight) * tex2D * pass_Color;
	} else if (renderMode == 1) {
		vec4 tex2D = texture(mainTex, pass_Uv);		
		if (tex2D.xyz == maskColor) {
			discard;
		}
		finalColor = (pass_Ambient) * tex2D * pass_Color * 2.0;
	} else {
		finalColor = pass_Color;
	}
	out_Color = finalColor; 
}