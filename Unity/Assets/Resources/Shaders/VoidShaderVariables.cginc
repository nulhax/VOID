#ifndef VOID_SHADER_VARIABLES_INCLUDED
#define VOID_SHADER_VARIABLES_INCLUDED

#include "UnityCG.cginc"

CBUFFER_START(VoidVariables)
	uniform float		void_FogStartDistance;
	uniform float		void_FogEndDistance;
	uniform float		void_FogDensity;
	
	uniform samplerCUBE	void_Skybox1;
	uniform samplerCUBE	void_Skybox2;
CBUFFER_END

float4 void_SampleFog(fixed4 screenPos, fixed3 worldPos, fixed4 sourceColour)
{
	fixed3 vecCameraToPos = worldPos - _WorldSpaceCameraPos;
	fixed3 dirCameraToPos = normalize(vecCameraToPos);
	fixed distFromCamera = length(vecCameraToPos);
	fixed deltaFogStartEnd = void_FogEndDistance - void_FogStartDistance;
	fixed fogIntensity = deltaFogStartEnd ? 1.0f - saturate((void_FogEndDistance - distFromCamera) / deltaFogStartEnd) : 0.0f;
	return lerp(sourceColour, texCUBE(void_Skybox1, dirCameraToPos), fogIntensity);
}

#endif	// VOID_SHADER_VARIABLES_INCLUDED