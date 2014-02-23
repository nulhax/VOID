Shader "VOID/DiffuseSpecNormalEmisReflect"
{
	Properties 
	{
		_Diffuse("_Diffuse", 2D) = "gray" {}
		_Normal("Normal", 2D) = "blue" {}
		_Specular("_Specular", 2D) = "black" {}
		_Emissive("_Emissive", 2D) = "black" {}
		_SpecPower("SpecPower", Float) = 1
		_EmissiveColorR("EmissiveColorR", Color) = (0,0,0,1)
		_EmissivePowerR("_EmissivePowerR", Range(0, 5)) = 1
		_EmissiveColorG("EmissiveColorG", Color) = (0,0,0,1)
		_EmissivePowerG("_EmissivePowerG", Range(0, 5)) = 1
		_EmissiveColorB("EmissiveColorB", Color) = (0,0,0,1)
		_EmissivePowerB("_EmissivePowerB", Range(0, 5)) = 1
		_EmissiveColorA("EmissiveColorA", Color) = (0,0,0,1)
		_EmissivePowerA("_EmissivePowerA", Range(0, 5)) = 1
		_Cube ("Reflection Cubemap", Cube) = "black" { TexGen CubeReflect }
		_ReflectColor ("Reflection Color", Color) = (0.5,0.5,0.5,0.5)
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
		#pragma surface surf BlinnPhongEditor vertex:vert
		#pragma target 3.0
		#pragma glsl


		sampler2D _Diffuse;
		sampler2D _Normal;
		sampler2D _Specular;
		sampler2D _Emissive;
		float _SpecPower;
		float4 _EmissiveColorR;
		float _EmissivePowerR;
		float4 _EmissiveColorG;
		float _EmissivePowerG;
		float4 _EmissiveColorB;
		float _EmissivePowerB;
		float4 _EmissiveColorA;
		float _EmissivePowerA;
		samplerCUBE _Cube;
		float4 _ReflectColor;
		

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
			float2 uv_Specular;
			float2 uv_Emissive;
			float3 worldRefl;
			INTERNAL_DATA
		};

		void vert (inout appdata_full v, out Input o) 
		{
			UNITY_INITIALIZE_OUTPUT(Input,o)
			float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
			float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);
		}	

		void surf (Input IN, inout EditorSurfaceOutput o) 
		{
			o.Normal = float3(0.0,0.0,1.0);
			o.Alpha = 1.0;
			o.Albedo = 0.0;
			o.Emission = 0.0;
			o.Gloss = 0.0;
			o.Specular = 0.0;
			o.Custom = 0.0;
			
			float4 Tex2D0 = tex2D(_Diffuse,(IN.uv_Diffuse.xyxy).xy);
			float4 Tex2DNormal0 = float4(UnpackNormal( tex2D(_Normal,(IN.uv_Normal.xyxy).xy)).xyz, 1.0);
			float4 Tex2D1 = tex2D(_Specular,(IN.uv_Specular.xyxy).xy);
			float4 Tex2D2 = tex2D(_Emissive,(IN.uv_Emissive.xyxy).xy);
			float4 Multiply0 = Tex2D1 * _SpecPower.xxxx;
			float4 Master0_2_NoInput = float4(0,0,0,0);
			float4 Master0_5_NoInput = float4(1,1,1,1);
			float4 Master0_7_NoInput = float4(0,0,0,0);
			float4 Master0_6_NoInput = float4(1,1,1,1);
			o.Albedo = Tex2D0;
			o.Emission = Tex2D2.r * _EmissiveColorR * _EmissiveColorR.a * _EmissivePowerR + 
						 Tex2D2.g * _EmissiveColorG * _EmissiveColorG.a * _EmissivePowerG + 
						 Tex2D2.b * _EmissiveColorB * _EmissiveColorB.a * _EmissivePowerB + 
						 Tex2D2.a * _EmissiveColorA * _EmissiveColorA.a * _EmissivePowerA;
			o.Normal = Tex2DNormal0;
			o.Specular = Tex2D1.a;
			o.Gloss = Multiply0;
			o.Normal = normalize(o.Normal);
			
			float3 worldRefl = WorldReflectionVector (IN, o.Normal);
			float4 reflcol = texCUBE(_Cube, worldRefl);
			reflcol *= Multiply0;
			o.Emission += reflcol.rgb * _ReflectColor.rgb;
			//o.Alpha = reflcol.a * _ReflectColor.a;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
