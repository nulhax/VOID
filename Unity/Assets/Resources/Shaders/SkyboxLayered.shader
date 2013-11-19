Shader "VOID/SkyboxLayered" {
Properties {
	_Blend("Blend", Range(0.0,1.0)) = 0.0
	_Tint ("Tint Color", Color) = (.5, .5, .5, .5)
	_FrontTex1 ("Front1 (+Z)", 2D) = "white" {}
	_BackTex1 ("Back1 (-Z)", 2D) = "white" {}
	_LeftTex1 ("Left1 (+X)", 2D) = "white" {}
	_RightTex1 ("Right1 (-X)", 2D) = "white" {}
	_UpTex1 ("Up1 (+Y)", 2D) = "white" {}
	_DownTex1 ("Down1 (-Y)", 2D) = "white" {}
	_FrontTex2 ("Front2 (+Z)", 2D) = "white" {}
	_BackTex2 ("Back2 (-Z)", 2D) = "white" {}
	_LeftTex2 ("Left2 (+X)", 2D) = "white" {}
	_RightTex2 ("Right2 (-X)", 2D) = "white" {}
	_UpTex2 ("Up2 (+Y)", 2D) = "white" {}
	_DownTex2 ("Down2 (-Y)", 2D) = "white" {}
}

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }
	
	CGINCLUDE
	#include "UnityCG.cginc"

	fixed4 _Tint;
	
	struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	struct v2f {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};
	v2f vert (appdata_t v)
	{
		v2f o;
		o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		o.texcoord = v.texcoord;
		return o;
	}
	fixed4 skybox_frag (v2f i, sampler2D smp)
	{
		fixed4 tex = tex2D (smp, i.texcoord);
		fixed4 col;
		col.rgb = tex.rgb + _Tint.rgb - unity_ColorSpaceGrey;
		col.a = tex.a * _Tint.a;
		return col;
	}
	ENDCG
	
	Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _FrontTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_FrontTex1); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _BackTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_BackTex1); }
		ENDCG 
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _LeftTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_LeftTex1); }
		ENDCG
	}
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _RightTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_RightTex1); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _UpTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_UpTex1); }
		ENDCG
	}	
	Pass{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _DownTex1;
		fixed4 frag (v2f i) : COLOR { return skybox_frag(i,_DownTex1); }
		ENDCG
	}
}	

SubShader {
	Tags { "Queue"="Background" "RenderType"="Background" }
	Cull Off ZWrite Off Fog { Mode Off }
	Color [_Tint]
	Pass {
		SetTexture [_FrontTex1] { combine texture +- primary, texture * primary }
	}
	Pass {
		SetTexture [_BackTex1]  { combine texture +- primary, texture * primary }
	}
	Pass {
		SetTexture [_LeftTex1]  { combine texture +- primary, texture * primary }
	}
	Pass {
		SetTexture [_RightTex1] { combine texture +- primary, texture * primary }
	}
	Pass {
		SetTexture [_UpTex1]    { combine texture +- primary, texture * primary }
	}
	Pass {
		SetTexture [_DownTex1]  { combine texture +- primary, texture * primary }
	}
}
}