Shader "VOID/Diffuse Spec Normal Emis"
{
	Properties 
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_Normal("Normal", 2D) = "blue" {}
		_Specular("Specular", 2D) = "black" {}
		_Emissive("Emissive", 2D) = "black" {}
		_SpecPower("SpecPower", Float) = 1
		_DiffuseCol("DiffuseCol", Color) = (1,1,1,1)
		_SpecCol("SpecCol", Color) = (1,1,1,1)
		_EmissiveColorR("EmissiveColorR", Color) = (0,0,0,1)
		_EmissivePowerR("EmissivePowerR", Float) = 1
		_EmissiveColorG("EmissiveColorG", Color) = (0,0,0,1)
		_EmissivePowerG("EmissivePowerG", Float) = 1
		_EmissiveColorB("EmissiveColorB", Color) = (0,0,0,1)
		_EmissivePowerB("EmissivePowerB", Float) = 1
		_EmissiveColorA("EmissiveColorA", Color) = (0,0,0,1)
		_EmissivePowerA("EmissivePowerA", Float) = 1
	}
	
	SubShader 
	{
		Tags
		{
			"RenderType"="Opaque"
			"IgnoreProjector"="False"
			"Queue"="Geometry"
		}

		Cull Back
		ZWrite On
		ZTest LEqual
		ColorMask RGBA
		Fog{}

		CGPROGRAM
		#pragma surface surf BlinnPhongEditor
		#pragma target 3.0


		sampler2D _Diffuse;
		sampler2D _Normal;
		sampler2D _Specular;
		sampler2D _Emissive;
		float4 _DiffuseCol;
		float4 _SpecCol;
		float _SpecPower;
		float4 _EmissiveColorR;
		float _EmissivePowerR;
		float4 _EmissiveColorG;
		float _EmissivePowerG;
		float4 _EmissiveColorB;
		float _EmissivePowerB;
		float4 _EmissiveColorA;
		float _EmissivePowerA;
		

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
		};

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
			float4 Multiply0 = Tex2D1 * _SpecPower.xxxx * _SpecCol;
			o.Albedo = Tex2D0 * _DiffuseCol;
			o.Emission = Tex2D2.r * _EmissiveColorR * _EmissiveColorR.a * _EmissivePowerR + 
						 Tex2D2.g * _EmissiveColorG * _EmissiveColorG.a * _EmissivePowerG + 
						 Tex2D2.b * _EmissiveColorB * _EmissiveColorB.a * _EmissivePowerB + 
						 Tex2D2.a * _EmissiveColorA * _EmissiveColorA.a * _EmissivePowerA;
			o.Normal = Tex2DNormal0;
			o.Specular = Tex2D1.a;
			o.Gloss = Multiply0;

			o.Normal = normalize(o.Normal);
		}
		ENDCG
	}
	Fallback "Diffuse"
}
