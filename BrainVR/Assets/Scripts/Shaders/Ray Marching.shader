Shader "Hidden/Ray Marching/Ray Marching"
{

	CGINCLUDE

	#include "UnityCG.cginc"
	#pragma target 3.0
	#pragma profileoption MaxLocalParams=1024
	#pragma profileoption NumInstructionSlots=4096
	#pragma profileoption NumMathInstructionSlots=4096

	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};

	sampler3D _VolumeTex;
	float4 _VolumeTex_TexelSize;

	sampler2D _FrontTex;
	sampler2D _BackTex;

	float4 _LightDir;
	float4 _LightPos;

	float _Dimensions;

	float _Opacity;
	int _ClippingOption;
	int _ShaderNumber;
	float4 _ClipDims;
	float4 _ClipPlane;

	uniform float4 _TransferFn[101];

	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
#if SHADER_API_D3D9
		if (_MainTex_TexelSize.y < 0)
			o.uv[0].y = 1 - o.uv[0].y;
#endif
		return o;
	}

	#define TOTAL_STEPS 128.0
	#define STEP_CNT 128
	#define STEP_SIZE 1 / 128.0

	uniform sampler2D _CameraDepthTexture;

	half4 raymarch(v2f i, float offset)
	{
		float3 frontPos = tex2D(_FrontTex, i.uv[1]).xyz;
		float frontDepth = tex2D(_FrontTex, i.uv[1]).w;
		float3 backPos = tex2D(_BackTex, i.uv[1]).xyz;
		float backDepth = tex2D(_BackTex, i.uv[1]).w;
		float3 dir = backPos - frontPos;
		float3 pos = frontPos;
		float4 dst = 0;
		float3 stepDist = dir * STEP_SIZE;

		float objectDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv[1])));

		float totalDist = backDepth - frontDepth;
		float seenDist = objectDepth - frontDepth;
		float distRatio = min(seenDist / totalDist, 1.0f);
		float totalStep = lerp(0, STEP_CNT, distRatio);

		for (int k = 0; k < STEP_CNT; k++)
		{
			float4 src = tex3D(_VolumeTex, pos);

			// Standard blending
			if (_ShaderNumber == 1) {
				// GM
				if (src.a + 0.08 >= 0.270588235294118 && src.a + 0.08 <= 0.317647058823529) {
					src.r = 0.04f - 2.5 * (src.a - 0.270588235294118);
					src.g = 0.3952941176f + 1 * (src.a - 0.270588235294118);
					src.b = 0.8f - 1 * (src.a - 0.270588235294118);
				}
				else {
					src.r = 0.0f;
					src.g = 0.0f;
					src.b = 0.0f;
				}
			}
			else if (_ShaderNumber == 2) {
				// WM
				if (src.a + 0.08 >= 0.317647058823529 && src.a + 0.08 <= 0.380392156862745) {
					src.r = 1.0f - 4 * (src.a - 0.317647058823529);
					src.g = 0.7882352941f + 1 * (src.a - 0.317647058823529);
					src.b = 0.2823529412f - 1 * (src.a - 0.317647058823529);
					src.a = 0.02f;
				}
				else if (src.a + 0.08 >= 0.380392156862745 && src.a + 0.08 <= 0.423529411764706) {
					src.r = 1.0f - 4 * (src.a - 0.380392156862745);
					src.g = 0.7882352941f + 1 * (src.a - 0.380392156862745);
					src.b = 0.2823529412f - 1 * (src.a - 0.380392156862745);
					src.a = 0.05f;
				}
				else {
					src.r = 0.0f;
					src.g = 0.0f;
					src.b = 0.0f;
					// src.a = 0.0f;
				}
			}
			else if (_ShaderNumber == 3) {
				//Corpus Collosum
				if (src.a + 0.08 > 0.423529411764706 && src.a + 0.08 <= 0.47843137254902) {
					src.r = 1.0f - 4 * (src.a - 0.423529411764706);
					src.g = 0.3607843137f + 1 * (src.a - 0.423529411764706);
					src.b = 0.5411764706f - 1 * (src.a - 0.423529411764706);
				}
				else {
					src.r = 0.0f;
					src.g = 0.0f;
					src.b = 0.0f;
				}
			}
			else if (_ShaderNumber == 4) {
				// GM & WM
				if (src.a + 0.08 >= 0.270588235294118 && src.a + 0.08 <= 0.317647058823529) {
					src.r = 0.04f - 2.5 * (src.a - 0.270588235294118);
					src.g = 0.3952941176f + 1 * (src.a - 0.270588235294118);
					src.b = 0.8f - 1 * (src.a - 0.270588235294118);
				}
				else if (src.a + 0.08 >= 0.317647058823529 && src.a + 0.08 <= 0.380392156862745) {
					src.r = 1.0f - 4 * (src.a - 0.317647058823529);
					src.g = 0.7882352941f + 1 * (src.a - 0.317647058823529);
					src.b = 0.2823529412f - 1 * (src.a - 0.317647058823529);
					src.a = 0.02f;
				}
				else if (src.a + 0.08 >= 0.380392156862745 && src.a + 0.08 <= 0.423529411764706) {
					src.r = 1.0f - 4 * (src.a - 0.380392156862745);
					src.g = 0.7882352941f + 1 * (src.a - 0.380392156862745);
					src.b = 0.2823529412f - 1 * (src.a - 0.380392156862745);
					src.a = 0.05f;
				}
				else if (src.a + 0.08 < 0.27058823529411 && src.a + 0.08 > 0.245588235294118) {
					src.r = 0.015f;
					src.g = 0.015f;
					src.b = 0.015f;
					src.a = 0.033f;
				}
				else {
					src.r = 0.00f;
					src.b = 0.00f;
					src.g = 0.00f;
				}

			}
			else if (_ShaderNumber == 5) {
				//WM & Corpus
				if (src.a + 0.08 > 0.423529411764706 && src.a + 0.08 <= 0.47843137254902) {
					src.r = 1.0f - 4 * (src.a - 0.423529411764706);
					src.g = 0.3607843137f + 1 * (src.a - 0.423529411764706);
					src.b = 0.5411764706f - 1 * (src.a - 0.423529411764706);
				}
				else if (src.a + 0.08 >= 0.317647058823529 && src.a + 0.08 <= 0.380392156862745) {
					src.r = 1.0f - 4 * (src.a - 0.317647058823529);
					src.g = 0.7882352941f + 1 * (src.a - 0.317647058823529);
					src.b = 0.2823529412f - 1 * (src.a - 0.317647058823529);
					src.a = 0.02f;
				}
				else if (src.a + 0.08 >= 0.380392156862745 && src.a + 0.08 <= 0.423529411764706) {
					src.r = 1.0f - 4 * (src.a - 0.380392156862745);
					src.g = 0.7882352941f + 1 * (src.a - 0.380392156862745);
					src.b = 0.2823529412f - 1 * (src.a - 0.380392156862745);
					src.a = 0.05f;
				}
				else {
					src.r = 0.0f;
					src.g = 0.0f;
					src.b = 0.0f;
					// src.a = 0.0f;
				}
			}
			else if (_ShaderNumber == 6) {
				//GM & Corpus
				if (src.a + 0.08 > 0.423529411764706 && src.a + 0.08 <= 0.47843137254902) {
					src.r = 1.0f - 4 * (src.a - 0.423529411764706);
					src.g = 0.3607843137f + 1 * (src.a - 0.423529411764706);
					src.b = 0.5411764706f - 1 * (src.a - 0.423529411764706);
				}
				else if (src.a + 0.08 >= 0.270588235294118 && src.a + 0.08 <= 0.317647058823529) {
					src.r = 0.04f - 2.5 * (src.a - 0.270588235294118);
					src.g = 0.3952941176f + 1 * (src.a - 0.270588235294118);
					src.b = 0.8f - 1 * (src.a - 0.270588235294118);
				}
				else {
					src.r = 0.0f;
					src.g = 0.0f;
					src.b = 0.0f;
					// src.a = 0.0f;
				}
			}
			else if (_ShaderNumber == 7) {
				// Combo All
				if (src.a + 0.08 > 0.423529411764706 && src.a + 0.08 <= 0.47843137254902) {
					// pink
					src.r = 1.0f - 4 * (src.a - 0.423529411764706);
					src.g = 0.3607843137f + 1 * (src.a - 0.423529411764706);
					src.b = 0.5411764706f - 1 * (src.a - 0.423529411764706);
				}
				else if (src.a + 0.08 >= 0.270588235294118 && src.a + 0.08 <= 0.317647058823529) {
					// blue
					src.r = 0.04f - 2.5 * (src.a - 0.270588235294118);
					src.g = 0.3952941176f + 1 * (src.a - 0.270588235294118);
					src.b = 0.8f - 1 * (src.a - 0.270588235294118);
				}
				else if (src.a + 0.08 >= 0.317647058823529 && src.a + 0.08 <= 0.380392156862745) {
					// yellow
					src.r = 1.0f - 4 * (src.a - 0.317647058823529);
					src.g = 0.7882352941f + 1 * (src.a - 0.317647058823529);
					src.b = 0.2823529412f - 1 * (src.a - 0.317647058823529);
					src.a = 0.02f;
				}
				else if (src.a + 0.08 >= 0.380392156862745 && src.a + 0.08 <= 0.423529411764706) {
					// yellow
					src.r = 1.0f - 4 * (src.a - 0.380392156862745);
					src.g = 0.7882352941f + 1 * (src.a - 0.380392156862745);
					src.b = 0.2823529412f - 1 * (src.a - 0.380392156862745);
					src.a = 0.05f;
				}
				else if (src.a + 0.08 < 0.27058823529411 && src.a + 0.08 > 0.245588235294118) {
					src.r = 0.015f;
					src.g = 0.015f;
					src.b = 0.015f;
					src.a = 0.033f;
				}
				else {
					src.r = 0.00f;
					src.b = 0.00f;
					src.g = 0.00f;
				}
			}
			else {
				if (src.a + 0.08 < 0.245588235294118 || src.a + 0.08 > 0.47843137254902) {
					src.r = 0.00f;
					src.g = 0.00f;
					src.b = 0.00f;
				}
			}

			// obstacle removing
			if (k > totalStep) {
				src.a = 0;
			}

			// plane clipping
			// if (dot(_ClipPlane, float4(pos - 0.5, 1)) + _ClipPlane.w < 0 || (_ClippingOption == 1 && dot(_ClipPlane, float4(pos - 0.6, 1)) + _ClipPlane.w > 0)) {
			if (dot(_ClipPlane, float4((pos - 0.5) * 2.0f, 1)) + _ClipPlane.w < 0 || (_ClippingOption == 1 && dot(_ClipPlane, float4((pos - 0.5) * 2.0f - 0.05f, 1)) + _ClipPlane.w > 0)) {
				src.a = 0;
			}

			// box clipping
			float border = step(1 - _ClipDims.x, pos.x);
			border *= step(pos.y, _ClipDims.y);
			border *= step(pos.z, _ClipDims.z);
			src.a *= border;

			src.rgb *= src.a;
			dst = (1.0f - dst.a) * src + dst;

			pos += stepDist;
		}

		return dst + dst;
	}

	ENDCG

	Subshader
	{
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		Pass
		{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				half4 frag(v2f i) : COLOR{ return raymarch(i, 0); }
			ENDCG
		}
	}

	Fallback off

} // shader