#version 150
in vec2 pass_Uv;
in vec4 pass_TiledArea;
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
uniform int semiTransparentMode;
uniform float lightIntensity;
uniform sampler2D mainTex;

void main(void) {
	if (discardPixel > 0.0) {
		discard;
	}
	vec4 finalColor;
	if (renderMode == 0 || renderMode == 1) {
		vec2 uv = pass_Uv;
		// X,Y is tiled U,V offset and Z,W is tiled U,V wrap size.
		if (pass_TiledArea.z != 0.0) {
			uv.x = pass_TiledArea.x + mod(pass_Uv.x, pass_TiledArea.z);
		}
		if (pass_TiledArea.w != 0.0) {
			uv.y = pass_TiledArea.y + mod(pass_Uv.y, pass_TiledArea.w);
		}
		//if (pass_TiledArea.z != 0.0 && pass_TiledArea.w != 0.0) {
		//	uv = pass_TiledArea.xy + mod(pass_Uv, pass_TiledArea.zw);
		//}

		vec4 tex2D = texture(mainTex, vec2(uv.x * 0.5,       uv.y));
		vec4 stp2D = texture(mainTex, vec2(uv.x * 0.5 + 0.5, uv.y));
		bool stp = (stp2D.x != 0.0);

		if (!stp && tex2D.xyz == maskColor) {
			discard; // Black surfaces are transparent when stp bit is unset.
		} else if (semiTransparentMode == 1) {
			if (stp) {
				discard; // Semi-transparent surface during no-stp bit pass.
			}
		} else if (semiTransparentMode == 2) {
			if (!stp) {
				discard; // Opaque surface during stp bit pass.
			}
		}

		if (renderMode == 0) {
			finalColor = (pass_Ambient + pass_NormalDotLight) * tex2D * pass_Color;
		} else {
			finalColor = (pass_Ambient) * tex2D * pass_Color * 2.0;
		}
	} else {
		finalColor = pass_Color;
	}
	out_Color = finalColor;
}