Shader "VOID/TexturedFog"
{
	Properties
	{
		_MainTex	("Base (RGB)", 2D) = "black" {}
		_FogTex		("Base (RGB)", 2D) = "white" {}

		//_Tint		("Tint Color", Color) = (.5, .5, .5, .5)
	}

	SubShader
	{
		Pass
		{
			Cull Off
			Fog { Mode Off }
			//Lighting Off
			ZTest Always
			ZWrite Off
	
			CGINCLUDE

			#include "UnityCG.cginc"
		
			//uniform fixed4 _Tint;
		
			ENDCG
		
			CGPROGRAM
			#pragma vertex DefaultPixelToFogPixel
			#pragma fragment FogFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma exclude_renderers flash
			
			uniform sampler2D _MainTex;
			uniform sampler2D _FogTex;
			uniform sampler2D _CameraDepthTexture;	// Defined by unity.
			uniform float4 _MainTex_TexelSize;	// Defined by unity.
			uniform float _FogDensity;
			uniform float4 _FogStartDistance;
			uniform float4x4 _FrustumCornersWS;	// for fast world space reconstruction
			uniform float4 _CameraWS;			// for fast world space reconstruction
		
			struct DefaultPixel
			{
				float4 vertexPosLocal : POSITION;
				float2 screenUV : TEXCOORD0;
			};
		
			struct FogPixel
			{
				float4 vertexPosProjected : SV_POSITION;
				float2 screenUV : TEXCOORD0;
				float2 depthUV : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};
		
			FogPixel DefaultPixelToFogPixel(DefaultPixel oldPixel)
			{
				FogPixel newPixel;
				half index = oldPixel.vertexPosLocal.z;
				oldPixel.vertexPosLocal.z = 0.1;
				newPixel.vertexPosProjected = mul(UNITY_MATRIX_MVP, oldPixel.vertexPosLocal);
				newPixel.screenUV = oldPixel.screenUV.xy;
				newPixel.depthUV = oldPixel.screenUV.xy;
		
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					newPixel.screenUV.y = 1-newPixel.screenUV.y;
				#endif
		
				newPixel.interpolatedRay = _FrustumCornersWS[(int)index];
				newPixel.interpolatedRay.w = index;
		
				return newPixel;
			}
		
			half4 FogFragment(FogPixel pixel) : COLOR
			{
				float pixelDepthScale = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, pixel.depthUV)));
				float4 camDir = ( /*_CameraWS  + */ pixelDepthScale * pixel.interpolatedRay);
				float fogInt = saturate(length( camDir ) * _FogStartDistance.x - 1.0) * _FogStartDistance.y;
				return lerp(tex2D(_FogTex, float2(pixel.screenUV.x, 1-pixel.screenUV.y)), tex2D(_MainTex, pixel.screenUV), exp(-(_FogDensity+0.001f)*fogInt) /*saturate((0.2f - pixelDepthScale)/(0.2f - 0.15f))*/ );
			}
			ENDCG
		}
	}

	Fallback off
}