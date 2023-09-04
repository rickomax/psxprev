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
uniform vec2 uvOffset;
uniform int lightMode;
uniform int colorMode;
uniform int textureMode;
uniform int semiTransparentPass;
uniform float lightIntensity;
uniform sampler2D mainTex;

const vec3 BlackMask = vec3(0.0, 0.0, 0.0);

const vec4 OutOfBoundsTexture_Color1 = vec4(1.0, 0.05, 0.0, 1.0); // Red with slight Orange
const vec4 MissingTexture_Color1     = vec4(1.0, 0.0, 1.0, 1.0); // Magenta
const vec4 InvalidTexture_Color2     = vec4(0.0, 0.0, 0.0, 1.0); // Black
const vec4 InvalidTexture_StpColor2  = vec4(0.5, 0.5, 0.5, 1.0); // Gray
const float InvalidTexture_Step = 8.0 / 256.0;

const int LightMode_AmbientDirectional = 0;
const int LightMode_Ambient            = 1;
const int LightMode_Directional        = 2;
const int LightMode_None               = 3;

const int ColorMode_VertexColor = 0;
const int ColorMode_SolidColor  = 1;

const int TextureMode_Enabled        = 0;
const int TextureMode_Disabled       = 1;
const int TextureMode_MissingTexture = 2;

const int RenderPass_Pass1Opaque                      = 0;
const int RenderPass_Pass2SemiTransparentOpaquePixels = 1;
const int RenderPass_Pass3SemiTransparent             = 2;


vec3 calcUV(vec2 uv) {
	// X,Y is tiled U,V offset and Z,W is tiled U,V wrap size.
	if (pass_TiledArea.z != 0.0) {
		uv.x = pass_TiledArea.x + mod(uv.x, pass_TiledArea.z);
	}
	if (pass_TiledArea.w != 0.0) {
		uv.y = pass_TiledArea.y + mod(uv.y, pass_TiledArea.w);
	}
	float outOfBounds = ((uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0) ? 1.0 : 0.0);
	return vec3(uv.x, uv.y, outOfBounds);
}

vec4 calcInvalidTexture(vec3 uv) {
	bool invalidX = (mod(uv.x, InvalidTexture_Step * 2) >= InvalidTexture_Step);
	bool invalidY = (mod(uv.y, InvalidTexture_Step * 2) >= InvalidTexture_Step);
	if (invalidX == invalidY) {
		// Primary color for even squares
		return (uv.z == 0 ? MissingTexture_Color1 : OutOfBoundsTexture_Color1);
	} else {
		// Secondary color for odd squares
		// Change black squares to gray, so that they show up with semi-transparency
		return (semiTransparentPass != 2 ? InvalidTexture_Color2 : InvalidTexture_StpColor2);
	}
}


void main(void) {
	// Process vertex shader discarding:
	if (discardPixel > 0.0) {
		discard;
	}

	// Process texture UVs and stp bit:
	vec3 uv = calcUV(pass_Uv); // uv.z is non-zero when out-of-bounds
	vec4 tex2D;
	bool stp;
	if (textureMode == 0 && uv.z == 0.0) {
		// Note: I read a comment stating the result of texture() can be undefined
		// when inside an if statement. If issues arise, then try moving this outside.
		tex2D      = texture(mainTex, vec2(uv.x * 0.5,       uv.y));
		vec4 stp2D = texture(mainTex, vec2(uv.x * 0.5 + 0.5, uv.y));
		stp = (stp2D.x != 0.0);
	} else if (textureMode == 1) {
		// Use vertex color only
		tex2D = vec4(1.0, 1.0, 1.0, 1.0);
		stp = true; // Untextured always treats stp bit as set.
	} else {
		// Use missing/out-of-bounds texture checkered pattern
		tex2D = calcInvalidTexture(uv);
		stp = true; // Show semi-transparency and avoid black masking
	}

	// Process semi-transparency discarding:
	if (!stp && tex2D.xyz == maskColor) {
		discard; // Black surfaces are transparent when stp bit is unset.
	} else if (semiTransparentPass == 1) {
		if (stp) {
			discard; // Semi-transparent surface during no-stp bit pass.
		}
	} else if (semiTransparentPass == 2) {
		if (!stp) {
			discard; // Opaque surface during stp bit pass.
		}
	}

	// Process lighting and final color:
	vec4 finalColor = tex2D * pass_Color;
	if (lightMode == 0) {
		finalColor *= (pass_Ambient + pass_NormalDotLight);
	} else if (lightMode == 1) {
		finalColor *= (pass_Ambient) * 2.0;
	} else if (lightMode == 2) {
		finalColor *= (pass_NormalDotLight);
	}
	// No extra logic for lightMode == 3
	out_Color = finalColor;
}