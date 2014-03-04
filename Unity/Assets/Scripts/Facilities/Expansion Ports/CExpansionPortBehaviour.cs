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


    public GameObject AttachedFacilityObject
    {
        get { if (m_cAttachedExpansionPortObject == null) return (null); return (m_cAttachedExpansionPortObject.transform.parent.gameObject); }
    }


    public CDoorBehaviour DoorBehaviour
    {
        get 
        { 
            if (m_cDoorObject == null) 
                return (m_cAttachedExpansionPortObject.GetComponent<CExpansionPortBehaviour>().DoorBehaviour);  
            
            return (m_cDoorObject.GetComponent<CDoorBehaviour>()); 
        }
    }
	

	public bool IsAttached
	{
        get { return (m_cAttachedExpansionPortObject != null); }
	}


// Member Functions


	public void AttachTo(GameObject _cExpansionPortObject)
	{
        // Remember who I am attached to
        m_cAttachedExpansionPortObject = _cExpansionPortObject;

        CExpansionPortBehaviour cExpansionPortBehaviour = _cExpansionPortObject.GetComponent<CExpansionPortBehaviour>();

        transform.parent.rotation = _cExpansionPortObject.transform.rotation * Quaternion.Inverse(transform.rotation) * Quaternion.Euler(0.0f, 180.0f, 0.0f);

        float fDistance = (transform.position - gameObject.transform.parent.position).magnitude;

        Vector3 vPositionDisplacement = transform.parent.position - transform.position;
        transform.parent.position = _cExpansionPortObject.transform.position + vPositionDisplacement;// + (_cExpansionPortObject.transform.forward * fDistance);

        // Sync position & rotation
        transform.parent.GetComponent<CNetworkView>().SyncTransformPosition();
        transform.parent.GetComponent<CNetworkView>().SyncTransformRotation();
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
        m_cAttachedExpansionPortObject = cFacilityExpansion.GetExpansionPort(_uiFacilityExpansionPortId);

        if (EventFacilityCreate != null) EventFacilityCreate(cCreatedFacilityObject);

        return (cCreatedFacilityObject);
    }


    void Start()
    {
        // Empty
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


    public GameObject m_cDoorObject = null;

    GameObject m_cAttachedExpansionPortObject = null;
	
	uint m_uiPortId = 0;


};
