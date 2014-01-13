//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRachetControl.cs
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
public class CRachetControl : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_bActive = new CNetworkVar<bool>(OnNetworkVarSync, false);
	}


	public void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_bActive)
		{
			if (m_bActive.Get())
			{
				m_fActiveTimer = 0.0f;
			}
			else
			{
				m_fDeactiveTimer = 0.0f;
			}
		}
	}


	public void Start()
	{
		gameObject.GetComponent<CToolInterface>().EventPrimaryActivate   += new CToolInterface.NotifyPrimaryActivate(OnUseStart);
		gameObject.GetComponent<CToolInterface>().EventPrimaryDeactivate += new CToolInterface.NotifyPrimaryDeactivate(OnUseEnd);
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
		if (gameObject.GetComponent<CToolInterface>().IsHeld)
		{
			if (m_bActive.Get())
			{
				
				m_fActiveTimer += Time.deltaTime * 3;

				transform.localPosition = Vector3.Lerp(s_vDeactivePosition, s_vActivePosition, m_fActiveTimer);
			}
			else
			{
				m_fDeactiveTimer += Time.deltaTime * 3;

				transform.localPosition = Vector3.Lerp(s_vActivePosition, s_vDeactivePosition, m_fDeactiveTimer);
			}
		}
	}


	[AServerMethod]
	public void OnUseStart(GameObject _cInteractableObject)
	{
		if (_cInteractableObject != null)
		{
			CPanelInterface cPanelInterface = _cInteractableObject.GetComponent<CPanelInterface>();

			if (cPanelInterface != null)
			{
				switch (cPanelInterface.PanelType)
				{
					case CPanelInterface.EType.FuseBox:
						HandleFuseBoxInteraction(_cInteractableObject); 
						break;
				}
			}
		}
	}


	[AServerMethod]
	public void OnUseEnd()
	{
		m_bActive.Set(false);
	}


	[AServerMethod]
	void HandleFuseBoxInteraction(GameObject _cFuseBox)
	{
		m_bActive.Set(true);

		if (_cFuseBox.GetComponent<CFuseBoxControl>().IsOpened)
		{
			_cFuseBox.GetComponent<CFuseBoxControl>().CloseFrontPlate();
		}
		else
		{
			_cFuseBox.GetComponent<CFuseBoxControl>().OpenFrontPlate();
		}
		
		//Debug.LogError("Interacted with fuse box");
	}


// Member Fields


	CNetworkVar<bool> m_bActive = null;


	float m_fActiveTimer = 0.0f;
	float m_fDeactiveTimer = 0.0f;


	static Vector3 s_vDeactivePosition = new Vector3(0.5f, 0.36f, 0.5f);
	static Vector3 s_vActivePosition = new Vector3(0.0f, 0.36f, 0.85f);


};
