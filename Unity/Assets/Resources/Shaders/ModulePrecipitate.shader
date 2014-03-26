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
		_Saturation("Hologram Saturation", Float) = 0.5
		_Brightness("Hologram Brightness", Float) = 4.0
		_Tint("Holographic Tint (Alpha)", Color) = (0.4,0.7,1,0.1)
		_MinHeight("Minimum Position", Float) = 0
		_MaxHeight("Maximum Position", Float) = 1
		_Amount("Amount Complete", Range(0.0,1.0)) = 0.0
		_GlowIntensity("Glow Intensity", Float) = 2.0
		_FadeDist("Fade Distance", Float) = 0.3
		_FadeDistRatio("Fade Distance Ratio", Range(0.01, 0.99)) = 0.5
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
		
		// Solid lower transparent cutout shader
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
			float _FadeDistRatio;

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
				// Calculate the fade distances
				float lowerFadeDist = _FadeDist * (1.0 - _FadeDistRatio);
				float upperFadeDist = _FadeDist * _FadeDistRatio;
				
				// Get the current height and current displacement
				float ch = lerp(_MinHeight - upperFadeDist, _MaxHeight + lowerFadeDist, _Amount);
				float cd = IN.vh - ch;
	
				// Clip areas that are out of bounds
				clip(sign(cd) == 1.0 ? -1:1);
				
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
	
		// Holographic upper transparent pass
		CGPROGRAM
			#pragma surface surf BlinnPhong vertex:vert
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Diffuse;
			float _Saturation;
			float _Brightness;
			float4 _Tint;
			float _MinHeight;
			float _MaxHeight;
			float _Amount;
			float _FadeDist;
			float _FadeDistRatio;
			
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
				float3 col = tex2D(_Diffuse, IN.uv_Diffuse.xy).rgb;
				
				// Greyscale intensitity constant
				float3 gsik = dot(float3(0.3, 0.59, 0.11), col);	
				
				// Saturate color, add brightness and add tint
				col = _Saturation * gsik + col * (1.0 - _Saturation);
				col = 0.5 + (col * _Brightness) * 0.5;
				col = col * _Tint.rgb;
				
				// Calculate the fade distances
				float lowerFadeDist = _FadeDist * (1.0 - _FadeDistRatio);
				float upperFadeDist = _FadeDist * _FadeDistRatio;
				
				// Get the current height and current displacement
				float ch = lerp(_MinHeight - upperFadeDist, _MaxHeight + lowerFadeDist, _Amount);
				float cd = IN.vp.y - ch;
				
				if(sign(cd) == 1.0)
				{
					alpha = _Tint.a;
				}
				
				o.Alpha = alpha;
				o.Emission = col;
			}
		ENDCG
		
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
		ZWrite Off 
	
		// Noise and glow effect pass
		CGPROGRAM
			#pragma surface surf BlinnPhong vertex:vert
			#pragma glsl
			#pragma target 3.0
			#include "Noise/SimplexNoise.cginc"
			
			float _MinHeight;
			float _MaxHeight;
			float _Amount;
			float _GlowIntensity;
			float _FadeDist;
			float _FadeDistRatio;
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
				// Calculate the fade distances
				float lowerFadeDist = _FadeDist * (1.0 - _FadeDistRatio);
				float upperFadeDist = _FadeDist * _FadeDistRatio;
				
				// Get the current height
				float ch = lerp(_MinHeight - upperFadeDist, _MaxHeight + lowerFadeDist, _Amount);
				float cd = IN.vp.y - ch;

				float alpha = 0.0;
				float3 col = float3(_GlowIntensity);

				// Calculate noise for current position
				if((sign(cd) == 1.0 && cd < upperFadeDist) ||
					sign(cd) == -1.0 && -cd < lowerFadeDist)
				{
					float sum = 0;	
					float4 np = float4(IN.vp, _Time.y * _NoiseSpeed);
					float freq = _NoiseFreq, amp = 0.5;
					
					for(int i = 0; i < 4; ++i)
					{
						sum += snoise(np * freq) * amp;
						freq *= 2.0;
						amp *= 0.5;
					}
					
					float falloff = 0;
					if(sign(cd) == 1.0 && cd < upperFadeDist)
						falloff = 1.0 - cd/upperFadeDist;
					else if(sign(cd) == -1.0 && -cd < lowerFadeDist)
						falloff = 1.0 - ((-1.0 * cd)/lowerFadeDist);
						
					alpha = (falloff * falloff + sum) * falloff * falloff;
					col *= _GlowIntensity * falloff * falloff;
				}
				
				if(_GlowIntensity < 1.0)
				{
					alpha *= _GlowIntensity;
				}
				
				o.Alpha = alpha;
				o.Emission = col;
			}
		ENDCG
	}
	Fallback "Diffuse"
}