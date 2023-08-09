#version 150
in vec2 pass_Uv;
in vec4 pass_TiledArea;
in vec4 pass_Ambient;
in vec4 pass_Color;

in float pass_NormalDotLight;
in float pass_NormalLength;
in float discardPixel;

out vec4 out_Color;

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

void main(void) {
	if (discardPixel > 0.0) {
		discard;
	}

	// Process texture UVs and stp bit
	vec4 tex2D;
	int stp;
	if (textureMode == 0) {
		vec2 uv = pass_Uv;
		// X,Y is tiled U,V offset and Z,W is tiled U,V wrap size.
		if (pass_TiledArea.z != 0.0) {
			uv.x = pass_TiledArea.x + mod(pass_Uv.x, pass_TiledArea.z);
		}
		if (pass_TiledArea.w != 0.0) {
			uv.y = pass_TiledArea.y + mod(pass_Uv.y, pass_TiledArea.w);
		}

		tex2D      = texture(mainTex, vec2(uv.x * 0.5,       uv.y));
		vec4 stp2D = texture(mainTex, vec2(uv.x * 0.5 + 0.5, uv.y));
		stp = (stp2D.x == 0.0 ? 0 : 1);
	} else {
		tex2D = vec4(1.0, 1.0, 1.0, 1.0);
		stp = 1; // Untextured always treats stp bit as set.
	}

	// Process semi-transparency discarding
	if (stp == 0 && tex2D.xyz == maskColor) {
		discard; // Black surfaces are transparent when stp bit is unset.
	} else if (semiTransparentPass == 1) {
		if (stp != 0) {
			discard; // Semi-transparent surface during no-stp bit pass.
		}
	} else if (semiTransparentPass == 2) {
		if (stp == 0) {
			discard; // Opaque surface during stp bit pass.
		}
	}

	// Process lighting and final color
	vec4 finalColor = tex2D * pass_Color;
	if (lightMode == 0) {
		finalColor *= (pass_Ambient + pass_NormalDotLight);
	} else if (lightMode == 1) {
		finalColor *= (pass_Ambient) * 2.0;
	} else if (lightMode == 2) {
		finalColor *= (pass_NormalDotLight);
	}
	out_Color = finalColor;
}