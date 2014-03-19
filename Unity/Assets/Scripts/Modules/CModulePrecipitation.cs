//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModulePrecipitation.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CModulePrecipitation : MonoBehaviour
{
	
	// Member Types
	
	
	// Member Delegates & Events


	// Member Fields
	public GameObject m_PrecipitativeMesh = null;
	public float m_BuildTime = 20.0f;

	private float m_Timer = 0.0f;


	// Member Properties
	public bool IsModuleBuilt
	{
		get { return(m_PrecipitativeMesh == null); }
	}

	
	// Member Methods
	void Start()
	{
		// Disable all children except for the precipitation mesh
		foreach(Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}

		// Create the module precipitation object
		m_PrecipitativeMesh = (GameObject)GameObject.Instantiate(m_PrecipitativeMesh);
		m_PrecipitativeMesh.transform.parent = transform;
		m_PrecipitativeMesh.transform.localPosition = Vector3.zero;
		m_PrecipitativeMesh.transform.localRotation = Quaternion.identity;
	}
	
	void Update()
	{
		if(m_Timer != m_BuildTime)
		{
			m_Timer += Time.deltaTime;
			m_Timer = Mathf.Clamp(m_Timer, 0.0f, m_BuildTime);

			m_PrecipitativeMesh.renderer.material.SetFloat("_Amount", m_Timer/m_BuildTime);
		}
		else
		{
			OnPrecipitationFinish();
		}
	}
	
	void OnPrecipitationFinish()
	{
		// Enable all the children
		foreach(Transform child in transform)
		{
			child.gameObject.SetActive(true);
		}

		// Destroy the precipitation mesh
		Destroy(m_PrecipitativeMesh);
		m_PrecipitativeMesh = null;
	}
};
