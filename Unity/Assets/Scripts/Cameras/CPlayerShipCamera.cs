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


public class CPlayerShipCamera : MonoBehaviour 
{
	
// Member Fields
	public Texture m_CrosshairTexture = null;
		
// Member Methods
	private void Awake()
	{
		//camera.enabled = true;
	}
	
	private void Start()
	{
		//camera.enabled = true;
	}
	
	private void OnGUI()
	{
		Rect textureRect = new Rect(Screen.width * 0.5f - (m_CrosshairTexture.width * 0.5f), Screen.height * 0.5f - (m_CrosshairTexture.height * 0.5f), 
									m_CrosshairTexture.width, m_CrosshairTexture.height);
		
		GUI.DrawTexture(textureRect, m_CrosshairTexture);
	}
}
