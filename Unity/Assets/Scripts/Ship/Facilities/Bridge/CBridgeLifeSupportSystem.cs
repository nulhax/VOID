//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CBridgeLifeSupportSystem.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CLifeSupportSystem))]
public class CBridgeLifeSupportSystem: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_AtmosphereGenerationRate = 15.0f;
	public float m_AtmosphereCapacitySupport = 2000.0f;
	public GameObject m_LifeSupportStation = null;

	private float m_PrevAtmosphereGenerationRate = 0.0f;
	private float m_PrevAtmosphereCapacitySupport = 0.0f;

	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		DebugAddLifeSupportLabel();
	}

	public void Update()
	{
		if(!CNetwork.IsServer)
			return;

		if(m_PrevAtmosphereGenerationRate != m_AtmosphereGenerationRate || 
		   m_AtmosphereCapacitySupport != m_PrevAtmosphereCapacitySupport)
		{
			CLifeSupportSystem lifeSupportSystem = gameObject.GetComponent<CLifeSupportSystem>();

			lifeSupportSystem.AtmosphereGenerationRate = m_AtmosphereGenerationRate;
			lifeSupportSystem.AtmosphereCapacitySupport = m_AtmosphereCapacitySupport;

			m_PrevAtmosphereGenerationRate = m_AtmosphereGenerationRate;
			m_PrevAtmosphereCapacitySupport = m_AtmosphereCapacitySupport;
		}
	}

	private void DebugAddLifeSupportLabel()
	{
		// Add the mesh renderer
		MeshRenderer mr = m_LifeSupportStation.AddComponent<MeshRenderer>();
		mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));
		
		// Add the text mesh
		TextMesh textMesh = m_LifeSupportStation.AddComponent<TextMesh>();
		textMesh.fontSize = 72;
		textMesh.characterSize = 0.10f;
		textMesh.color = Color.green;
		textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.offsetZ = -0.01f;
		textMesh.fontStyle = FontStyle.Italic;
		textMesh.text = m_LifeSupportStation.name;
	}
}
