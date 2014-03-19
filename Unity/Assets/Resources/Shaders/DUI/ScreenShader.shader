Shader "ScreenShader"
{
	Properties 
	{
		_BG("Background", 2D) = "black" {}
		_BGAlphaMask("Background Alpha Mask", 2D) = "black" {}
		_BGAlpha("Background Alpha", Float) = 0.9
		_UI("UI Texture", 2D) = "black" {}
		_UIAlpha("UI Alpha", Float) = 0.9
		_UIBrightness("UI Brightness", Float) = 1.5
	}
	
	SubShader 
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		
		CGPROGRAM
			#pragma surface surf BlinnPhong alpha

			sampler2D _BG;
			sampler2D _BGAlphaMask;
			float _BGAlpha;
			sampler2D _UI;
			float _UIAlpha;
			float _UIBrightness;
			
			struct Input 
			{
				float2 uv_BG;
				float2 uv_BGAlphaMask;
				float2 uv_UI;
			};

			void surf (Input IN, inout SurfaceOutput o) 
			{
				float4 uiCol = tex2D(_UI, IN.uv_UI);
				uiCol = saturate(uiCol * _UIBrightness);
				
				o.Emission = uiCol.rgb;
				o.Alpha = _UIAlpha * uiCol.a;
			}
		ENDCG
	}
	Fallback "Diffuse"
}