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


[RequireComponent(typeof(CEngineInterface))]
public class CEngineSmallBehaviour: MonoBehaviour 
{

// Member Types
	
	
// Member Delegates & Events


// Member Properties


// Member Methods


	void Awake()
	{
		CAudioCue audioCue = GetComponent<CAudioCue>();
		if (audioCue == null)
			audioCue = gameObject.AddComponent<CAudioCue>();
		//m_AmbientHumSoundIndex = audioCue.AddSound("Audio/SmallEngineAmbientHum", 0.0f, 0.0f, true);
	}


	void Start()
	{
		m_cEngineInterface = gameObject.GetComponent<CEngineInterface>();


		// Set the cubemap for the children
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.material.SetTexture("_Cube", gameObject.GetComponent<CModuleInterface>().CubeMapSnapshot);
		}

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the engine turns on and off.
		GetComponent<CAudioCue>().Play(0.15f, true, 0);
	}


    void OnEventComponentHealthChange(CActorHealth _cSender, float _fPreviousHealth, float _fNewHealth)
    {
		if(CNetwork.IsServer)
		{
			// Get the combined health of the mechanical components
            //m_cEngineInterface.SetPropulsion( m_cEngineInterface.m_fInitialPropulsion * (currentCombinedHealth / combinedInitialHealth) );
		}
	}


	void Update()
	{
		if(gameObject.GetComponent<CModulePrecipitation>().IsCompleted)
			UpdateAnimation();
	}


	void UpdateAnimation()
	{
		float currentSpeed = m_AverageAnimationSpeed * (m_cEngineInterface.PropulsionForce / m_cEngineInterface.m_fInitialPropulsion);

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

        if (gameObject.GetComponentInChildren<Light>() != null)
        gameObject.GetComponentInChildren<Light>().intensity = (currentSpeed / 15.0f);
	}

    
// Member Fields


    public Transform m_OuterRing = null;
    public Transform m_MiddleRing = null;
    public Transform m_InnerRing = null;

    public float m_AverageAnimationSpeed = 360.0f; // Deg per second


    float m_VarianceTimer1 = 0.0f;
    float m_VarianceTimer2 = 0.0f;
    float m_VarianceTimer3 = 0.0f;

    CEngineInterface m_cEngineInterface = null;

    int m_AmbientHumSoundIndex = -1;


}
