Shader "VOID/GalaxyShader"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "black" {}

		_Blend("Blend", Range(0.0,1.0)) = 0.0
		_Tint ("Tint Color", Color) = (.5, .5, .5, .5)

		_FrontTex1 ("Front1 (+Z)", 2D) = "white" {}
		_BackTex1 ("Back1 (-Z)", 2D) = "white" {}
		_LeftTex1 ("Left1 (+X)", 2D) = "white" {}
		_RightTex1 ("Right1 (-X)", 2D) = "white" {}
		_UpTex1 ("Up1 (+Y)", 2D) = "white" {}
		_DownTex1 ("Down1 (-Y)", 2D) = "white" {}
		_FrontTex2 ("Front2 (+Z)", 2D) = "white" {}
		_BackTex2 ("Back2 (-Z)", 2D) = "white" {}
		_LeftTex2 ("Left2 (+X)", 2D) = "white" {}
		_RightTex2 ("Right2 (-X)", 2D) = "white" {}
		_UpTex2 ("Up2 (+Y)", 2D) = "white" {}
		_DownTex2 ("Down2 (-Y)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off
		Fog { Mode Off }
		Lighting Off
		ZWrite Off
	
		CGINCLUDE

		#include "UnityCG.cginc"

		uniform sampler2D _CameraDepthTexture;	// Defined by unity.
		uniform float4 _MainTex_TexelSize;	// Defined by unity.

		uniform fixed4 _Tint;
		uniform float _FogDensity;
		uniform float4 _FogColor;
		uniform float4 _FogStartDistance;
		uniform float4x4 _FrustumCornersWS;	// for fast world space reconstruction
		uniform float4 _CameraWS;			// for fast world space reconstruction
	
		struct DefaultPixel
		{
			float4 vertexPosLocal : POSITION;
			float2 screenUV : TEXCOORD0;
		};

		struct SkyboxPixel
		{
			float4 vertexPosProjected : SV_POSITION;
			float2 screenUV : TEXCOORD0;
		};

		struct FogPixel
		{
			float4 vertexPosProjected : SV_POSITION;
			float2 screenUV : TEXCOORD0;
			float2 depthUV : TEXCOORD1;
			float4 interpolatedRay : TEXCOORD2;
		};

		SkyboxPixel DefaultPixelToSkyboxPixel(DefaultPixel oldPixel)
		{
			SkyboxPixel newPixel;
			newPixel.vertexPosProjected = mul(UNITY_MATRIX_MVP, oldPixel.vertexPosLocal);
			newPixel.screenUV = oldPixel.screenUV;
			return newPixel;
		}

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

		fixed4 SkyboxFragment (SkyboxPixel pixel, sampler2D skyboxFace)
		{
			fixed4 diffuseSample = tex2D(skyboxFace, pixel.screenUV);
			fixed4 colour;
			colour.rgb = diffuseSample.rgb + _Tint.rgb - unity_ColorSpaceGrey;
			colour.a = diffuseSample.a * _Tint.a;
			return colour;
		}

		half4 FogFragment(FogPixel pixel, uniform sampler2D mainTex)
		{
			float pixelDepthScale = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, pixel.depthUV)));
			float4 camDir = ( /*_CameraWS  + */ pixelDepthScale * pixel.interpolatedRay);
			float fogInt = saturate(length( camDir ) * _FogStartDistance.x - 1.0) * _FogStartDistance.y;
			return lerp(_FogColor, tex2D(mainTex, pixel.screenUV), exp(-_FogDensity*fogInt));
		}

		ENDCG
	
		// Skybox passes.
		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _FrontTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _FrontTex1); }
			ENDCG 
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _BackTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _BackTex1); }
			ENDCG 
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _LeftTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _LeftTex1); }
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _RightTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _RightTex1); }
			ENDCG
		}
			
		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _UpTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _UpTex1); }
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex DefaultPixelToSkyboxPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			sampler2D _DownTex1;
			fixed4 InternalFragment (SkyboxPixel pixel) : COLOR { return SkyboxFragment(pixel, _DownTex1); }
			ENDCG
		}

		// Fog pass.
		Pass
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }

			CGPROGRAM
			#pragma vertex DefaultPixelToFogPixel
			#pragma fragment InternalFragment
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma exclude_renderers flash
			uniform sampler2D _MainTex;
			fixed4 InternalFragment (FogPixel pixel) : COLOR { return FogFragment(pixel, _MainTex); }
			ENDCG
		}
	}	

	SubShader
	{
		Tags { "Queue"="Background" "RenderType"="Background" }
		Cull Off
		Fog { Mode Off }
		Lighting Off
		ZWrite Off

		Color [_Tint]	// "primary"
		Pass
		{
			SetTexture [_FrontTex1] { combine texture }
			SetTexture [_FrontTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_FrontTex2] { combine previous +- primary, previous * primary }
		}
		Pass
		{
			SetTexture [_BackTex1] { combine texture }
			SetTexture [_BackTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_BackTex2] { combine previous +- primary, previous * primary }
		}
		Pass
		{
			SetTexture [_LeftTex1] { combine texture }
			SetTexture [_LeftTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_LeftTex2] { combine previous +- primary, previous * primary }
		}
		Pass
		{
			SetTexture [_RightTex1] { combine texture }
			SetTexture [_RightTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_RightTex2] { combine previous +- primary, previous * primary }
		}
		Pass
		{
			SetTexture [_UpTex1] { combine texture }
			SetTexture [_UpTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_UpTex2] { combine previous +- primary, previous * primary }
		}
		Pass
		{
			SetTexture [_DownTex1] { combine texture }
			SetTexture [_DownTex2] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
			SetTexture [_DownTex2] { combine previous +- primary, previous * primary }
		}
	}

	Fallback off
}