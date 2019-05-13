#version 150
in vec4 pass_Color;
in vec2 pass_Uv;
in vec4 pass_Diffuse;
in vec4 pass_Ambient;
in float pass_NormalDotLight;
in float pass_NormalLength;

out vec4 out_Color;

uniform sampler2D mainTex;

void main(void) {	
	vec4 finalColor;
	if (pass_NormalLength == 0.0) {
		finalColor = pass_Color;
	} else {
		vec4 diffuseNorm = pass_Diffuse * pass_NormalDotLight;
		vec4 tex2D = texture(mainTex, pass_Uv) * pass_Color;
		finalColor = (pass_Ambient + diffuseNorm) * tex2D;
	}
	out_Color = finalColor; 
}