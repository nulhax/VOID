//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CModulePortInterface.cs
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


[RequireComponent(typeof(CNetworkView))]
public class CModulePortInterface : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Fields
	
	
	public CModuleInterface.ESize m_Size = CModuleInterface.ESize.INVALID;
	public CModuleInterface.EType m_PreplacedModuleType = CModuleInterface.EType.INVALID;
	public GameObject m_Positioner = null;
	public bool m_Internal = true;
	
	
	CNetworkVar<CNetworkViewId> m_cAttachedModuleViewId = null;


// Member Properties

	
    [AServerOnly]
    public CModuleInterface.ESize PortSize
    {
        get { return (m_Size); }
    }


	public bool IsInternal
	{
		get { return(m_Internal); }
	}


    public CNetworkViewId AttachedModuleViewId
    {
        get { return (m_cAttachedModuleViewId.Get()); }
    }


    public GameObject AttachedModuleObject
    {
		get { return (m_cAttachedModuleViewId.Get().GameObject); }
    }


    public bool IsModuleAttached
    {
        get { return (AttachedModuleViewId != null); }
    }


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cAttachedModuleViewId = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
    }


    public GameObject CreateModule(CModuleInterface.EType _eType)
    {
		GameObject cModuleObject = null;
		if(!IsModuleAttached)
		{
	        cModuleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(_eType));
	        cModuleObject.GetComponent<CNetworkView>().SetPosition(m_Positioner.transform.position);
	        cModuleObject.GetComponent<CNetworkView>().SetRotation(m_Positioner.transform.rotation.eulerAngles);
	        cModuleObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

	        m_cAttachedModuleViewId.Set(cModuleObject.GetComponent<CNetworkView>().ViewId);
		}
        return (cModuleObject);
    }


	void Start()
	{
        if (m_PreplacedModuleType != CModuleInterface.EType.INVALID &&
            CNetwork.IsServer)
        {
            CreateModule(m_PreplacedModuleType);
        }
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_cAttachedModuleViewId)
        {
            // Empty
        }
    }


};
