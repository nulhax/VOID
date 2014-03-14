Shader "VOID/Nebulae (Fractal Brownian Motion)" 
{
	Properties
	{
		_ColorGradient("ColorGradient", 2D) = "black" {}
		_Frequency("Frequency", Float) = 0.1
		_Lacunarity("Lacunarity", Float) = 2.0
		_Persistence("Persistence", Float) = 0.5
		_Offset("Offset Position", Vector) = (0,0,0,0)
	}

	SubShader 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		AlphaTest Greater .01
		ColorMask RGB
		Cull Off 
		Lighting Off 
		ZWrite Off 
		Fog { Color (0,0,0,0) }
	
	    Pass 
	    {
	
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			uniform sampler2D _PermTable2D;
			uniform sampler2D _Gradient3D;
			uniform sampler2D _ColorGradient;
			uniform float _Frequency;
			uniform float _Lacunarity;
			uniform float _Persistence;
			uniform float4 _Offset;
			
			struct v2f 
			{
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float4 worldPos : TEXCOORD1;
			};
			
			v2f vert (appdata_base v)
			{
			    v2f o;
			    o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.uv = v.texcoord.xy;
			    o.worldPos = mul(_Object2World, v.vertex) + _Offset;
			    return o;
			}
		
			float3 fade(float3 t)
			{
				return t * t * t * (t * (t * 6 - 15) + 10); // new curve
				//return t * t * (3 - 2 * t); // old curve
			}
		
			float4 perm2d(float2 uv)
			{
				return tex2D(_PermTable2D, uv);
			}
		
			float gradperm(float x, float3 p)
			{
				float3 g = tex2D(_Gradient3D, float2(x, 0) ).rgb *2.0 - 1.0;
				return dot(g, p);
			}
					
			float noise(float3 p)
			{
				float3 P = fmod(floor(p), 256.0);	// FIND UNIT CUBE THAT CONTAINS POINT
			  	p -= floor(p);                      // FIND RELATIVE X,Y,Z OF POINT IN CUBE.
				float3 f = fade(p);                 // COMPUTE FADE CURVES FOR EACH OF X,Y,Z.
			
				P = P / 256.0;
				const float one = 1.0 / 256.0;
				
			    // HASH COORDINATES OF THE 8 CUBE CORNERS
				float4 AA = perm2d(P.xy) + P.z;
			 
				// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
			  	return lerp( lerp( lerp( gradperm(AA.x, p ),  
			                             gradperm(AA.z, p + float3(-1, 0, 0) ), f.x),
			                       lerp( gradperm(AA.y, p + float3(0, -1, 0) ),
			                             gradperm(AA.w, p + float3(-1, -1, 0) ), f.x), f.y),
			                             
			                 lerp( lerp( gradperm(AA.x+one, p + float3(0, 0, -1) ),
			                             gradperm(AA.z+one, p + float3(-1, 0, -1) ), f.x),
			                       lerp( gradperm(AA.y+one, p + float3(0, -1, -1) ),
			                             gradperm(AA.w+one, p + float3(-1, -1, -1) ), f.x), f.y), f.z);
			}
		
			// fractal sum, range -1.0 - 1.0
			float fBm(float3 p, int octaves)
			{
				float freq = _Frequency, amp = 0.5;
				float sum = 0;	
				for(int i = 0; i < octaves; i++) 
				{
					sum += noise(p * freq) * amp;
					freq *= _Lacunarity;
					amp *= _Persistence;
				}
				return sum;
			}

			half4 frag (v2f i) : COLOR
			{
				float n = fBm(i.worldPos.xyz, 6);

				half4 col = tex2D(_ColorGradient, float2(n, 0));

				// Bring UV to -1, 1 space
				float2 nuv = float2((i.uv.x * 2.0) - 1.0, (i.uv.y * 2.0) - 1.0);
				float lennuv = length(nuv);
				
				// Fade the color away at the edges
				if(lennuv > 0.5)
				{
					col *= (1.0 - lennuv) / (1.0 - 0.5);
				}
				
			    return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}