Shader "VOID/Holographic" 
{
	Properties 
	{
		_Diffuse("Diffuse", 2D) = "white" {}
		_Color("Color", Color) = ( 0.4, 0.7, 0.7, 0.4 )
		_Saturation("Saturation", Float) = 0.8
		_Brightness("Brightness", Float) = 4.0
	}
	
	SubShader 
	{
		Tags 
		{
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent"
		}
		
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
		ZWrite Off 
		Cull Off 
		
		CGPROGRAM
			#pragma surface surf Lambert vertex:vert
			
			sampler2D _Diffuse;
			float4 _Color;
			float _Saturation;
			float _Brightness;

			struct Input 
			{
				float2 uv_Diffuse;
			};
			
			void vert(inout appdata_full v, out Input o) 
			{
          		UNITY_INITIALIZE_OUTPUT(Input, o);
			}
		
			void surf(Input IN, inout SurfaceOutput o)
			{			
				float alpha = 0.0;
				float3 col = tex2D(_Diffuse, IN.uv_Diffuse.xy).rgb;
				
				// Greyscale intensitity constant
				float3 gsik = dot(float3(0.3, 0.59, 0.11), col);	
				
				// Saturate color, add brightness and add tint
				col = _Saturation * gsik + col * (1.0 - _Saturation);
				col = 0.5 + (col * _Brightness) * 0.5;
				col = col * _Color.rgb;
				
				alpha = _Color.a;
				
				o.Alpha = alpha;
				o.Emission = col;
			}
		ENDCG
		
		

		

	} 
	FallBack "Diffuse"
}
