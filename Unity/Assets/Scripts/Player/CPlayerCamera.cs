//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CPlayerCamera : MonoBehaviour 
{
	
// Member Fields
	private Texture m_CrosshairTexture = null;
		
// Member Methods
	void Awake()
	{
		// Get the crosshair texture
		m_CrosshairTexture = (Texture)Resources.Load("Prefabs/Player/recticle");
		
		// Disable the current camera
        Camera.main.enabled = false;
		
		// Add the camera component
		gameObject.AddComponent<Camera>();
		
		// Configure the camerea
		gameObject.camera.cullingMask &= ~(1 << LayerMask.NameToLayer("DUI"));
		gameObject.camera.farClipPlane = 5000.0f;
	}
	
	void OnGUI()
	{
		Rect textureRect = new Rect(Screen.width * 0.5f - (m_CrosshairTexture.width * 0.5f), Screen.height * 0.5f - (m_CrosshairTexture.height * 0.5f), 
									m_CrosshairTexture.width, m_CrosshairTexture.height);
		
		GUI.DrawTexture(textureRect, m_CrosshairTexture);
	}
}
