Shader "Custom/ClipShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_ClipPlane("ClipPlane", Vector) = (0,0,0,0)
	}
	SubShader {
		// Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float4 _ClipPlane;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			clip(dot(_ClipPlane, float4(IN.worldPos, 1)));

			// half NdotL = dot(s.Normal, lightDir);

			// Albedo comes from a texture tinted by color
			// if (IN.facing) {
				o.Albedo = _Color;
			// } else {
			// 	o.Albedo = 0;
			// }
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
