#ifndef VOID_SHADER_VARIABLES_INCLUDED
#define VOID_SHADER_VARIABLES_INCLUDED

#include "UnityCG.cginc"

CBUFFER_START(VoidVariables)
	uniform float3		void_FrustumCornerTopLeft;
	uniform float3		void_FrustumCornerTopRight;
	uniform float3		void_FrustumCornerBottomRight;
	uniform float3		void_FrustumCornerBottomLeft;
	uniform float		void_FogStartDistance;
	uniform float		void_FogEndDistance;
	uniform float		void_FogStartDistanceInverse;
	uniform float		void_FogDensity;
	uniform float		void_CameraScale;
	
	uniform samplerCUBE	void_Skybox1;
	uniform samplerCUBE	void_Skybox2;
CBUFFER_END

float4 void_SampleFog(fixed4 screenPos, fixed3 viewDir, fixed4 sourceColour)	// Returns [red,green,blue,fogIntensity]
{
	//return float4(sourceColour, 0.0f);
	float2 screenUV = screenPos.xy / screenPos.w;
	float3 posTopX = void_FrustumCornerTopLeft + (void_FrustumCornerTopRight - void_FrustumCornerTopLeft) * screenUV.x;
	float3 posBottomX = void_FrustumCornerBottomLeft + (void_FrustumCornerBottomRight - void_FrustumCornerBottomLeft) * screenUV.x;
	float3 pos = posTopX + (posBottomX - posTopX) * screenUV.y;
	float distanceScalar = length(pos) * _ProjectionParams.w;
	float actualDistance = screenPos.z * distanceScalar;
	//float fogIntensity = saturate(actualDistance / _ProjectionParams.z);
	float fogIntensity = 1.0f - saturate((void_FogEndDistance - actualDistance) / (void_FogEndDistance - void_FogStartDistance));
	return float4(lerp(sourceColour, texCUBE(void_Skybox1, viewDir), fogIntensity));
}

#endif	// VOID_SHADER_VARIABLES_INCLUDED