//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTestPowerGenerator.cs
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


[RequireComponent(typeof(CPropulsionGeneratorBehaviour))]
public class CTestEngineBehaviour: MonoBehaviour 
{
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	public float m_MaxPropulsion = 150.0f;
	public CDUIConsole m_DUIConsole = null;
	
	public CComponentInterface m_MechanicalComponent1 = null;
	public CComponentInterface m_MechanicalComponent2 = null;
	
	public Transform m_OuterRing = null;
	public Transform m_MiddleRing = null;
	public Transform m_InnerRing = null;
	
	public float m_AverageAnimationSpeed = 360.0f; // Deg per second
	
	private float m_VarianceTimer1 = 0.0f;
	private float m_VarianceTimer2 = 0.0f;
	private float m_VarianceTimer3 = 0.0f;

	private CPropulsionGeneratorBehaviour m_PropulsionGenerator = null;
	private CDUIPropulsionEngineRoot m_DUIPropulsionRoot = null;


	// Member Properties
	
	
	// Member Methods
	public void Start()
	{
		m_PropulsionGenerator = gameObject.GetComponent<CPropulsionGeneratorBehaviour>();

		// Register for changes in mechanical health
		m_MechanicalComponent1.EventHealthChange += HandleMechanicalHealthChange;
		m_MechanicalComponent2.EventHealthChange += HandleMechanicalHealthChange;

		// Get the DUI of the power generator
		m_DUIPropulsionRoot = m_DUIConsole.DUI.GetComponent<CDUIPropulsionEngineRoot>();
		m_DUIPropulsionRoot.RegisterPropulsionEngine(gameObject);

        gameObject.GetComponent<CAudioCue>().Play(0.1f, true, 0);

		if(CNetwork.IsServer)
		{
			// Set the generation rate
			m_PropulsionGenerator.PropulsionForce = m_MaxPropulsion;
			m_PropulsionGenerator.PropulsionPotential = m_MaxPropulsion;
		}

		// Set the cubemap for the children
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.SetTexture("_Cube", transform.parent.GetComponent<CModulePortInterface>().CubeMapSnapshot);
		}
	}

	private void HandleMechanicalHealthChange(CComponentInterface _Component, CActorHealth _ComponentHealth)
	{
		if(CNetwork.IsServer)
		{
			// Get the combined health of the mechanical components
			float currentCombinedHealth = 0.0f;
			float combinedInitialHealth = 0.0f;

			currentCombinedHealth += m_MechanicalComponent1.GetComponent<CActorHealth>().health;
			currentCombinedHealth += m_MechanicalComponent2.GetComponent<CActorHealth>().health;

			combinedInitialHealth += m_MechanicalComponent1.GetComponent<CActorHealth>().health_initial;
			combinedInitialHealth += m_MechanicalComponent2.GetComponent<CActorHealth>().health_initial;

			m_PropulsionGenerator.PropulsionForce = m_MaxPropulsion * (currentCombinedHealth / combinedInitialHealth);
		}
	}

	private void Update()
	{
		UpdateAnimation();
	}

	private void UpdateAnimation()
	{
		float currentSpeed = m_AverageAnimationSpeed * (m_PropulsionGenerator.PropulsionForce / m_PropulsionGenerator.PropulsionPotential);

		m_VarianceTimer1 += Time.deltaTime * currentSpeed * Mathf.Deg2Rad * 0.75f;
		m_VarianceTimer2 += Time.deltaTime * currentSpeed * Mathf.Deg2Rad * 0.5f;
		m_VarianceTimer3 += Time.deltaTime * currentSpeed * Mathf.Deg2Rad * 0.25f;
		
		// Outer ring
		float angle = currentSpeed * 0.5f * Time.deltaTime;
		Vector3 axis = new Vector3(Mathf.Sin(m_VarianceTimer3), Mathf.Cos(m_VarianceTimer3), 0.0f).normalized;
		m_OuterRing.transform.Rotate(axis, angle);
		
		// Middle ring
		angle = currentSpeed * Time.deltaTime;
		axis = new Vector3(Mathf.Cos(m_VarianceTimer2), Mathf.Sin(m_VarianceTimer2), 0.0f).normalized;
		m_MiddleRing.transform.Rotate(axis, angle);
		
		// Inner ring
		angle = currentSpeed * 2.0f * Time.deltaTime;
		axis = new Vector3(Mathf.Sin(m_VarianceTimer1), -Mathf.Cos(m_VarianceTimer1), 0.0f).normalized;
		m_InnerRing.transform.Rotate(axis, angle);
	}
}
