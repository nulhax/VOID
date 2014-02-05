//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CNaniteStorageBehaviour.cs
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


[RequireComponent(typeof(CModuleInterface))]
public class CNaniteStorageBehaviour : CNetworkMonoBehaviour 
{
	
	// Member Types
	
	
	// Member Delegates & Events
	
	
	// Member Fields
	CNetworkVar<int> m_iStoredNanites = null;
	CNetworkVar<int> m_iNaniteCapacity = null;
	CNetworkVar<bool> m_bNanitesAvailable = null;

	const int m_kiMaxNanites = 250;
	
	// Member Properties
	public int StoredNanites
	{ 
		get { return (m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iStoredNanites.Set(value); }
	}

	public int MaxNaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get()); }
		
		[AServerOnly]
		set
		{ 
			m_iNaniteCapacity.Set(value); 

			if(value < StoredNanites)
				StoredNanites = value;
		}
	}
	
	public int AvailableNaniteCapacity
	{ 
		get { return (m_iNaniteCapacity.Get() - m_iStoredNanites.Get()); }
		
		[AServerOnly]
		set { m_iNaniteCapacity.Set(value); }
	}
	
	public bool HasAvailableNanites
	{
		get { return (m_bNanitesAvailable.Get() && m_iStoredNanites.Get() != 0.0f); }
	}
	
	// Member Functions
	
	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_iStoredNanites= _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, 0);
		m_iNaniteCapacity= _cRegistrar.CreateNetworkVar<int>(OnNetworkVarSync, m_kiMaxNanites);
		m_bNanitesAvailable = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, false);
	}
	
	void OnNetworkVarSync(INetworkVar _VarInstance)
	{

	}
	
	public void Start()
	{
		CGameShips.Ship.GetComponent<CShipNaniteSystem>().RegisterNaniteSilo(gameObject);
		
		if(CNetwork.IsServer)
		{
			ActivateNaniteAvailability();
		}

        GetComponent<CActorInteractable>().EventHover += OnHover;
	}
	
	[AServerOnly]
	public void ActivateNaniteAvailability()
	{
		m_bNanitesAvailable.Set(true);
	}
	
	[AServerOnly]
	public void Deactivate()
	{
		m_bNanitesAvailable.Set(false);
	}

	[AServerOnly]
	public void DeductNanites(int _iNanites)
	{
		StoredNanites = StoredNanites - _iNanites;
	}

    // TEMPORARY //
    //
    // Hover text logic that needs revision. OnGUI + Copy/Paste code = Terribad
    //
    // TEMPORARY //
    bool bShowName = false;
    bool bOnGUIHit = false;
    void OnHover(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
    {
        bShowName = true;
    }


    public void OnGUI()
    {
        float fScreenCenterX = Screen.width / 2;
        float fScreenCenterY = Screen.height / 2;
        float fWidth = 100.0f;
        float fHeight = 20.0f;
        float fOriginX = fScreenCenterX + 25.0f;
        float fOriginY = fScreenCenterY - 10.0f;

        if (bShowName && !bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Nanite Storage");
            bOnGUIHit = true;
        }
        else if (bShowName && bOnGUIHit)
        {
            GUI.Label(new Rect(fOriginX, fOriginY, fWidth, fHeight), "Nanite Storage");
            bShowName = false;
            bOnGUIHit = false;
        }
    }
    // TEMPORARY //
    //
    // 
    //
    // TEMPORARY //
}
