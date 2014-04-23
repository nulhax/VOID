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
		Prefabricator		= 800
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


    public delegate void BuiltHandler(GameObject _cModule);
    public event BuiltHandler EventBuilt;


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


	public Cubemap CubeMapSnapshot
	{
		get { return (m_CubemapSnapshot); }
	}


// Member Methods


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_bBuiltPercent = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 0);
    }


    public List<GameObject> GetAttachedComponents()
    {
        return (m_aAttachedComponents);
    }


    public void RegisterAttachedComponent(CComponentInterface _cComponentInterface)
    {
        m_aAttachedComponents.Add(_cComponentInterface.gameObject);
    }


    public void Build(float _fRatio)
    {
        // Incrmeent and cap
        m_fServerBuiltRatio += _fRatio;
        m_fServerBuiltRatio = Mathf.Clamp(m_fServerBuiltRatio, 0.0f, 1.0f);

        // Set built
        m_bBuiltPercent.Set((byte)(m_fServerBuiltRatio * 100.0f));
    }


	[AServerOnly]
	public static GameObject CreateNewModule(EType _ModuleType, CFacilityInterface _FacilityParent, Vector3 _LocalPostion)
	{
		GameObject moduleObject = CNetwork.Factory.CreateObject(CModuleInterface.GetPrefabType(_ModuleType));
		moduleObject.transform.parent = _FacilityParent.transform;
		moduleObject.transform.localPosition = _LocalPostion;

		return(moduleObject);
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


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        if (_cSyncedVar == m_bBuiltPercent)
        {
            GetComponent<CModulePrecipitation>().SetProgressRatio(m_bBuiltPercent.Value / 100.0f);

            if (m_bBuiltPercent.Value == 100)
            {
                m_cModel.SetActive(true);

                m_aAttachedComponents.ForEach((GameObject _cComponent) =>
                {
                    _cComponent.SetActive(true);
                });

                if (EventBuilt != null) EventBuilt(gameObject);
            }
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


    CNetworkVar<byte> m_bBuiltPercent = null;


    GameObject m_cParentFacility = null;
	Camera m_CubemapCam = null;
	Cubemap m_CubemapSnapshot = null;


    List<GameObject> m_aAttachedComponents = new List<GameObject>();


	static List<GameObject> s_mModules                                                  = new List<GameObject>();
	static Dictionary<EType,     List<GameObject>> s_mModulesByType                     = new Dictionary<EType, List<GameObject>>();
	static Dictionary<ECategory, List<GameObject>> s_mModulesByCategory                 = new Dictionary<ECategory, List<GameObject>>();
	static Dictionary<ESize,     List<GameObject>> s_mModulesBySize                     = new Dictionary<ESize, List<GameObject>>();
    static Dictionary<EType,     CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs  = new Dictionary<EType, CGameRegistrator.ENetworkPrefab>();


// Server Member Fields


    float m_fServerBuiltRatio = 0.0f; // 0.0f (0%) => 1.0f (100%)


};
