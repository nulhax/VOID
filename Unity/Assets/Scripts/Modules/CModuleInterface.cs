//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CComponentInterface.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;


/* Implementation */


[RequireComponent(typeof(CNetworkView))]
public class CModuleInterface : CNetworkMonoBehaviour
{

// Member Types


	public enum ECategory
	{
		INVALID,
		
		Atmosphere  = 50,
		Crew        = 100,
		Defence     = 150,
		Exploration = 200,
        Gravity     = 250,
        Power       = 300,
        Production  = 350,
        Propulsion  = 400,
        Research    = 450,
        Resources   = 500
	}
	

	public enum EType
	{
		INVALID,

		AtmosphereGenerator = 50,
		PlayerSpawner       = 100,
        TurretCockpit       = 150,
        LaserTurret         = 200,
        PilotCockpit        = 250,
        PowerGenerator      = 300,
        PowerCapacitor      = 350,
        MiningTurret        = 400,
        MiningCockpit       = 450,
        Dispenser           = 600,
        NaniteSilo          = 650,
        Engine              = 700,
        Starter             = 750,
        TurretPulseSmall    = 800,
        TurretPulseMedium   = 805,
        TurretMissleSmall   = 850,
        TurretMissileMedium = 855,
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


    public delegate void BuiltHandler(CModuleInterface _cSender);
    public event BuiltHandler EventBuilt;


    public delegate void FuntionalRatioChangeHandler(CModuleInterface _cSender, float _fOldFuntionalRatio, float _fNewFunctionalRatio);
    public event FuntionalRatioChangeHandler EventFunctionalRatioChange;


    public delegate void EnableChangeHandler(CModuleInterface _cSender, bool _bEnabled);
    public event EnableChangeHandler EventEnableChange;


// Member Properties


	public EType ModuleType
	{
		get { return (m_eModuleType); }
	}


	public ECategory ModuleCategory
	{
		get { return (m_eModuleCategory); }
	}


	public ESize ModuleSize
	{
		get { return (m_eModuleSize); }
	}


    public float FunctioanlRatio
    {
        get { return (m_fFunctionalRatio.Value); }
    }


    public bool IsEnabled
    {
        get { return (IsBuilt &&
                      m_bEnabled.Value); }
    }


    public bool IsBroken
    {
        get { return (m_fFunctionalRatio.Value == 0.0f); }
    }


	public bool IsInternal
	{
		get { return(m_bInternal); }
	}


    public bool IsBuilt
    {
        get { return (m_bBuiltPercent.Value == 100); }
    }


	public bool IsBuildable
	{
		get { return(m_bBuildable); }
	}


    public GameObject ParentFacility
    {
        get { return (m_cParentFacility); }
    }


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_fFunctionalRatio = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 1.0f);
        m_bBuiltPercent = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 0);
        m_bEnabled = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
    }


    public List<GameObject> GetAttachedComponents()
    {
        return (m_aAttachedComponents);
    }


    public void RegisterAttachedComponent(CComponentInterface _cComponentInterface)
    {
        m_aAttachedComponents.Add(_cComponentInterface.gameObject);
    }


    [AServerOnly]
    public void Build(float _fRatio)
    {
        // Incrmeent and cap
        m_fServerBuiltRatio += _fRatio;
        m_fServerBuiltRatio = Mathf.Clamp(m_fServerBuiltRatio, 0.0f, 1.0f);

        // Set built
        m_bBuiltPercent.Set((byte)(m_fServerBuiltRatio * 100.0f));
    }


    [AServerOnly]
    public void SetFuntionalRatio(float _fRatio)
    {
        m_fFunctionalRatio.Value = _fRatio;
    }


    [AServerOnly]
    public void SetEnabled(bool _bEnabled)
    {
        m_bEnabled.Value = _bEnabled;
    }


	public static List<GameObject> GetAllModules()
	{
		return (s_mModules);
	}


	public static List<GameObject> FindModulesByType(EType _eModuleType)
	{
		if (!s_mModulesByType.ContainsKey(_eModuleType))
		{
			return (new List<GameObject>());
		}

		return (s_mModulesByType[_eModuleType]);
	}


	public static List<GameObject> FindModulesByCategory(ECategory _eModuleCategory)
	{
		if (!s_mModulesByCategory.ContainsKey(_eModuleCategory))
		{
			return (new List<GameObject>());
		}
		
		return (s_mModulesByCategory[_eModuleCategory]);
	}


	public static List<GameObject> FindModulesBySize(ESize _eModuleSize)
	{
		if (!s_mModulesBySize.ContainsKey(_eModuleSize))
		{
			return (new List<GameObject>());
		}
		
		return (s_mModulesBySize[_eModuleSize]);
	}


    public static void RegisterPrefab(EType _eModuleType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eModuleType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eModuleType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eModuleType))
        {
            Debug.LogError(string.Format("Module type ({0}) has not been registered a prefab", _eModuleType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eModuleType]);
    }


	void Awake()
	{
		// Add self to the list of modules
		s_mModules.Add(gameObject);

		// Add self to the global list of module types
		if (!s_mModulesByType.ContainsKey(m_eModuleType))
		{
			s_mModulesByType.Add(m_eModuleType, new List<GameObject>());
		}
	
		s_mModulesByType[m_eModuleType].Add(gameObject);

		// Add self to the global list of module categories
		if (!s_mModulesByCategory.ContainsKey(m_eModuleCategory))
		{
			s_mModulesByCategory.Add(m_eModuleCategory, new List<GameObject>());
		}
		
		s_mModulesByCategory[m_eModuleCategory].Add(gameObject);

		// Add self to the global list of module sizes
		if (!s_mModulesBySize.ContainsKey(m_eModuleSize))
		{
			s_mModulesBySize.Add(m_eModuleSize, new List<GameObject>());
		}
		
		s_mModulesBySize[m_eModuleSize].Add(gameObject);
	}


	void Start()
	{
		// Ensure a type is defined 
		if (m_eModuleType == EType.INVALID)
		{
			Debug.LogError(string.Format("This module has not been given a module type. GameObjectName({0})", gameObject.name));
		}
		
		// Ensure a category is defined 
		if (m_eModuleCategory == ECategory.INVALID)
		{
			Debug.LogError(string.Format("This module has not been given a module category. GameObjectName({0})", gameObject.name));
		}
		
		// Ensure a size is defined 
		if (m_eModuleSize == ESize.INVALID)
		{
			Debug.LogError(string.Format("This module has not been given a module size. GameObjectName({0})", gameObject.name));
		}

        if (m_cModel == null)
        {
            Debug.LogError(string.Format("This module has not had its model defined. GameObjectName({0})", gameObject.name));
        }

        // Hide model if not built
        if (!IsBuilt)
        {
            m_cModel.SetActive(false);

            m_aAttachedComponents.ForEach((GameObject _cComponent) =>
            {
                _cComponent.SetActive(false);
            });
        }

		// Register self with parent facility
		CFacilityInterface cFacilityInterface = CUtility.FindInParents<CFacilityInterface>(gameObject);

		if(cFacilityInterface != null)
		{
			cFacilityInterface.RegisterModule(this);
			m_cParentFacility = cFacilityInterface.gameObject;
		}
		else
		{
			Debug.LogError("Could not find facility to register to");
		}
	}


	void OnDestroy()
	{
		// Remove self from global list of modules
		s_mModules.Remove(gameObject);
		s_mModulesByType[ModuleType].Remove(gameObject);
		s_mModulesByCategory[ModuleCategory].Remove(gameObject);
		s_mModulesBySize[ModuleSize].Remove(gameObject);
	}


	void Update()
	{
		// Empty
	}


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bBuiltPercent)
        {
            GetComponent<CModulePrecipitation>().SetProgressRatio(m_bBuiltPercent.Value / 100.0f);

            // Check is completely built
            if (m_bBuiltPercent.Value == 100)
            {
                m_cModel.SetActive(true);

                m_aAttachedComponents.ForEach((GameObject _cComponent) =>
                {
                    _cComponent.SetActive(true);
                });

                if (EventBuilt != null) EventBuilt(this);

                // Enable module
                if (CNetwork.IsServer)
                {
                    SetEnabled(true);
                }
            }
        }
        else if (_cSyncedVar == m_bEnabled)
        {
            if (EventEnableChange != null) EventEnableChange(this, m_bEnabled.Value);
        }
        else if (_cSyncedVar == m_fFunctionalRatio)
        {
            if (EventFunctionalRatioChange != null) EventFunctionalRatioChange(this, m_fFunctionalRatio.PreviousValue, m_fFunctionalRatio.Value);
        }
    }


#if UNITY_EDITOR
	[ContextMenu("Create Module Extras (Editor only)")]
	void CreatePrecipitationObject()
	{
		Vector3 oldPos = transform.position;
		transform.position = Vector3.zero;
		
		GameObject combinationMesh = new GameObject("_CombinationObject");
		combinationMesh.transform.localPosition = Vector3.zero;
		combinationMesh.transform.localRotation = Quaternion.identity;
		
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		
		for(int i = 0; i < meshFilters.Length; ++i) 
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
		AssetDatabase.CreateAsset(mesh, "Assets/Models/Modules/_Combined/" + gameObject.name + ".asset");
		mf.sharedMesh = mesh;
		
		// Save the precipitation mat
		Material precipitateMat = new Material(Shader.Find("VOID/Module Precipitate"));
		AssetDatabase.CreateAsset(precipitateMat, "Assets/Models/Modules/_Combined/Materials/" + gameObject.name + "_Precipitative" + ".mat");
		
		// Use this material and save an instance of the prefab
		mr.sharedMaterial = precipitateMat;
		PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/Modules/_Precipitative/" + gameObject.name + "_Precipitative" + ".prefab", combinationMesh);
		
		// Save assets and reposition original
		AssetDatabase.SaveAssets();
		transform.position = oldPos;
		DestroyImmediate(combinationMesh);
	}
#endif


// Member Fields


    public GameObject m_cModel = null;
	public EType m_eModuleType = EType.INVALID;
	public ECategory m_eModuleCategory = ECategory.INVALID;
	public ESize m_eModuleSize = ESize.INVALID;
    public float m_fNanitesCost = 100.124f;
	public bool m_bInternal = true;
	public bool m_bBuildable = true;


    CNetworkVar<float> m_fFunctionalRatio = null;
    CNetworkVar<byte> m_bBuiltPercent = null;
    CNetworkVar<bool> m_bEnabled = null;


    GameObject m_cParentFacility = null;


    List<GameObject> m_aAttachedComponents = new List<GameObject>();


	static List<GameObject> s_mModules                                                  = new List<GameObject>();
	static Dictionary<EType,     List<GameObject>> s_mModulesByType                     = new Dictionary<EType, List<GameObject>>();
	static Dictionary<ECategory, List<GameObject>> s_mModulesByCategory                 = new Dictionary<ECategory, List<GameObject>>();
	static Dictionary<ESize,     List<GameObject>> s_mModulesBySize                     = new Dictionary<ESize, List<GameObject>>();
    static Dictionary<EType,     CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs  = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


// Server Member Fields


    float m_fServerBuiltRatio = 0.0f; // 0.0f (0%) => 1.0f (100%)


};
