Shader "VOID/AnimatingSun" 
{
	Properties
	{
		_Color1("Color1", Color) = (0.0, 0.0, 0.0, 1.0)
		_Color2("Color2", Color) = (1.0, 1.0, 1.0, 1.0)
		_ScaleFactor("ScaleFactor", Range(0.0,5.0)) = 1.0
		_EmmisivePower("Emmisive Power", Float) = 2.0
	}

	SubShader 
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="False"
			"RenderType"="Opaque"
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Fog{}

		CGPROGRAM
		#pragma surface surf BlinnPhong vertex:vert
		#pragma target 3.0
		#include "UnityCG.cginc"
		#include "SimplexNoise.cginc"
			
		float4 _Color1;
		float4 _Color2;
		float _ScaleFactor;
		float _EmmisivePower;
			
		struct Input 
		{
			float3 noise;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.noise = v.vertex.xyz;
			o.noise += v.normal * _ScaleFactor;
			o.noise.y -= _Time.z;
		}	

		void surf(Input IN, inout SurfaceOutput o) 
		{
			float3 noise = IN.noise;
		
			float freq = 1.0, amp = 0.5;
			float sum = 0;	
			for(int i = 0; i < 4; i++) 
			{
				sum += abs(snoise(noise * freq) * amp);
				freq *= 2.0;
				amp *= 0.5;
			}
		
			//final = 0.5 + 0.5 * final;
			sum += abs(sin(_Time.y + sum) * 0.25);

			float4 c = lerp(_Color1, _Color2, sum);
			
			o.Emission = c * _EmmisivePower;
		}
		ENDCG
	}
	Fallback "Diffuse"
}