#version 150

in vec2 pass_Uv;
in vec4 pass_TiledArea;
in vec3 pass_Color;
in float pass_NormalDotLight;
in float pass_NormalLength;
in float pass_DiscardPixel;

out vec4 out_Color;

uniform sampler2D u_MainTexture;
uniform vec3 u_LightDirection;
uniform vec3 u_MaskColor;
uniform vec3 u_AmbientColor;
uniform vec3 u_SolidColor;
uniform vec2 u_UvOffset;
uniform int u_LightMode;
uniform int u_ColorMode;
uniform int u_TextureMode;
uniform int u_SemiTransparentPass;
uniform float u_LightIntensity;

const vec3 BlackMask = vec3(0.0);

const vec3 OutOfBoundsTexture_Color1 = vec3(1.0, 0.05, 0.0); // Red with slight Orange
const vec3 MissingTexture_Color1     = vec3(1.0, 0.0, 1.0); // Magenta
const vec3 InvalidTexture_Color2     = vec3(0.0, 0.0, 0.0); // Black
const vec3 InvalidTexture_StpColor2  = vec3(0.5, 0.5, 0.5); // Gray
//const float InvalidTexture_Step = 8.0 / 256.0;
const vec2 InvalidTexture_Step = vec2((8.0 / 256.0) / 4.0, (8.0 / 256.0) / 8.0); // Divided by number of texture page columns, rows

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
	// We only add the offset and wrap a component if the wrap size is non-zero.
	uv = mix(uv, pass_TiledArea.xy + mod(uv, pass_TiledArea.zw), bvec2(pass_TiledArea.zw));
	float outOfBounds = mix(0.0, 1.0, any(lessThan(uv, vec2(0.0))) || any(greaterThan(uv, vec2(1.0))));
	// Readable version of above:
	//if (pass_TiledArea.z != 0.0) {
	//	uv.x = pass_TiledArea.x + mod(uv.x, pass_TiledArea.z);
	//}
	//if (pass_TiledArea.w != 0.0) {
	//	uv.y = pass_TiledArea.y + mod(uv.y, pass_TiledArea.w);
	//}
	//float outOfBounds = ((uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0) ? 1.0 : 0.0);
	return vec3(uv, outOfBounds);
}

vec3 calcInvalidTexture(vec3 uv) {
	// Check if we're in an even or odd square
	bvec2 evenOdd = greaterThanEqual(mod(uv.xy, InvalidTexture_Step * 2.0), InvalidTexture_Step);
	if (evenOdd.x == evenOdd.y) {
		// Primary color for even squares
		return (uv.z == 0.0 ? MissingTexture_Color1 : OutOfBoundsTexture_Color1);
	} else {
		// Secondary color for odd squares
		// Change black squares to gray, so that they show up with semi-transparency
		return (u_SemiTransparentPass != 2 ? InvalidTexture_Color2 : InvalidTexture_StpColor2);
	}
}


void main(void) {
	// Process vertex shader discarding:
	if (pass_DiscardPixel > 0.0) {
		discard;
	}

	// Process texture UVs and stp bit:
	vec3 uv = calcUV(pass_Uv); // uv.z is non-zero when out-of-bounds
	vec3 texColor;
	bool stp;
	if (u_TextureMode == 0 && uv.z == 0.0) {
		// Note: I read a comment stating the result of texture() can be undefined
		// when inside an if statement. If issues arise, then try moving this outside.
		texColor = texture(u_MainTexture, vec2(uv.x * 0.5,       uv.y)).xyz;
		stp      = texture(u_MainTexture, vec2(uv.x * 0.5 + 0.5, uv.y)).x != 0.0;
	} else if (u_TextureMode == 1) {
		// Use vertex color only
		texColor = vec3(1.0);
		stp = true; // Untextured always treats stp bit as set.
	} else {
		// Use missing/out-of-bounds texture checkered pattern
		texColor = calcInvalidTexture(uv);
		stp = true; // Show semi-transparency and avoid black masking
	}

	// Process semi-transparency discarding:
	if (!stp && texColor == u_MaskColor) {
		discard; // Black surfaces are transparent when stp bit is unset.
	} else if (stp && u_SemiTransparentPass == 1) {
		discard; // Semi-transparent surface during no-stp bit pass.
	} else if (!stp && u_SemiTransparentPass == 2) {
		discard; // Opaque surface during stp bit pass.
	}

	// Process lighting and final color:
	vec3 finalColor = texColor * pass_Color;
	if (u_LightMode == 0) {
		finalColor *= (u_AmbientColor + pass_NormalDotLight);
	} else if (u_LightMode == 1) {
		finalColor *= (u_AmbientColor) * 2.0;
	} else if (u_LightMode == 2) {
		finalColor *= (pass_NormalDotLight);
	}
	// No extra logic for lightMode == 3
	out_Color = vec4(finalColor, 1.0);
}