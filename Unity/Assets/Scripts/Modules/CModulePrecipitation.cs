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


public class CModulePrecipitation : CNetworkMonoBehaviour
{
	
// Member Types
	
	
// Member Delegates & Events


// Member Fields


	public GameObject m_PrecipitativeMesh = null;

	private GameObject m_PrecipitativeObject = null;
    private CNetworkVar<byte> m_BuiltRatio = null;


// Member Properties


    public float BuiltRatio
    {
        get { return (m_BuiltRatio.Get()); }
    }


	public bool IsModuleBuilt
	{
		get { return(m_PrecipitativeMesh == null); }
	}

	
// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_BuiltRatio = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 0);
    }


    [AServerOnly]
    public void SetBuiltRatio(float _fRatio)
    {
        if (_fRatio > 1.0f ||
            _fRatio < 0.0f)
        {
            Debug.LogError("Invalid built ratio: " + _fRatio.ToString());
        }

        m_BuiltRatio.Set((byte)(_fRatio * 200.0f));
    }


	void Awake()
	{
		// Disable all children except for the precipitation mesh
		foreach(Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}

		// Create the module precipitation object
		m_PrecipitativeObject = (GameObject)GameObject.Instantiate(m_PrecipitativeMesh);
		m_PrecipitativeObject.transform.parent = transform;
		m_PrecipitativeObject.transform.localPosition = Vector3.zero;
		m_PrecipitativeObject.transform.localRotation = Quaternion.identity;
	}
	

	void Update()
	{
        // Empty
	}
	

    [AClientOnly]
	void OnPrecipitationFinish()
	{
		// Enable all the children
		foreach(Transform child in transform)
		{
			child.gameObject.SetActive(true);
		}

		// Destroy the precipitation mesh
		Destroy(m_PrecipitativeObject);
		m_PrecipitativeObject = null;
	}


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        if (_cSynedVar == m_BuiltRatio)
        {
            if (m_BuiltRatio.Get() == 200)
            {
                OnPrecipitationFinish();
            }
            else
            {
                m_PrecipitativeMesh.renderer.material.SetFloat("_Amount", (float)m_BuiltRatio.Get() / 200.0f);
            }
        }
    }
};
