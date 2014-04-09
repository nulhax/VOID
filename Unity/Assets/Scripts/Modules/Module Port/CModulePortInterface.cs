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
	public bool m_PreplacedModuleBuilt = false;
	public GameObject m_Positioner = null;
	public bool m_Internal = true;

	private Camera m_CubemapCam = null;
	private Cubemap m_CubemapSnapshot = null;
	
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

	public Cubemap CubeMapSnapshot
	{
		get { return(m_CubemapSnapshot); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_cAttachedModuleViewId = _cRegistrar.CreateReliableNetworkVar<CNetworkViewId>(OnNetworkVarSync, null);
    }


    public GameObject CreateModule(CModuleInterface.EType _eType)
    {
		GameObject cModuleObject = null;
		if(!IsModuleAttached)
		{
	        cModuleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(_eType));
	        cModuleObject.GetComponent<CNetworkView>().SetPosition(m_Positioner.transform.position);
	        cModuleObject.GetComponent<CNetworkView>().SetEulerAngles(m_Positioner.transform.rotation.eulerAngles);
	        cModuleObject.GetComponent<CNetworkView>().SetParent(GetComponent<CNetworkView>().ViewId);

	        m_cAttachedModuleViewId.Set(cModuleObject.GetComponent<CNetworkView>().ViewId);
		}
        return (cModuleObject);
    }

	void Start()
	{
		//UpdateCubemap();

        if (m_PreplacedModuleType != CModuleInterface.EType.INVALID &&
            CNetwork.IsServer)
        {
            CreateModule(m_PreplacedModuleType);

			// Make the module fully built already
            if (m_PreplacedModuleBuilt)
                m_cAttachedModuleViewId.Value.GameObject.GetComponent<CModuleInterface>().IncrementBuiltRatio(1.0f);
        }

		// Register self with parent facility
		CFacilityInterface fi = CUtility.FindInParents<CFacilityInterface>(gameObject);
		
		if(fi != null)
		{
			fi.RegisterModulePort(this);
		}
		else
		{
			Debug.LogError("Could not find facility to register to");
		}
	}


	public void UpdateCubemap()
	{
		// Disable all of the renderers for self
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.enabled = false;
		}

		if(m_CubemapSnapshot == null)
		{
			m_CubemapSnapshot = new Cubemap(16, TextureFormat.ARGB32, false);
		}

		if(m_CubemapCam == null)
		{
			GameObject tempCam = new GameObject("Cubemap Renderer");
			tempCam.transform.parent = transform;
			tempCam.transform.localPosition = Vector3.up * 1.5f;
			tempCam.transform.localRotation = Quaternion.identity;
			m_CubemapCam = tempCam.AddComponent<Camera>();
			m_CubemapCam.cullingMask = 1 << LayerMask.NameToLayer("Default");
			m_CubemapCam.farClipPlane = 100;
			m_CubemapCam.enabled = false;
		}

		//m_CubemapCam.RenderToCubemap(m_CubemapSnapshot);

		// Re-enable all of the renderers for self
		foreach(Renderer r in GetComponentsInChildren<Renderer>())
		{
			r.enabled = true;
		}
	}


	void OnDestroy()
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
