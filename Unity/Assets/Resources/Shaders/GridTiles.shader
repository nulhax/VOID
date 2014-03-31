Shader "VOID/Grid Tiles" 
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
		_GlowIntensity("Glow Intensity", Float) = 2.0
		_GlowDist("Fade Distance", Float) = 0.1
		_PlaneNormal("Plane Normal", Vector) = (0,1,0,0)
		_PlanePoint("Plane Point", Vector) = (0,0,0,0)
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
			#pragma surface surf BlinnPhong
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Diffuse;
			sampler2D _Normal;
			sampler2D _Specular;
			float4 _DiffuseCol;
			float4 _SpecCol;
			float _SpecPower;
			float _GlowDist;
			float4 _PlaneNormal;
			float4 _PlanePoint;

			struct Input 
			{
				float2 uv_Diffuse;
				float2 uv_Normal;
				float2 uv_Specular;
				float3 worldPos;
			};
			
			void surf(Input IN, inout SurfaceOutput o)
			{	
				// Calculate distance from plane
				float planeD = -(_PlaneNormal.x * _PlanePoint.x) - (_PlaneNormal.y * _PlanePoint.y) - (_PlaneNormal.z * _PlanePoint.z);
																							
				float dist = (_PlaneNormal.x * IN.worldPos.x + _PlaneNormal.y * IN.worldPos.y + _PlaneNormal.z * IN.worldPos.z + planeD) / 
							sqrt(_PlaneNormal.x * _PlaneNormal.x + _PlaneNormal.y * _PlaneNormal.y + _PlaneNormal.z * _PlaneNormal.z);

				// Clip areas that are out of bounds
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
	
		// Holographic upper transparent pass
		CGPROGRAM
			#pragma surface surf BlinnPhong
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Diffuse;
			float _Saturation;
			float _Brightness;
			float4 _Tint;
			float _GlowDist;
			float4 _PlaneNormal;
			float4 _PlanePoint;
			
			struct Input 
			{
				float2 uv_Diffuse;
				float3 worldPos;
			};
		
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
				
				// Calculate distance from plane
				float planeD = -(_PlaneNormal.x * _PlanePoint.x) - (_PlaneNormal.y * _PlanePoint.y) - (_PlaneNormal.z * _PlanePoint.z);
																							
				float dist = (_PlaneNormal.x * IN.worldPos.x + _PlaneNormal.y * IN.worldPos.y + _PlaneNormal.z * IN.worldPos.z + planeD) / 
							sqrt(_PlaneNormal.x * _PlaneNormal.x + _PlaneNormal.y * _PlaneNormal.y + _PlaneNormal.z * _PlaneNormal.z);
				
				if(sign(dist) == 1.0)
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
	
		// Glow effect pass
		CGPROGRAM
			#pragma surface surf BlinnPhong
			#pragma target 3.0
			#include "Noise/SimplexNoise.cginc"
			
			float _GlowIntensity;
			float _GlowDist;
			float4 _PlaneNormal;
			float4 _PlanePoint;
			
			struct Input 
			{
				float2 uv_Diffuse;
				float3 worldPos;
			};
		
			void surf(Input IN, inout SurfaceOutput o)
			{											
				// Calculate distance from plane
				float planeD = -(_PlaneNormal.x * _PlanePoint.x) - (_PlaneNormal.y * _PlanePoint.y) - (_PlaneNormal.z * _PlanePoint.z);
																							
				float dist = (_PlaneNormal.x * IN.worldPos.x + _PlaneNormal.y * IN.worldPos.y + _PlaneNormal.z * IN.worldPos.z + planeD) / 
							sqrt(_PlaneNormal.x * _PlaneNormal.x + _PlaneNormal.y * _PlaneNormal.y + _PlaneNormal.z * _PlaneNormal.z);

				float alpha = 0.0;
				float3 col = float3(_GlowIntensity);

				// Calculate noise for current position
				if(sign(dist) == -1.0 && -dist < _GlowDist)
				{
					float falloff = 1.0 - (-1.0 * dist)/_GlowDist;
						
					alpha = falloff * falloff;
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