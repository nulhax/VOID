Shader "VOID/Pilot Shader Replacement" 
{
	Properties 
	{
		_Diffuse("Diffuse", 2D) = "white" {}
	}
	
	SubShader 
	{
		Tags 
		{ 
//			"RenderType" = "Transparent" 
//			"IgnoreProjector"="True"
//			"RenderQueue" = "Transparent" 
		}
		
		Cull Front
		Blend SrcAlpha OneMinusSrcAlpha 
		AlphaTest Greater 0.1
		LOD 200
		
		CGPROGRAM
			#pragma surface surf Lambert
			
			sampler2D _Diffuse;

			struct Input 
			{
				float2 uv_Diffuse;
			};
		
			void surf(Input IN, inout SurfaceOutput o)
			{		
				const float _Saturation = 0.8;
				const float _Brightness = 4.0;
				const float4 _Tint = float4(0.4,0.7,0.7,0.4);
															
				float4 col = tex2D(_Diffuse, IN.uv_Diffuse.xy);
				
				// Greyscale intensitity constant
				float3 gsik = dot(float3(0.3, 0.59, 0.11), col.rgb);	
				
				// Saturate color and add tint
				col.rgb = _Saturation * gsik + col.rgb * (1.0 - _Saturation);
				col = saturate(col * _Brightness);
				
				o.Emission = col.rgb * _Tint.rgb;
				o.Alpha = _Tint.a;
			}
		ENDCG
	} 
	FallBack "Diffuse"
}
