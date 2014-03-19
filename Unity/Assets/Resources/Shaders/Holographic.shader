Shader "VOID/Holographic" 
{
	Properties 
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_Saturation("Saturation", Float) = 0.5
		_Brightness("Brightness", Float) = 4.0
		_Tint("Holographic Tint (Alpha)", Color) = (0.4,0.7,1,0.7)
	}
	
	SubShader 
	{
		Tags 
		{ 
			//"RenderType" = "Transparent" 
			//"IgnoreProjector"="True"
			//"RenderQueue" = "Transparent" 
		}
		
		LOD 200
		
		CGPROGRAM
			#pragma surface surf Lambert alpha
			
			sampler2D _Diffuse;
			float _Saturation;
			float _Brightness;
			float4 _Tint;

			struct Input 
			{
				float2 uv_Diffuse;
			};
		
			void surf(Input IN, inout SurfaceOutput o)
			{								
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
