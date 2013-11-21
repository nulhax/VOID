Shader "Hidden/Composite Camera Shader" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SecondTex ("Base (RGB)", 2D) = "red" {}
	}
		
	CGINCLUDE
	
	#include "UnityCG.cginc"

	struct v2f 
	{ 
		float4 pos	: POSITION;
		float2 uv	: TEXCOORD0;
		float2 uvD	: TEXCOORD1;
	}; 

	uniform sampler2D _MainTex;
	uniform sampler2D _SecondTex;
	
	sampler2D _CameraDepthTexture; 
	
	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
		o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
		o.uvD = o.uv;
		o.uvD.y = 1.0f - o.uvD.y;
		
		return o;
	}
	
	float4 frag(v2f i) : COLOR
	{
		float centerDepth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uvD)));

		float4 ret = centerDepth <= 1.0f ? tex2D(_MainTex, i.uv) : tex2D(_SecondTex, i.uv);

		return centerDepth;
	}
	
	ENDCG
	 
	SubShader 
	{	
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off } 
			
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
	Fallback off
} 