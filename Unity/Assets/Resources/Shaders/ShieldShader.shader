Shader "VOID/Shield Shader"
{
	Properties 
	{
		_EmissiveTex("Emissive Texture", 2D) = "black" {}
		_EmissivePower("Emissive Power", Float) = 2.0
		_Alpha("Alpha", Float) = 0.9
		_TimeAlive("Alive Time", Range(0.0, 1.0)) = 0.0
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
			#pragma surface surf BlinnPhong alpha vertex:vert

			sampler2D _EmissiveTex;
			float _EmissivePower;
			float _Alpha;
			float _TimeAlive;
			
			struct Input 
			{
				float2 uv_EmissiveTex;
				float3 worldPos;
			};
			
			void vert(inout appdata_full v, out Input o) 
			{
          		UNITY_INITIALIZE_OUTPUT(Input, o);;
			}

			void surf(Input IN, inout SurfaceOutput o) 
			{
				float4 Col = tex2D(_EmissiveTex, IN.uv_EmissiveTex);
				Col = Col * _EmissivePower;
				
				// Fade the color away at the edges
				float2 nuv = float2((IN.uv_EmissiveTex.x * 2.0) - 1.0, (IN.uv_EmissiveTex.y * 2.0) - 1.0);
				float lennuv = length(nuv);
				float modifier = sin((lennuv + 0.3) * 5.0 * (1.0 - _TimeAlive));//(1.0 - _TimeAlive));
				
				if(_TimeAlive < 0.2)
				{
					modifier *= _TimeAlive / 0.2;
				}
				
				Col *= modifier;
				
				o.Emission = Col.rgb;
				o.Alpha = _Alpha * Col.a;
			}
		ENDCG
	}
	Fallback "Diffuse"
}