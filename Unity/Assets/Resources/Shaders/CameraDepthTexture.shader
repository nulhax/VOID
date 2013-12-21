Shader "Hidden/CameraDepthTexture" 
{
	Properties 
	{
		_MainTex ("", 2D) = "black" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f 
	{
	    float4 pos : POSITION;
	    #ifdef UNITY_MIGHT_NOT_HAVE_DEPTH_TEXTURE
	    float2 depth : TEXCOORD0;
	    #endif
	};
	
	v2f vert( appdata_base v ) 
	{
	    v2f o;
	    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
	    UNITY_TRANSFER_DEPTH(o.depth);
	    return o;
	}
	
	float4 fragOpaque(v2f i) : COLOR 
	{
		UNITY_OUTPUT_DEPTH(i.depth);
	}
	
	ENDCG
	
	SubShader 
	{	
		Tags { "RenderType" = "Opaque" }
	
		Pass 
		{
			Fog { Mode off } 
			
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment fragOpaque
			ENDCG 
		}               
	}               
	Fallback off
}