Shader "VOID/Bumped Specular"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		CGINCLUDE
		#include"VoidShaderVariables.cginc"
		ENDCG
	
		CGPROGRAM
		#pragma surface surf BlinnPhong finalcolor:FogPass
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;
		half _Shininess;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
			float4 screenPos;
		};

		void FogPass(Input IN, SurfaceOutput o, inout fixed4 colour) {colour = void_SampleFog(IN.screenPos, IN.worldPos, colour);}

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Gloss = tex.a;
			o.Alpha = _Color.a;
			o.Specular = _Shininess;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}

		ENDCG
	}

	FallBack "Specular"
}
