//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPrecipitationBheaviour.cs
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


public class CPrecipitativeMeshBehaviour : MonoBehaviour
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
        get { return (m_fTargetProgressRatio == 1.0f); }
	}

	
// Member Methods


    public void SetProgressRatio(float _fRatio)
    {
        _fRatio = Mathf.Clamp(_fRatio, 0.0f, 1.0f);

        m_fTargetProgressRatio = _fRatio;
    }


    void Start()
    {
        // Empty
    }
	

	void Update()
	{
        /*
        if (m_fProgressRatio < m_fTargetProgressRatio)
        {
            m_fProgressRatio += k_fProgressIncrementRate * Time.deltaTime;

            if (m_fProgressRatio > m_fTargetProgressRatio)
            {
                m_fProgressRatio = m_fTargetProgressRatio;
            }

            if (m_fTargetProgressRatio >= 1.0f)
            {
                //m_fProgressRatio = 1.0f;
            }
            else
            {
                renderer.material.SetFloat("_Amount", m_fProgressRatio);
            }
        }
        else
         * */
        {
            m_fProgressRatio = m_fTargetProgressRatio;
            renderer.material.SetFloat("_Amount", m_fTargetProgressRatio);
        }
	}
	

    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        // Empty
    }


// Member Fields


    public ParticleSystem m_cParticles = null;


    float m_fProgressRatio       = 0.0f;
    float m_fTargetProgressRatio = 0.0f;


};
