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
	public GameObject m_PowerGeneratorStation = null;
	
	private float m_PrevPowerGenerationRate = 0.0f;
	private float m_PrevPowerBatteryCapacity = 0.0f;
	
	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		DebugAddPowerGeneratorLabel();

		CPowerGeneratorSystem powerGenSystem = gameObject.GetComponent<CPowerGeneratorSystem>();

		// Debug: Set the charge to half its total capacity
		powerGenSystem.BatteryCapacity = m_PowerBatteryCapacity;
		powerGenSystem.BatteryCharge = powerGenSystem.BatteryCapacity / 2;

		m_PrevPowerBatteryCapacity = m_PowerBatteryCapacity;
	}
	
	public void Update()
	{
		if(!CNetwork.IsServer)
			return;

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
	
	private void DebugAddPowerGeneratorLabel()
	{
		// Add the mesh renderer
		MeshRenderer mr = m_PowerGeneratorStation.AddComponent<MeshRenderer>();
		mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));
		
		// Add the text mesh
		TextMesh textMesh = m_PowerGeneratorStation.AddComponent<TextMesh>();
		textMesh.fontSize = 72;
		textMesh.characterSize = 0.10f;
		textMesh.color = Color.green;
		textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.offsetZ = -0.01f;
		textMesh.fontStyle = FontStyle.Italic;
		textMesh.text = m_PowerGeneratorStation.name;
	}
}
