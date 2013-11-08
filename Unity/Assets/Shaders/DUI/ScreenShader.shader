Shader "DUI/ScreenShader" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Alpha ("Alpha", Range(0.0, 1.0)) = 0.5
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_EmissivePower ("Emissive Power", Range(1.0, 10.0)) = 1.0
	}
	SubShader 
	{
		Tags 
		{ 
			"RenderType" = "Transparent" 
			"Queue" = "Transparent"
		}
		
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong alpha

		sampler2D _MainTex;
		float _Alpha;
		float _Shininess;
		float _EmissivePower;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutput o) 
		{
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
		
			o.Emission = c.rgb * _EmissivePower;
			o.Specular = _Shininess * _SpecColor.r;
			o.Gloss = _Shininess * _SpecColor.r;
			o.Alpha = _Alpha;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
