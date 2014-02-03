//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CCompoundCameraSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


[RequireComponent(typeof(CGalaxy))]
public class CGalaxySkybox : MonoBehaviour 
{
	// Member Types
	public enum ESkybox
	{
		Composite,
		Solid,

		MAX
	}
	
	// Member Delegates & Events
	
	
	// Member Fields
	private CGalaxy m_Galaxy = null;

	private Cubemap[] mSkyboxes = new Cubemap[(uint)ESkybox.MAX];


	// Member Properties


	// Member Method
	public void Start()
	{
		m_Galaxy = GetComponent<CGalaxy>();

        // Fog and skybox are controlled by the galaxySkybox.
        RenderSettings.fog = false;
        RenderSettings.skybox = null;

        // Load skyboxes.
        string[] skyboxFaces = new string[6];
        skyboxFaces[0] = "Left";
        skyboxFaces[1] = "Right";
        skyboxFaces[2] = "Down";
        skyboxFaces[3] = "Up";
        skyboxFaces[4] = "Front";
        skyboxFaces[5] = "Back";

        Profiler.BeginSample("Initialise cubemap from 6 textures");
        for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
        {
            for (uint uiFace = 0; uiFace < 6; ++uiFace)  // For each face on the skybox...
            {
                Texture2D skyboxFace = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + skyboxFaces[uiFace], typeof(Texture2D)) as Texture2D;  // Load the texture from file.
                if (!mSkyboxes[uiSkybox])
                    mSkyboxes[uiSkybox] = new Cubemap(skyboxFace.width, skyboxFace.format, false);
                mSkyboxes[uiSkybox].SetPixels(skyboxFace.GetPixels(), (CubemapFace)uiFace);
                Resources.UnloadAsset(skyboxFace);
            }

            mSkyboxes[uiSkybox].Apply(false, true);
        }
        Profiler.EndSample();

		//Profiler.BeginSample("Load cubemaps");
		//for (uint uiSkybox = 0; uiSkybox < (uint)ESkybox.MAX; ++uiSkybox)    // For each skybox...
		//    mSkyboxes[uiSkybox] = Resources.Load("Textures/Galaxy/" + uiSkybox.ToString() + "Cubemap", typeof(Cubemap)) as Cubemap;  // Load the cubemap texture from file.
		//Profiler.EndSample();

		// Update the galaxy aesthetic
		UpdateGalaxyAesthetic(m_Galaxy.centreCell);
	}

	private void UpdateGalaxyAesthetic(CGalaxy.SCellPos absoluteCell)
	{
		Profiler.BeginSample("UpdateGalaxyAesthetic");
		
		// Skybox.
		Shader.SetGlobalTexture("void_Skybox1", mSkyboxes[(uint)ESkybox.Solid]);
		
		if (RenderSettings.skybox == null)
			RenderSettings.skybox = new Material(Shader.Find("VOID/MultitexturedSkybox"));
		RenderSettings.skybox.SetVector("_Tint", Color.grey);
		
		Shader.SetGlobalFloat("void_FogStartDistance", 2000.0f);
		Shader.SetGlobalFloat("void_FogEndDistance", 4000.0f);
		Shader.SetGlobalFloat("void_FogDensity", 0.01f);
		
		// Calculate perspective warp.
		Camera camera = Camera.current;
		if (camera)
		{
			float CAMERA_NEAR = camera.nearClipPlane;
			float CAMERA_FAR = camera.farClipPlane;
			float CAMERA_FOV = camera.fieldOfView;
			float CAMERA_ASPECT_RATIO = camera.aspect;
			
			float fovWHalf = CAMERA_FOV * 0.5f;
			
			Vector3 toTop = camera.transform.up * CAMERA_NEAR * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);
			Vector3 toRight = toTop * CAMERA_ASPECT_RATIO;
			
			Vector3 topLeft = camera.transform.forward * CAMERA_NEAR - toRight + toTop;
			float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR / CAMERA_NEAR;
			
			topLeft.Normalize();
			topLeft *= CAMERA_SCALE;
			
			Vector3 topRight = (camera.transform.forward * CAMERA_NEAR + toRight + toTop).normalized * CAMERA_SCALE;
			Vector3 bottomRight = (camera.transform.forward * CAMERA_NEAR + toRight - toTop).normalized * CAMERA_SCALE;
			Vector3 bottomLeft = (camera.transform.forward * CAMERA_NEAR - toRight - toTop).normalized * CAMERA_SCALE;
			
			Shader.SetGlobalVector("void_FrustumCornerTopLeft", topLeft);
			Shader.SetGlobalVector("void_FrustumCornerTopRight", topRight);
			Shader.SetGlobalVector("void_FrustumCornerBottomRight", bottomRight);
			Shader.SetGlobalVector("void_FrustumCornerBottomLeft", bottomLeft);
			Shader.SetGlobalFloat("void_CameraScale", CAMERA_SCALE);
		}
		
		Profiler.EndSample();
	}
}
