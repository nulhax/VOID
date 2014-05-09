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


[RequireComponent(typeof(CPowerGeneratorInterface))]
public class CPowerGeneratorSmallBehaviour: MonoBehaviour 
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
	}


	void Start()
	{
        m_cModuleInterface = GetComponent<CModuleInterface>();

		// Set the cubemap for the children
		foreach (Renderer r in GetComponentsInChildren<Renderer>())
		{
			//r.material.SetTexture("_Cube", transform.parent.GetComponent<CModulePortInterface>().CubeMapSnapshot);
		}

		// Begin playing the sound.
		// Todo: Once individual sounds can be disabled, this must be moved to where the power generator turns on and off.
		GetComponent<CAudioCue>().Play(transform, 0.25f, true, 0);

        m_vSphereTopPos = m_cTransSphere.transform.position;
        m_vSphereBottomPos = m_vSphereTopPos - new Vector3(0.0f, 0.3f, 0.0f);
	}


    void Update()
    {
        UpdateAnaimation();
    }


    void UpdateAnaimation()
    {
        if (!GetComponent<CModuleInterface>().IsBuilt)
            return;
        
        m_cTransInnterRing.Rotate(Vector3.up, 180.0f * Time.deltaTime);
        m_cTransOuterRing.Rotate(Vector3.up, 90.0f * Time.deltaTime);

        if (m_bSphereDown)
        {
            m_cTransSphere.position = Vector3.MoveTowards(m_cTransSphere.position, m_vSphereBottomPos, 0.1f * Time.deltaTime);

            if (m_cTransSphere.position == m_vSphereBottomPos)
            {
                m_bSphereDown = false;
            }
        }
        else
        {
            m_cTransSphere.position = Vector3.MoveTowards(m_cTransSphere.position, m_vSphereTopPos, 0.1f * Time.deltaTime);

            if (m_cTransSphere.position == m_vSphereTopPos)
            {
                m_bSphereDown = true;
            }
        }
    }


    [AServerOnly]
    void OnEventComponentHealthChange(CActorHealth _cSender, float _fPreviousHealth, float _fNewHealth)
	{
		if (CNetwork.IsServer)
		{
            //m_cModuleInterface.SetFuntionalRatio(_fNewHealth / _cSender.health_initial);
		}
	}



// Member Fields


    public Transform m_cTransSphere = null;
    public Transform m_cTransInnterRing = null;
    public Transform m_cTransOuterRing  = null;

    CModuleInterface m_cModuleInterface = null;

    Vector3 m_vSphereTopPos    = Vector3.zero;
    Vector3 m_vSphereBottomPos = Vector3.zero;

    bool m_bSphereDown = true;


}
