#version 150
in vec3 in_Position;
in vec3 in_Color; 
in vec3 in_Normal;
in vec3 in_Uv;
in ivec3 in_Index;

flat out int pass_Selected;
out vec4 pass_Color;
out vec2 pass_Uv;
out float pass_NormalDotLight;
out vec4 pass_Diffuse;
out vec4 pass_Ambient;

uniform int line;
uniform int selectedIndex;
uniform mat4 mvpMatrix;
uniform vec3 lightDirection;
uniform sampler2D mainTex;

const vec4 ambient = vec4(0.5, 0.5, 0.5, 1.0);
const vec4 diffuse = vec4(0.75, 0.75, 0.75, 1.0);

const vec4 white = vec4(1.0, 1.0, 1.0, 1.0);
const vec4 green = vec4(0.0, 1.0, 0.0, 1.0);
const vec4 red = vec4(1.0, 0.0, 0.0, 1.0);

void main(void) {	
    gl_Position = mvpMatrix * vec4(in_Position, 1.0);
	if (line == 1) {
		pass_Color = white;
		pass_Selected = 0;
	} else if (line == 2) {
		pass_Color = green;
		pass_Selected = 0;
	} else {	
		if (selectedIndex == in_Index.x) {			
			pass_Color = red;
			pass_Selected = 1;
		} else {					
			pass_NormalDotLight = clamp(dot(in_Normal, lightDirection), 0.0, 1.0);
			pass_Color = vec4(in_Color, 1.0);
			pass_Uv = in_Uv.st;
			pass_Selected = 0;
			pass_Ambient = ambient;
			pass_Diffuse = diffuse;
		}
	}	
}