Shader "VOID/Nebulae" 
{
	Properties
	{
		_Color1("Color1", Color) = (0.0, 0.0, 0.0, 1.0)
		_Color2("Color2", Color) = (1.0, 1.0, 1.0, 1.0)
		_Offset("Offset", Vector) = (0.0, 0.0, 0.0, 1.0)
		_NoiseParams("NoiseParams", Vector) = (0.0, 255.0, 0.5, 1.0)
	}

	SubShader 
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="False"
			"RenderType"="Opaque"
		}

		Cull Front
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
		float4 _Offset;
		float4 _NoiseParams;
			
		struct Input 
		{
			float3 worldPos;
			float3 worldNormal;
			float3 noise;
		};

		void vert(inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.noise = v.vertex.xyz - v.normal;
			o.noise += v.normal * _Offset.w;
			o.noise += _Offset.xyz;
		}	

		void surf(Input IN, inout SurfaceOutput o) 
		{
			float3 noise = IN.noise;
		
		
		
		
			float maxAmp = 0;
		    float amp = 1;
		    float freq = _NoiseParams.w;
		    float final = 0;

		    //add successively smaller, higher-frequency terms
	        final += snoise(noise * freq) * amp;
	        maxAmp += amp;
	        amp *= _NoiseParams.z;
	        freq *= 2;
	        
	        final += abs(snoise(noise * freq)) * amp;
	        maxAmp += amp;
	        amp *= _NoiseParams.z;
	        freq *= 2;
	        
	        final += abs(snoise(noise * freq)) * amp;
	        maxAmp += amp;
	        amp *= _NoiseParams.z;
	        freq *= 2;
	        
	        final += abs(snoise(noise * freq)) * amp;
	        maxAmp += amp;
	        amp *= _NoiseParams.z;
	        freq *= 2;

		    //take the average value of the iterations
		    final /= maxAmp;

		    //normalize the result
		    final = final * (_NoiseParams.y - _NoiseParams.x) / 2 + (_NoiseParams.y + _NoiseParams.x) / 2;

		
		
			
		
		
		
		
		
		
//			float o1 = snoise(noise);
//			float o2 = snoise(2.0 * noise) / 2.0;
//			float o3 = abs(snoise(4.0 * noise)) / 4.0;
//			float o4 = abs(snoise(8.0 * noise)) / 8.0;
//		
//			float final = o1 + o2 + o3 + o4;
			//final = 0.5 + 0.5 * final;
			//final += abs(sin(_Time.y + final) * 0.25);

			float4 c = lerp(_Color1, _Color2, final);
			
			o.Emission = c;
		}
		ENDCG
	}
	Fallback "Diffuse"
}