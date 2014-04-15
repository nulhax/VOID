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


	public GameObject ActiveTurretObject
	{
		get 
		{ 
			if (m_cActiveTurretViewId.Value == null)
			{
				return (null);
			}

			return (m_cActiveTurretViewId.Value.GameObject); 
		}
	}


// Member Methods


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		m_cActiveTurretViewId = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync, null);
    }

	void Start()
	{
        m_cCockpit = gameObject.GetComponent<CCockpit>();

        if (CNetwork.IsServer)
        {
            // Subscribe to cockpit events - Does not need to unsubscribe
            m_cCockpit.EventMounted += new CCockpit.HandlePlayerEnter(OnPlayerEnterCockpit);
            m_cCockpit.EventDismounted += new CCockpit.HandlePlayerLeave(OnPlayerLeaveCockpit);
        }
	}


	void OnDestroy()
	{
        // Empty
	}


	void HandleCockpitRotations(Vector2 _vTurretRotations, Vector2 _TurretMinMaxEulerX)
	{
		// Get the rotations of the turret
		Vector2 cockpitRotations = _vTurretRotations;

		// Transform X for use of the turret
		cockpitRotations.x = Mathf.Lerp(m_MinMaxEulerX.x, m_MinMaxEulerX.y, 
		                                (_vTurretRotations.x - _TurretMinMaxEulerX.x) / (_TurretMinMaxEulerX.y - _TurretMinMaxEulerX.x));

		// Update the rotations of the cockpit
		Vector3 cockpitLocalEuler = m_CockpitSeat.transform.localEulerAngles;
		cockpitLocalEuler = new Vector3(cockpitRotations.x, cockpitLocalEuler.y, cockpitLocalEuler.z);
		m_CockpitSeat.transform.localEulerAngles = cockpitLocalEuler;

		// Update the rotations of the center
		Vector3 centerLocalEuler = m_CockpitCenter.transform.localEulerAngles;
		cockpitLocalEuler = new Vector3(centerLocalEuler.x, cockpitRotations.y, centerLocalEuler.z);
		m_CockpitCenter.transform.localEulerAngles = cockpitLocalEuler;
	}


	[AServerOnly]
	void OnPlayerEnterCockpit(ulong _ulPlayerId)
	{
		CModuleInterface cSelfModuleInterface = GetComponent<CModuleInterface>();

		List<GameObject> acTurrets = null;

		switch (cSelfModuleInterface.ModuleType)
		{
		case CModuleInterface.EType.TurretCockpit:
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
                    cTurretObject.GetComponent<CTurretBehaviour>().SetUnderControl(true);

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
            ActiveTurretObject.GetComponent<CTurretBehaviour>().SetUnderControl(false);

            // Cleanup
            m_cActiveTurretViewId.Set(null);
        }

		//Debug.Log("Player left cockpit");
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
		if (_cSyncedVar == m_cActiveTurretViewId)
		{
			if(m_cActiveTurretViewId.Get() != null)
			{
				CTurretBehaviour tb = m_cActiveTurretViewId.Get().GameObject.GetComponent<CTurretBehaviour>();

				// Register the handling cockpit rotations
				//tb.EventTurretRotated += HandleCockpitRotations;

				// Set initial states
				//HandleCockpitRotations(tb.Rotation, tb.MinMaxRotationX);

				// Set the render texture from the turret
				m_CockpitScreen.renderer.material.SetTexture("_MainTex", tb.CameraRenderTexture);
				m_CockpitScreen2.renderer.material.SetTexture("_MainTex", tb.CameraRenderTexture);
			}

			// Unregister previous the handling cockpit rotations
			//if(m_cActiveTurretViewId.GetPrevious() != null)
			//	m_cActiveTurretViewId.GetPrevious().GameObject.GetComponent<CTurretBehaviour>().EventTurretRotated -= HandleCockpitRotations;
		}
	}


// Member Fields


    public GameObject m_CockpitSeat = null;
    public GameObject m_CockpitCenter = null;
    public GameObject m_CockpitScreen = null;
    public GameObject m_CockpitScreen2 = null;

    public Vector2 m_MinMaxEulerX = new Vector2(340.0f, 370.0f);


    CNetworkVar<TNetworkViewId> m_cActiveTurretViewId = null;


    CCockpit m_cCockpit = null;


};
