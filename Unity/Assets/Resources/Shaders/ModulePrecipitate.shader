Shader "VOID/Module Precipitate" 
{
	Properties
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_MinHeight("Minimum Position", Float) = 0
		_MaxHeight("Maximum Position", Float) = 1
		_Amount("Amount Complete", Range(0.0,1.0)) = 0.0
		_FadeDist("Fade Distance", Float) = 0.1
		_NoiseFreq("Noise Frequency", Float) = 10.0
		_NoiseSpeed("Noise Speed", Float) = 2.0
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
		}
		
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0.1
		ColorMask RGB
		Cull Off 
		ZWrite On 
		Fog { }
	
		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		#include "UnityCG.cginc"
		#include "Noise/SimplexNoise.cginc"
		
		sampler2D _Diffuse;
		float _MinHeight;
		float _MaxHeight;
		float _Amount;
		float _FadeDist;
		float _NoiseFreq;
		float _NoiseSpeed;
		
		struct Input 
		{
			float2 uv_Diffuse;
			float4 vp;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
		    UNITY_INITIALIZE_OUTPUT(Input, o);
		    o.vp = v.vertex;
		}
	
		void surf(Input IN, inout SurfaceOutput o)
		{											
			float3 col = tex2D(_Diffuse, IN.uv_Diffuse).xyz;
			float alpha = 1.0;
			
			float ih = lerp(_MinHeight, _MaxHeight, _Amount);
			float dist = IN.vp.y - ih + _FadeDist;
			if(sign(dist) == 1.0)
			{
				if(dist < _FadeDist)
				{
					float4 np = float4(IN.vp.xyz, _Time.y * _NoiseSpeed);
					float freq = _NoiseFreq, amp = 0.5;
					float sum = 0;	
					for(int i = 0; i < 2; i++) 
					{
						sum += abs(snoise(np * freq)) * amp;
						freq *= 3.0;
						amp *= 0.33;
					}
				
					float falloff = dist/_FadeDist;
					alpha = (1.0f - falloff + sum) * (1.0f - falloff);
				}
				else
				{
					alpha = 0.0;
				}
			}
			
			o.Albedo = col;
			o.Alpha = alpha;
		}
		ENDCG
	}
	Fallback "Diffuse"
}