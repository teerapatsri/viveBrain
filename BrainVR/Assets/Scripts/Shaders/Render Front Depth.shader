Shader "Hidden/Ray Marching/Render Front Depth" {

	CGINCLUDE
		#pragma exclude_renderers xbox360
		#include "UnityCG.cginc"

		// Magic! From UnityCG.cginc
		#define UNITY_TRANSFER_DEPTH(oo) oo = o.pos.zw
		#define UNITY_OUTPUT_DEPTH(i) return i.x/i.y

		struct v2f {
			float4 pos : POSITION;
			float3 localPos : TEXCOORD0;
			float2 depth : TEXCOORD1;
		};

		v2f vert(appdata_base v)
		{
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.localPos = v.vertex.xyz + 0.5;
			UNITY_TRANSFER_DEPTH(o.depth);
			return o;
		}

		half calcDepth(v2f i)
		{
			UNITY_OUTPUT_DEPTH(i.depth);
		}

		half4 frag(v2f i) : COLOR
		{
			half depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(calcDepth(i)));
			return float4(i.localPos, depth);
		}

	ENDCG

	Subshader
	{
		Tags {"RenderType"="Volume"}
		Fog { Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}

	Fallback Off
}
