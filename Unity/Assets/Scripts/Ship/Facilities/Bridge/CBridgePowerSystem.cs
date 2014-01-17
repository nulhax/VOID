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


[RequireComponent(typeof(CPowerGeneratorSystem))]
public class CBridgePowerSystem: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_PowerGenerationRate = 15.0f;
	public float m_PowerBatteryCapacity = 1000.0f;

	public GameObject m_AttachedFuseBox = null;
	
	private float m_PrevPowerGenerationRate = 0.0f;
	private float m_PrevPowerBatteryCapacity = 0.0f;
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		DebugAddPowerGeneratorLabel();

		// Register for when the fusebox breaks/fixes
		CFuseBoxControl fbc = m_AttachedFuseBox.GetComponent<CFuseBoxControl>();
		fbc.EventBroken += HandleFuseBoxBreaking;
		fbc.EventFixed += HandleFuseBoxFixing;

		// Debug: Set the charge to half its total capacity
		if(CNetwork.IsServer)
		{
			CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();

			powerGenSystem.BatteryCapacity = m_PowerBatteryCapacity;
			powerGenSystem.BatteryCharge = powerGenSystem.BatteryCapacity / 2;

			m_PrevPowerBatteryCapacity = m_PowerBatteryCapacity;
		}
	}
	
	public void Update()
	{
		if(CNetwork.IsServer)
		{
			if(m_PrevPowerGenerationRate != m_PowerGenerationRate)
			{
				CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();
				
				powerGenSystem.PowerGenerationRate = m_PowerGenerationRate;
			
				m_PrevPowerGenerationRate = m_PowerGenerationRate;
			}

			if(m_PrevPowerBatteryCapacity != m_PowerBatteryCapacity)
			{
				CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();

				powerGenSystem.BatteryCapacity = m_PowerBatteryCapacity;

				m_PrevPowerBatteryCapacity = m_PowerBatteryCapacity;
			}
		}
	}

	private void HandleFuseBoxBreaking(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();

			powerGenSystem.DeactivatePowerGeneration();
		}
	}

	private void HandleFuseBoxFixing(GameObject _FuseBox)
	{
		if(CNetwork.IsServer)
		{
			CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();
			
			powerGenSystem.ActivatePowerGeneration();
		}
	}
	
	private void DebugAddPowerGeneratorLabel()
	{
		// Add the mesh renderer
		MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
		mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));
		
		// Add the text mesh
		TextMesh textMesh = gameObject.AddComponent<TextMesh>();
		textMesh.fontSize = 72;
		textMesh.characterSize = 0.10f;
		textMesh.color = Color.green;
		textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.offsetZ = -0.01f;
		textMesh.fontStyle = FontStyle.Italic;
		textMesh.text = gameObject.name;
	}
}
