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
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CModulePrecipitation : CNetworkMonoBehaviour
{
	
// Member Types


    const float k_fProgressIncrementRate = 0.1f;
	
	
// Member Delegates & Events


// Member Properties


    public float ProgressRatio
    {
        get { return (m_fProgressRatio); }
    }


	public bool IsCompleted
	{
		get { return(m_cPrecipitativeMesh == null); }
	}

	
// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        // Empty
    }


    public void SetProgressRatio(float _fRatio)
    {
        if (_fRatio > 1.0f ||
            _fRatio < 0.0f)
        {
            Debug.LogError("Invalid built ratio: " + _fRatio);
        }

        m_fTargetProgressRatio = _fRatio;
    }


    void Start()
    {
        if (m_cPrecipitativeMesh == null)
        {
            Debug.LogError(string.Format("GameObject({0}) does not have a precipitative object.", gameObject.name));
        }

        if (!GetComponent<CModuleInterface>().IsBuilt)
        {
            // Create the module hologram object
            m_cPrecipitativeMesh = (GameObject)GameObject.Instantiate(m_cPrecipitativeMesh);
            m_cPrecipitativeMesh.transform.parent = transform;
            m_cPrecipitativeMesh.transform.localPosition = Vector3.zero;
            m_cPrecipitativeMesh.transform.localRotation = Quaternion.identity;

            m_bInstanced = true;
        }
    }
	

	void Update()
	{
        if (m_bInstanced &&
            m_fProgressRatio < m_fTargetProgressRatio)
        {
            m_fProgressRatio += k_fProgressIncrementRate * Time.deltaTime;

            if (m_fProgressRatio > m_fTargetProgressRatio)
            {
                m_fProgressRatio = m_fTargetProgressRatio;
            }

            if (m_fTargetProgressRatio >= 1.0f)
            {
                m_fProgressRatio = 1.0f;

                OnPrecipitationFinish();
            }
            else
            {
                m_cPrecipitativeMesh.renderer.material.SetFloat("_Amount", (float)m_fProgressRatio);
            }
        }
	}
	

    [ALocalOnly]
	void OnPrecipitationFinish()
	{
		// Destroy the hologram mesh
        Destroy(m_cPrecipitativeMesh);
        m_cPrecipitativeMesh = null;
	}


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        // Empty
    }


// Member Fields


    public GameObject m_cPrecipitativeMesh = null;

    float m_fProgressRatio       = 0.0f;
    float m_fTargetProgressRatio = 0.0f;

    bool m_bInstanced = false;


};
