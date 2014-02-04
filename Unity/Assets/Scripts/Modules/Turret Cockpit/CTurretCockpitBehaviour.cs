//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CTurretCockpitBehaviour.cs
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


[RequireComponent(typeof(CCockpit))]
public class CTurretCockpitBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


    private CNetworkVar<bool> m_CalibratorComponentActive = null;
    private CNetworkVar<bool> m_LiquidComponentActive = null;
    private CNetworkVar<bool> m_RatchetComponentActive = null;


	public GameObject ActiveTurretObject
	{
		get 
		{ 
			if (m_cActiveTurretViewId.Get() == null)
			{
				return (null);
			}

			return (CNetwork.Factory.FindObject(m_cActiveTurretViewId.Get())); 
		}
	}


// Member Methods
	

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_cActiveTurretViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);

        //m_CalibratorComponentActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, null);
        //m_LiquidComponentActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, null);
        //m_RatchetComponentActive = _cRegistrar.CreateNetworkVar<bool>(OnNetworkVarSync, null);
    }

	void Start()
	{
		CCockpit cCockpit = gameObject.GetComponent<CCockpit>();

		// Subscribe to cockpit events
		cCockpit.EventPlayerEnter += new CCockpit.HandlePlayerEnter(OnPlayerEnterCockpit);
		cCockpit.EventPlayerLeave += new CCockpit.HandlePlayerLeave(OnPlayerLeaveCockpit);
	}


	void OnDestroy()
	{
		// Empty
	}


	void Update()
	{
		// Empty
	}


	[AServerOnly]
	void OnPlayerEnterCockpit(ulong _ulPlayerId)
	{
		CModuleInterface cSelfModuleInterface = GetComponent<CModuleInterface>();

		List<GameObject> acTurrets = null;

		switch (cSelfModuleInterface.ModuleType)
		{
		case CModuleInterface.EType.LaserCockpit:
			acTurrets = cSelfModuleInterface.ParentFacility.GetComponent<CFacilityInterface>().FindModulesByType(CModuleInterface.EType.LaserTurret);
			break;

		case CModuleInterface.EType.MiningCockpit:
			acTurrets = cSelfModuleInterface.ParentFacility.GetComponent<CFacilityInterface>().FindModulesByType(CModuleInterface.EType.MiningTurret);
			break;

		default:
			Debug.LogError("Unknown module cockpit type");
			break;
		}

    	if (acTurrets != null &&
        	acTurrets.Count > 0)
		{
	        foreach (GameObject cTurretObject in acTurrets)
	        {
	            if (!cTurretObject.GetComponent<CTurretBehaviour>().IsUnderControl)
	            {
					// Notify turret that it has been mounted by player
					cTurretObject.GetComponent<CTurretBehaviour>().TakeControl(_ulPlayerId);

	                m_cActiveTurretViewId.Set(cTurretObject.GetComponent<CNetworkView>().ViewId);

					break;
	            }
	        }
		}
	}


	[AServerOnly]
	void OnPlayerLeaveCockpit(ulong _ulPlayerId)
	{
        if (ActiveTurretObject != null)
        {
            // Release turret control
            ActiveTurretObject.GetComponent<CTurretBehaviour>().ReleaseControl();

            // Cleanup
            m_cActiveTurretViewId.Set(null);
        }

		//Debug.Log("Player left cockpit");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cActiveTurretViewId)
		{
			// Empty
		}
	}


// Member Fields


	CNetworkVar<CNetworkViewId> m_cActiveTurretViewId = null;

	
	float m_fFireTimer		= 0.0f;
	float m_fFireInterval	= 0.2f;


	bool m_bUpdateRotation = false;



};
