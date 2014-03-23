Shader "VOID/Nebulae_Placeholder" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Cull Front
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Emission = c.rgb * 0.5;
			o.Alpha = 0.5;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
