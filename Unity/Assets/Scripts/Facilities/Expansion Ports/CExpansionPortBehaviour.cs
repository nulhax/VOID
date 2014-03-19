//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CExpansionPortInterface.cs
//  Description :   This script is used for alligning new hull segments to expansion ports.
//
//  Author  	:  Daniel Langsford
//  Mail    	:  folduppugg@hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */
 

public class CExpansionPortBehaviour : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void HandleFacilityCreate(GameObject _cFacilityObject);
    public event HandleFacilityCreate EventFacilityCreate;


    public delegate void HandleFacilityDestroy();


// Member Properties
	
	
	public uint ExpansionPortId 
	{
		get { return(m_uiPortId); }	
		
        set
		{
			if(m_uiPortId == 0)
			{
				m_uiPortId = value;
			}
			else
			{
				Debug.LogError("Cannot set ID value twice");
			}			
		}			
	}


    public GameObject AttachedFacility
    {
        get 
        {
            if (AttachedExpansionPort == null)
            {
                return (null);
            }

            return (AttachedExpansionPort.transform.parent.gameObject); 
        }
    }


    public GameObject AttachedExpansionPort
    {
        get
        {
            if (m_cAttachedExpansionPortViewId.Get() == null)
            {
                return (null);
            }

            return (m_cAttachedExpansionPortViewId.Get().GameObject);
        }
    }


    public CExpansionPortBehaviour AttachedExpansionPortBehaviour
    {
        get
        {
            if (m_cAttachedExpansionPortViewId.Get() == null)
            {
                return (null);
            }

            return (AttachedExpansionPort.GetComponent<CExpansionPortBehaviour>());
        }
    }


    public GameObject Door
    {
        get 
        {
            if (m_cDoor != null)
            {
                return (m_cDoor);
            }
            else if (AttachedExpansionPort != null &&
                     AttachedExpansionPortBehaviour.Door != null)
            {
                return (AttachedExpansionPortBehaviour.Door);
            }

            return (null); 
        }
    }


    public CDoorBehaviour DoorBehaviour
    {
        get
        {
            if (Door != null)
            {
                return (Door.GetComponent<CDoorBehaviour>());
            }
            else if (AttachedExpansionPort != null &&
                     AttachedExpansionPortBehaviour.Door != null)
            {
                return (AttachedExpansionPortBehaviour.DoorBehaviour);
            }

            return (null);
        }
    }


    public GameObject AttachedDuiDoorControl
    {
        get
        {
            if (m_cDuiDoorControl != null)
            {
                return (m_cDuiDoorControl);
            }
            else if (AttachedExpansionPort != null &&
                     AttachedExpansionPortBehaviour.AttachedDuiDoorControl != null)
            {
                return (AttachedExpansionPortBehaviour.AttachedDuiDoorControl);
            }

            return (null);
        }
    }
	

	public bool IsAttached
	{
        get { return (m_cAttachedExpansionPortViewId.Get() != null); }
	}


// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cAttachedExpansionPortViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);

        _cRegistrar.RegisterRpc(this, "PositionToNeighbour");
    }


    [AServerOnly]
    public GameObject CreateFacility(CFacilityInterface.EType _eFacilityType, int _iFacilityExpansionPortId)
    {
        // Retrieve the facility prefab
        CGameRegistrator.ENetworkPrefab eFacilityPrefab = CFacilityInterface.GetPrefabType(_eFacilityType);

        // Create facility object
        GameObject cCreatedFacilityObject = CNetwork.Factory.CreateObject(eFacilityPrefab);

        // Retrieve expansion port from created facility that will attach to me
        GameObject cExpansionPort = cCreatedFacilityObject.GetComponent<CFacilityExpansion>().GetExpansionPort(_iFacilityExpansionPortId);

        // Attach expansion ports together
        m_cAttachedExpansionPortViewId.Set(cExpansionPort.GetComponent<CNetworkView>().ViewId);
        AttachedExpansionPortBehaviour.m_cAttachedExpansionPortViewId.Set(SelfNetworkViewId);

        // Position expansion port and facility relative to me
        cExpansionPort.GetComponent<CExpansionPortBehaviour>().InvokeRpcAll("PositionToNeighbour");

        // Notify observers
        if (EventFacilityCreate != null) EventFacilityCreate(cCreatedFacilityObject);

        return (cCreatedFacilityObject);
    }


    void Start()
    {
        // Register myself as the first door parent
        if (Door != null)
        {
            DoorBehaviour.RegisterParentExpansionPort(this);

            if (AttachedDuiDoorControl != null)
            {
                AttachedDuiDoorControl.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiDoorControlBehaviour>().EventClickOpenDoor += OnDuiDoorButtonClick;
                AttachedDuiDoorControl.GetComponent<CDUIConsole>().DUI.GetComponent<CDuiDoorControlBehaviour>().EventClickCloseDoor += OnDuiDoorButtonClick;
            }
        }
    }


    void OnDestroy()
    {
        // Empty
    }


    void Update()
    {
        //RenderNormals();
    }


    void RenderNormals()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        Debug.DrawRay(transform.position, transform.up, Color.green);
        Debug.DrawRay(transform.position, transform.right, Color.red);
    }


    [ANetworkRpc]
    void PositionToNeighbour()
    {
        // Rotation
        transform.parent.rotation = AttachedExpansionPort.transform.rotation * Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0.0f, 180.0f, 0.0f);

        // Position
        float fDistance = (transform.position - gameObject.transform.parent.position).magnitude;

        Vector3 vPositionDisplacement = transform.parent.position - transform.position;
        transform.parent.position = AttachedExpansionPort.transform.position + vPositionDisplacement;
    }


    void OnDuiDoorButtonClick(CDuiDoorControlBehaviour.EButton _eButton)
    {
        switch (_eButton)
        {
            case CDuiDoorControlBehaviour.EButton.OpenDoor:
                DoorBehaviour.SetOpened(true);
                break;

            case CDuiDoorControlBehaviour.EButton.CloseDoor:
                DoorBehaviour.SetOpened(false);
                break;

            default:
                Debug.LogError("Unknown button" + _eButton);
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_cAttachedExpansionPortViewId)
        {
            // Empty
        }
    }

	
// Members


    public GameObject m_cDoor = null;
    public GameObject m_cDuiDoorControl = null;

    CNetworkVar<CNetworkViewId> m_cAttachedExpansionPortViewId = null;

	uint m_uiPortId = 0;


};
