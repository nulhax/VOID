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


    public enum EType
    {
        INVALID,

        Internal,
        External,

        MAX
    }


    public enum ESize
    {
        INVALID,

        Small,
        Medium,
        Large,

        MAX
    }


// Member Delegates & Events


// Member Properties


    [AServerOnly]
    public EType PortType
    {
        get { return (m_eType); }
    }


    [AServerOnly]
    public ESize PortSize
    {
        get { return (m_eSize); }
    }


    public CNetworkViewId AttachedModuleViewId
    {
        get { return (m_cAttachedModuleViewId.Get()); }
    }


    public GameObject AttachedModuleObject
    {
        get { return (m_cAttachedModuleViewId.Get().GameObject); }
    }


    public bool IsEmpty
    {
        get { return (AttachedModuleViewId == null); }
    }


// Member Methods


    public override void InstanceNetworkVars()
    {
        m_cAttachedModuleViewId = new CNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
    }


    public GameObject CreateModule(CModuleInterface.EType _eType)
    {
        GameObject cModuleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(_eType));
        cModuleObject.GetComponent<CNetworkView>().SetPosition(m_cPositioner.transform.position);
        cModuleObject.GetComponent<CNetworkView>().SetRotation(m_cPositioner.transform.rotation.eulerAngles);
        cModuleObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

        m_cAttachedModuleViewId.Set(cModuleObject.GetComponent<CNetworkView>().ViewId);

        return (cModuleObject);
    }


	void Start()
	{
        if (m_ePreplacedModuleType != CModuleInterface.EType.INVALID &&
            CNetwork.IsServer)
        {
            CreateModule(m_ePreplacedModuleType);
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


// Member Fields


    public EType m_eType = EType.INVALID;
    public ESize m_eSize = ESize.INVALID;
    public CModuleInterface.EType m_ePreplacedModuleType = CModuleInterface.EType.INVALID;
    public GameObject m_cPositioner = null;


    CNetworkVar<CNetworkViewId> m_cAttachedModuleViewId = null;


};
