#version 150
in vec4 pass_Color;
in vec2 pass_Uv;
in vec4 pass_Diffuse;
in vec4 pass_Ambient;
in float pass_NormalDotLight;

out vec4 out_Color;

uniform mat4 mvpMatrix;
uniform vec3 lightDirection;
uniform sampler2D mainTex;

void main(void) {	
	vec4 finalColor;
	vec4 diffuseNorm = pass_Diffuse * pass_NormalDotLight;
	vec4 tex2D = texture(mainTex, pass_Uv) * pass_Color;
	finalColor =  (pass_Ambient + diffuseNorm) * tex2D;
	out_Color = finalColor; 
}