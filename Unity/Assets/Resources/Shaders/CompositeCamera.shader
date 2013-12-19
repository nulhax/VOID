Shader "Hidden/CompositeCameraShader" 
{
	Properties 
	{
		_ForgroundTex ("Foreground Render Texture", 2D) = "black" {}
		_BackgroundTex ("Background Render Texture", 2D) = "black" {}
		_ForgroundDepth ("Foreground Depth Texture", 2D) = "black" {}
		_BackgroundDepth ("Background Depth Texture", 2D) = "black" {} 
		_ForgroundInBackground ("Forground has enteted Background", Float) = 0
	}
		
	CGINCLUDE
	
	#include "UnityCG.cginc"

	struct v2f 
	{ 
		float4 pos	: POSITION; 
   		float2 uv : TEXCOORD0; 
	}; 

	sampler2D _ForgroundTex;
	sampler2D _BackgroundTex;
	sampler2D _ForgroundDepth;
	sampler2D _BackgroundDepth;
	float _ForgroundInBackground;
	
	v2f vert(appdata_img v)
	{
		v2f o; 
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);  
		o.uv = v.texcoord.xy;  
		
	#if UNITY_UV_STARTS_AT_TOP
		o.uv.y = 1 - o.uv.y;
	#endif
		
		return o;
	}

 	float4 frag(v2f i) : COLOR
	{  
		float4 foregroundCol = tex2D(_ForgroundTex, i.uv);
		float4 backgroundCol = tex2D(_BackgroundTex, i.uv);
		
		float forgroundDepth = UNITY_SAMPLE_DEPTH(tex2D(_ForgroundDepth, i.uv));
		float backgroundDepth = UNITY_SAMPLE_DEPTH(tex2D(_BackgroundDepth, i.uv));
 		   
 		float4 output = backgroundCol; 
 		if(forgroundDepth < backgroundDepth)
 		{
 			output = foregroundCol;  
 			
 			if(_ForgroundInBackground == 1)
	 		{  
	 			output.r = (foregroundCol.r) + (backgroundCol.r * (1.0 - foregroundCol.a));
	 		 	output.g = (foregroundCol.g) + (backgroundCol.g * (1.0 - foregroundCol.a));
	 		 	output.b = (foregroundCol.b) + (backgroundCol.b * (1.0 - foregroundCol.a)); 
	 		}  
 		} 
 		
 		if(_ForgroundInBackground == 0)
 		{  
 			output.r = (foregroundCol.r) + (backgroundCol.r * (1.0 - foregroundCol.a));
 		 	output.g = (foregroundCol.g) + (backgroundCol.g * (1.0 - foregroundCol.a));
 		 	output.b = (foregroundCol.b) + (backgroundCol.b * (1.0 - foregroundCol.a)); 
 		} 
 	 
 		return output;
 	}
	
	ENDCG 
	

	 
	SubShader 
	{	
		Pass 
		{
			ZTest Off Cull Off ZWrite Off
			Fog { Mode off } 
			
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0		
			ENDCG 
		}               
	}                  
	Fallback off
} 