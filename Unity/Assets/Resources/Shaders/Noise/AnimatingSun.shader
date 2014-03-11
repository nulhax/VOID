Shader "VOID/AnimatingSun" 
{
	Properties
	{
		_Color1("Color1", Color) = (0.0, 0.0, 0.0, 1.0)
		_Color2("Color2", Color) = (1.0, 1.0, 1.0, 1.0)
		_ScaleFactor("ScaleFactor", Range(0.0,5.0)) = 1.0
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
			
		struct Input 
		{
			float3 noise;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.noise = v.vertex.xyz;
			o.noise += v.normal * _ScaleFactor;
			o.noise.y += _Time.x;
		}	

		void surf(Input IN, inout SurfaceOutput o) 
		{
			float3 noise = IN.noise;
		
			float o1 = abs(snoise(noise));
			float o2 = abs(snoise(2.0 * noise)) / 2.0;
			float o3 = abs(snoise(4.0 * noise)) / 4.0;
			float o4 = abs(snoise(8.0 * noise)) / 8.0;
		
			float final = o1 + o2 + o3 + o4;
			//final = 0.5 + 0.5 * final;
			final += abs(sin(_Time.y + final) * 0.25);

			float4 c = lerp(_Color1, _Color2, final);
			
			o.Emission = c;
		}
		ENDCG
	}
	Fallback "Diffuse"
}