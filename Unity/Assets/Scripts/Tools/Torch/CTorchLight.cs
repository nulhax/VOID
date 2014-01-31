//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTorchLight.cs
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

[RequireComponent(typeof(CToolInterface))]
public class CTorchLight : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_bTorchLit = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, true);
		m_bTorchColour = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 0);
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
		if (_cVarInstance == m_bTorchLit)
		{
			if (!m_bTorchLit.Get())
			{
				light.intensity = 0;
			}
			else
			{
				light.intensity = 2;
			}			

		}
		else if (_cVarInstance == m_bTorchColour)
		{
			switch (m_bTorchColour.Get())
			{
				case 0:
					light.color = new Color(174.0f / 255.0f, 208.0f / 255.0f, 1.0f);
					break;
				case 1:
					light.color = new Color(1.0f, 0, 0);
					break;
				case 2:
					light.color = new Color(0, 1.0f, 0);
					break;
				case 3:
					light.color = new Color(0, 0, 1.0f);
					break;
			}
		}
    }


	public void Start()
	{
		gameObject.GetComponent<CToolInterface>().EventPrimaryActivate += ToggleActivate;
		gameObject.GetComponent<CToolInterface>().EventSecondaryActivate += ToggleColour;

		if (CNetwork.IsServer)
		{
			m_bTorchLit.Set(false);
		}
		
		//Get audio cues
		CAudioCue[] cues = gameObject.GetComponents<CAudioCue>();
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{
	}


	void ToggleActivate(GameObject _cInteractableObject)
    {
        if (!m_bTorchLit.Get())
        {
            m_bTorchLit.Set(true);		
        }
        else
        {
            m_bTorchLit.Set(false);			
	    }
    }


	void ToggleColour(GameObject _cInteractableObject)
    {
		byte bNextNumber = (byte)(m_bTorchColour.Get() + 1);


		if (bNextNumber > 3)
		{
			bNextNumber = 0;
		}

		m_bTorchColour.Set(bNextNumber);		
	}


// Member Fields


    CNetworkVar<bool> m_bTorchLit = null;
	CNetworkVar<byte> m_bTorchColour = null;
};