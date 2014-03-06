Shader "MineralsShader"
{
	Properties 
	{
		_Diffuse("_Emissive", 2D) = "gray" {}
		_Normal("_Normal", 2D) = "black" {}
		_AlphaMap("_AlphaMap", 2D) = "white" {}
		_EmissivePower("_EmissivePower", Float) = 1
		_Tint("_Tint", Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Transparent" // Geometry
			"IgnoreProjector"="True"
			"RenderType"="Transparent" //Opaque
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Blend SrcAlpha OneMinusSrcAlpha
		Fog{}

		CGINCLUDE
		#include"VoidShaderVariables.cginc"
		ENDCG

		CGPROGRAM
		#pragma surface surf BlinnPhongEditor vertex:vert finalcolor:FogPass
		#pragma target 3.0

		sampler2D _Diffuse;
		sampler2D _Normal;
		sampler2D _AlphaMap;
		float _EmissivePower;
		float4 _Tint;

		struct EditorSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half3 Gloss;
			half Specular;
			half Alpha;
			half4 Custom;
		};
			
		inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
		{
			half3 spec = light.a * s.Gloss;
			half4 c;
			c.rgb = (s.Albedo * light.rgb + light.rgb * spec) * s.Alpha;
			c.a = s.Alpha;
			return c;
		}

		inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 h = normalize (lightDir + viewDir);
				
			half diff = max (0, dot ( lightDir, s.Normal ));
				
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, s.Specular*128.0);
				
			half4 res;
			res.rgb = _LightColor0.rgb * diff;
			res.w = spec * Luminance (_LightColor0.rgb);
			res *= atten * 2.0;

			return LightingBlinnPhongEditor_PrePass( s, res );
		}
			
		struct Input
		{
			float2 uv_Diffuse;
			float2 uv_Normal;
			float2 uv_AlphaMap;
			float3 worldPos;
			float4 screenPos;
		};

		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o)
			float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);
		}

		void FogPass(Input IN, EditorSurfaceOutput o, inout fixed4 colour) {colour = void_SampleFog(IN.screenPos, IN.worldPos, colour);}

		void surf (Input IN, inout EditorSurfaceOutput o)
		{
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			o.Custom = 0.0;
				
			float4 Tex2D0 = tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy) * _Tint;
			float4 Tex2DNormal0 = float4(UnpackNormal( tex2D(_Normal,(IN.uv_Normal.xyxy).xy)).xyz, 1.0 );
			float4 Tex2D1 = tex2D(_AlphaMap,(IN.uv_AlphaMap.xyxy).xy);
			float4 Master0_2_NoInput = float4(0,0,0,0);
			float4 Master0_5_NoInput = float4(1,1,1,1);
			float4 Master0_7_NoInput = float4(0,0,0,0);
			float4 Master0_6_NoInput = float4(1,1,1,1);
			o.Emission = Tex2D0 * _EmissivePower;
			o.Normal = Tex2DNormal0;
			o.Alpha = Tex2D1.xxxx;

			o.Normal = normalize(o.Normal);
		}

		ENDCG
	}

	Fallback "Diffuse"
}
