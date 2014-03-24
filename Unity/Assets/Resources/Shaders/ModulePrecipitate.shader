Shader "VOID/Module Precipitate" 
{
	Properties
	{
		_Diffuse("Diffuse", 2D) = "gray" {}
		_Normal("Normal", 2D) = "blue" {}
		_Specular("Specular", 2D) = "black" {}
		_SpecPower("SpecPower", Float) = 1
		_DiffuseCol("DiffuseCol", Color) = (1,1,1,1)
		_SpecCol("SpecCol", Color) = (1,1,1,1)
		_Saturation("Saturation", Float) = 0.5
		_Brightness("Brightness", Float) = 4.0
		_Tint("Holographic Tint (Alpha)", Color) = (0.4,0.7,1,0.1)
		_MinHeight("Minimum Position", Float) = 0
		_MaxHeight("Maximum Position", Float) = 1
		_Amount("Amount Complete", Range(0.0,1.0)) = 0.0
		_FadeDist("Fade Distance", Float) = 0.3
		_NoiseFreq("Noise Frequency", Float) = 10.0
		_NoiseSpeed("Noise Speed", Float) = 0.25
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue"="AlphaTest" 
			"IgnoreProjector"="True"
			"RenderType"="TransparentCutout" 
		}
		
		Cull Off 
		
		CGPROGRAM
			#pragma surface surf BlinnPhong vertex:vert
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Diffuse;
			sampler2D _Normal;
			sampler2D _Specular;
			float4 _DiffuseCol;
			float4 _SpecCol;
			float _SpecPower;
			float _MinHeight;
			float _MaxHeight;
			float _Amount;
			float _FadeDist;

			struct Input 
			{
				float2 uv_Diffuse;
				float2 uv_Normal;
				float2 uv_Specular;
				float3 worldRefl;
				float vh;
			};
			
			void vert (inout appdata_full v, out Input o)
			{
			    UNITY_INITIALIZE_OUTPUT(Input, o);
			    o.vh = v.vertex.y;
			}
			
			void surf(Input IN, inout SurfaceOutput o)
			{											
				float ih = lerp(_MinHeight, _MaxHeight, _Amount);
				float dist = IN.vh - ih + (_FadeDist * (1.0 - _Amount));
	
				clip(sign(dist) == 1.0 ? -1:1);
				
				float4 Tex2D0 = tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy);
				float4 Tex2DNormal0 = float4(UnpackNormal( tex2D(_Normal,(IN.uv_Normal.xyxy).xy)).xyz, 1.0);
				float4 Tex2D1 = tex2D(_Specular,(IN.uv_Specular.xyxy).xy);
				float4 Multiply0 = Tex2D1 * _SpecPower.xxxx * _SpecCol;
				o.Albedo = Tex2D0 * _DiffuseCol;
				o.Normal = Tex2DNormal0;
				o.Specular = Tex2D1.a;
				o.Gloss = Multiply0;
			}
		ENDCG
		
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
		ZWrite Off 
	
		CGPROGRAM
			#pragma surface surf BlinnPhong vertex:vert
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "Noise/SimplexNoise.cginc"
			
			sampler2D _Diffuse;
			float _Saturation;
			float _Brightness;
			float4 _Tint;
			float _MinHeight;
			float _MaxHeight;
			float _Amount;
			float _FadeDist;
			float _NoiseFreq;
			float _NoiseSpeed;
			
			struct Input 
			{
				float2 uv_Diffuse;
				float3 vp;
			};
			
			void vert (inout appdata_full v, out Input o)
			{
			    UNITY_INITIALIZE_OUTPUT(Input, o);
			    o.vp = v.vertex.xyz;
			}
		
			void surf(Input IN, inout SurfaceOutput o)
			{											
				float alpha = 0.0;
				float4 col = tex2D(_Diffuse, IN.uv_Diffuse.xy);
				
				// Greyscale intensitity constant
				float3 gsik = dot(float3(0.3, 0.59, 0.11), col.rgb);	
				
				// Saturate color and add tint
				col.rgb = _Saturation * gsik + col.rgb * (1.0 - _Saturation);
				col = 0.5 + (col * _Brightness) * 0.5;
				col.rgb = col.rgb * _Tint.rgb;
				
				float ih = lerp(_MinHeight, _MaxHeight, _Amount);
				float ch = IN.vp.y - ih + (_FadeDist * (1.0 - _Amount));
				float lowerFadeDist = _FadeDist * 0.3;
				if(sign(ch) == 1.0)
				{
					if(ch < _FadeDist)
					{
						float4 np = float4(IN.vp, _Time.y * _NoiseSpeed);
						float freq = _NoiseFreq, amp = 0.5;
						float sum = 0;	
						for(int i = 0; i < 2; i++) 
						{
							sum += abs(snoise(np * freq)) * amp;
							freq *= 3.0;
							amp *= 0.33;
						}
					
						float falloff = ch/_FadeDist;
						alpha = (1.0f - falloff + sum) * (1.0f - falloff);
						col.rgb = lerp(col.rgb, float3(_Brightness), alpha);
					}

					if(alpha < _Tint.a)
						alpha = _Tint.a;
				}
				else if(-ch < lowerFadeDist)
				{
					float4 np = float4(IN.vp, _Time.y * _NoiseSpeed);
					float freq = _NoiseFreq, amp = 0.5;
					float sum = 0;	
					for(int i = 0; i < 2; i++) 
					{
						sum += abs(snoise(np * freq)) * amp;
						freq *= 3.0;
						amp *= 0.33;
					}
				
					float falloff = -ch/lowerFadeDist;
					alpha = (1.0 - falloff + sum) * (1.0f - falloff);
					col.rgb = float3(_Brightness);
				}
				
				o.Alpha = alpha;
				o.Emission = col.rgb;
			}
		ENDCG
	}
	Fallback "Diffuse"
}