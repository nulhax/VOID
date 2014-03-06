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
 

public class CExpansionPortBehaviour : MonoBehaviour
{

// Member Types


    public enum EBuildState
    {
        state_default,
        state_Construction,
        state_Orientation,
        state_unaviable,
        state_max
    };


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
            if (m_cAttachedExpansionPort == null)
            {
                return (null);
            }

            return (m_cAttachedExpansionPort.transform.parent.gameObject); 
        }
    }


    public GameObject AttachedExpansionPort
    {
        get
        {
            return (m_cAttachedExpansionPort);
        }
    }


    public CExpansionPortBehaviour AttachedExpansionPortBehaviour
    {
        get
        {
            if (m_cAttachedExpansionPort == null)
            {
                return (null);
            }

            return (m_cAttachedExpansionPort.GetComponent<CExpansionPortBehaviour>());
        }
    }


    public GameObject AttachedDoor
    {
        get 
        {
            if (m_cDoor != null)
            {
                return (m_cDoor);
            }
            else if (AttachedExpansionPort != null &&
                     AttachedExpansionPortBehaviour.AttachedDoor != null)
            {
                return (AttachedExpansionPortBehaviour.AttachedDoor);
            }

            return (null); 
        }
    }


    public CDoorBehaviour AttachedDoorBehaviour
    {
        get
        {
            if (AttachedDoor != null)
            {
                return (AttachedDoor.GetComponent<CDoorBehaviour>());
            }
            else if (AttachedExpansionPort != null &&
                     AttachedExpansionPortBehaviour.AttachedDoor != null)
            {
                return (AttachedExpansionPortBehaviour.AttachedDoorBehaviour);
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
        get { return (m_cAttachedExpansionPort != null); }
	}


// Member Functions


    [AServerOnly]
	public void AttachTo(GameObject _cExpansionPortObject)
	{
        // Remember who I am attached to
        m_cAttachedExpansionPort = _cExpansionPortObject;

        CExpansionPortBehaviour cExpansionPortBehaviour = _cExpansionPortObject.GetComponent<CExpansionPortBehaviour>();

        transform.parent.rotation = _cExpansionPortObject.transform.rotation * Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0.0f, 180.0f, 0.0f);

        float fDistance = (transform.position - gameObject.transform.parent.position).magnitude;

        Vector3 vPositionDisplacement = transform.parent.position - transform.position;
        transform.parent.position = _cExpansionPortObject.transform.position + vPositionDisplacement;// + (_cExpansionPortObject.transform.forward * fDistance);

        // Sync position & rotation
        transform.parent.GetComponent<CNetworkView>().SyncTransformPosition();
        transform.parent.GetComponent<CNetworkView>().SyncTransformRotation();

        // Register myself as the second door parent
        //AttachedDoorBehaviour.RegisterParentExpansionPort(this, 1);
	}


    [AServerOnly]
    public GameObject CreateFacility(CFacilityInterface.EType _eFacilityType, uint _uiFacilityExpansionPortId)
    {
        // Retrieve the facility prefab
        CGameRegistrator.ENetworkPrefab eFacilityPrefab = CFacilityInterface.GetPrefabType(_eFacilityType);

        // Create facility
        GameObject cCreatedFacilityObject = CNetwork.Factory.CreateObject(eFacilityPrefab);

        // Tell other expansion port to connect to me
        CFacilityExpansion cFacilityExpansion = cCreatedFacilityObject.GetComponent<CFacilityExpansion>();
        cFacilityExpansion.GetExpansionPort(_uiFacilityExpansionPortId).GetComponent<CExpansionPortBehaviour>().AttachTo(gameObject);

        // Remmeber who I am currently connected to
        m_cAttachedExpansionPort = cFacilityExpansion.GetExpansionPort(_uiFacilityExpansionPortId);

        if (EventFacilityCreate != null) EventFacilityCreate(cCreatedFacilityObject);

        return (cCreatedFacilityObject);
    }


    void Start()
    {
        // Register myself as the first door parent
        if (AttachedDoor != null)
        {
            AttachedDoorBehaviour.RegisterParentExpansionPort(this, 0);
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


    void CreateDoors()
    {

    }

	
// Members


    public GameObject m_cDoor = null;
    public GameObject m_cDuiDoorControl = null;

    GameObject m_cAttachedExpansionPort = null;

	uint m_uiPortId = 0;


};
