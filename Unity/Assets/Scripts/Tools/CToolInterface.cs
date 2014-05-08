//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CToolInterface.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */

//This Script contains all the tool info, such as:
//		an enum for what type of tool the tool is
//		an ammo counter?
//		slots taken up
//		
//This Script can:
//		Get the type of tool
//		Shoot the Tool
//		Reload the Tool


[RequireComponent(typeof(CActorInteractable))]
[RequireComponent(typeof(CActorBoardable))]
[RequireComponent(typeof(CActorGravity))]
[RequireComponent(typeof(CToolOrientation))]
public class CToolInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum EType
	{
		INVALID,

		Ratchet          = 10,
		CircuitryKit     = 20,
		FireExtinguisher = 30,
		Torch            = 40,
		AK47             = 50,
		MiningDrill      = 60,

		MAX
	}

	public enum EToolCategory
	{
		INVALID,

		OneHanded,
		TwoHanded,

		MAX,
	}


    [ABitSize(4)]
	public enum ENetworkAction : byte
	{
        INVALID,

		PickUp,

        MAX
	}


// Member Delegates & Events


    [ALocalOnly]
    public delegate void NotifyPrimaryActiveChange(bool _bActive);
    public event NotifyPrimaryActiveChange EventPrimaryActiveChange;

    [ALocalOnly]
    public delegate void NotifySecondaryActiveChange(bool _bActive);
	public event NotifySecondaryActiveChange EventSecondaryActiveChange;

    [ALocalOnly]
    public delegate void NotifyEquippedChange(bool _bEquipped);
    public event NotifyEquippedChange EventEquippedChange;

    public delegate void NotifyToolEvent();
    public event NotifyToolEvent EventUse;
	public event NotifyToolEvent EventReload;
    public event NotifyToolEvent EventPickedUp;
    public event NotifyToolEvent EventDropped;


// Member Properties
	
	
	public EType ToolType
    {
        get { return (m_eToolType); }
    }
    

    public GameObject OwnerPlayerActor
    {
		get
		{
            if (!IsOwned)
            {
                return (null);
            }

			return (CGamePlayers.GetPlayerActor(OwnerPlayerId)); 
		}
    }


	public ulong OwnerPlayerId
	{
		get { return (m_ulOwnerPlayerId.Get()); }
	}


    public bool IsOwned
    {
		get { return (m_ulOwnerPlayerId.Get() != 0); }
    }


    [ALocalOnly]
    public bool IsEquiped
    {
        get { return (m_bEquipped); }
    }


    [ALocalOnly]
    public bool IsPrimaryActive
    {
        get { return (m_bPrimaryActive); }
    }


    [ALocalOnly]
    public bool IsSeconaryActive
    {
        get { return (m_bSecondaryActive); }
    }


// Member Functions


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_ulOwnerPlayerId = _cRegistrar.CreateReliableNetworkVar<ulong>(OnNetworkVarSync, 0);
    }


	public void Awake()
    {
		// Empty
	}


	public void Start()
	{
		if (m_eToolType == EType.INVALID)
        {
            Debug.LogError(string.Format("This tool has not been given a tool type. GameObjectName({0})", gameObject.name));
        }

        EventDropped += () =>
        {
            if (m_bPrimaryActive)
            {
                SetPrimaryActive(false);
            }

            if (m_bSecondaryActive)
            {
                SetSecondaryActive(false);
            }
        };

        CGamePlayers.SelfActor.GetComponent<CPlayerArmController>().EventDisableToolRotation += OnDisableToolRotation;
	}


	public void OnDestroy()
	{
		// Empty
	}


	public void Update()
	{
        if (IsOwned && m_bUseToolRotation)
        { 
            Transform ActorHead = OwnerPlayerActor.GetComponent<CPlayerHead>().Head.transform;
            gameObject.transform.rotation = ActorHead.rotation;
        }
	}


    [ALocalOnly]
    public void SetEquipped(bool _bEquipped)
    {
        m_bEquipped = _bEquipped;

        if (EventEquippedChange != null) EventEquippedChange(m_bEquipped);
    }


    [ALocalOnly]
    public void SetPrimaryActive(bool _bActive)
    {
        m_bPrimaryActive = _bActive;    

        if (EventPrimaryActiveChange != null) EventPrimaryActiveChange(_bActive);
    }


    [ALocalOnly]
    public void SetSecondaryActive(bool _bActive)
    {
        m_bSecondaryActive = _bActive;

        if (EventSecondaryActiveChange != null) EventSecondaryActiveChange(_bActive);
    }


    [AServerOnly]
    public void Reload()
    {
        Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

<<<<<<< HEAD
        // Check currently held
=======
        // Check currently held
>>>>>>> 1e1da5ec43f0ec2e1500fd866717234f237c7aa1
		if (IsOwned && m_bReloading != null)	// m_bReloading is null when the tool can not be reloaded (e.g. a torch).
        {
            m_bReloading.Set(true);
        }
    }


	[AServerOnly]
	public void SetOwner(ulong _ulPlayerId)
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		if (!IsOwned)
		{
            // Set owner player
			m_ulOwnerPlayerId.Set(_ulPlayerId);
		}
	}


	[AServerOnly]
	public void SetDropped()
	{
		Logger.WriteErrorOn(!CNetwork.IsServer, "Only servers are allow to invoke this method");

		// Check currently held
		if (IsOwned)
		{
            // Set owning object view id
            m_ulOwnerPlayerId.Set(0);
		}
	}


    [AServerOnly]
    public void SetVisible(bool _bVisible)
    {
        if (_bVisible)
        {
            foreach (Renderer cRenderer in transform.GetComponentsInChildren<Renderer>())
            {
                cRenderer.enabled = true;
            }
        }
        else
        {
            foreach (Renderer cRenderer in transform.GetComponentsInChildren<Renderer>())
            {
                cRenderer.enabled = false;
            }
        }
    }


    public static void RegisterPrefab(EType _ToolType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_ToolType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _ToolType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_ToolType))
        {
            //Debug.LogError(string.Format("Tool type ({0}) has not been registered a prefab", _ToolType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_ToolType]);
    }


    void OnNetworkVarSync(INetworkVar _cVarInstance)
    {
        if (_cVarInstance == m_ulOwnerPlayerId)
        {
            if (IsOwned)
            {                        
                // Turn off dynamic physics                
	            rigidbody.isKinematic = true;
	            rigidbody.detectCollisions = false;
				collider.isTrigger = true;
                

                // Stop receiving synchronizations
                GetComponent<CActorNetworkSyncronized>().m_SyncPosition = false;
                GetComponent<CActorNetworkSyncronized>().m_SyncRotation = false;

                // Notify observers
                if (EventPickedUp != null) EventPickedUp();
            }
            else
            {
                // Turn on dynamic physics
               
            	rigidbody.isKinematic = false;
            	rigidbody.detectCollisions = true;
				collider.isTrigger = false;
                

                rigidbody.AddForce(transform.forward * 5.0f, ForceMode.VelocityChange);
                rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.VelocityChange);

                // Receive synchronizations
                GetComponent<CActorNetworkSyncronized>().m_SyncPosition = true;
                GetComponent<CActorNetworkSyncronized>().m_SyncRotation = true;

                if (m_bPrimaryActive)
                {
                    SetPrimaryActive(false);
                }

                if (m_bSecondaryActive)
                {
                    SetSecondaryActive(false);
                }

                // Notify observers
                if (EventDropped != null) EventDropped();
            }
        }
        else if (_cVarInstance == m_bReloading)
        {
            if (m_bReloading.Get())
            {
                // Notify observers
                if (EventReload != null) EventReload();
            }
        }
    }

    void OnDisableToolRotation(bool _bUseHeadRotation)
    {
        m_bUseToolRotation = _bUseHeadRotation;
    }


#if UNITY_EDITOR
    [ContextMenu("Create Tool Extras (Editor only)")]
    void CreatePrecipitationObject()
    {
        Vector3 oldPos = transform.position;
        transform.position = Vector3.zero;

        GameObject combinationMesh = new GameObject("_CombinationObject");
        combinationMesh.transform.localPosition = Vector3.zero;
        combinationMesh.transform.localRotation = Quaternion.identity;

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; ++i)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);

        // Add the mesh renderer and filter to the prefab
        MeshRenderer mr = combinationMesh.AddComponent<MeshRenderer>();
        MeshFilter mf = combinationMesh.AddComponent<MeshFilter>();

        // Save the mesh
        AssetDatabase.CreateAsset(mesh, "Assets/Models/Tools/_Combined/" + gameObject.name + ".asset");
        mf.sharedMesh = mesh;

        // Save the precipitation mat
        Material precipitateMat = new Material(Shader.Find("VOID/Module Precipitate"));
        AssetDatabase.CreateAsset(precipitateMat, "Assets/Models/Tools/_Combined/Materials/" + gameObject.name + "_Precipitative" + ".mat");

        // Use this material and save an instance of the prefab
        mr.sharedMaterial = precipitateMat;
        GameObject cPrecipitativePrefab = PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/Tools/_Precipitative/" + gameObject.name + " Precipitative Model" + ".prefab", combinationMesh);
        cPrecipitativePrefab.AddComponent<CPrecipitativeMeshBehaviour>();

        GameObject cParticles = PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/Tools/_Precipitative/_Particle System Copy.prefab", (GameObject)Resources.Load("Prefabs/Tools/_Precipitative/_Particle System", typeof(GameObject)));

        // Save assets and reposition original
        AssetDatabase.SaveAssets();
        transform.position = oldPos;
        DestroyImmediate(combinationMesh);
    }
#endif


// Member Fields

	public EType m_eToolType = EType.INVALID;
	public EToolCategory m_eToolCategory = EToolCategory.INVALID;
    public GameObject m_cModel = null;
    public GameObject m_cPrecipitativeModel = null;
    public string m_sName = "Unnamed tool";
    public string m_sDescription = "This is the default tool description";
    public float m_fNaniteCost = -1.0f;
    public float m_fBuildDuration = 10.4f;
    public bool m_bDispensable = false;


    public GameObject m_RightHandPos;
    public GameObject m_LeftHandPos;

    CNetworkVar<ulong> m_ulOwnerPlayerId = null;
    CNetworkVar<bool> m_bReloading = null;


    bool m_bEquipped = false;
    bool m_bPrimaryActive = false;
    bool m_bSecondaryActive = false;
    bool m_bUseToolRotation = true;

	static Dictionary<EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();

};
