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
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
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
		Turrets     = 150,
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
		INVALID 			= -1,

		Prefabricator		= 0,
		AtmosphereGenerator = 50,
		PlayerSpawner       = 100,
        TurretCockpit       = 150,
        PilotCockpit        = 250,
        PowerGenerator      = 300,
        PowerBattery        = 350,
        Dispenser           = 600,
        NaniteSilo          = 650,
        Engine              = 700,
        Thruster            = 750,
        TurretPulseSmall    = 800,
        TurretPulseMedium   = 805,
        TurretMissleSmall   = 850,
        TurretMissileMedium = 855,
        ResearchMainframe   = 900,
        ShieldGenerator     = 950,

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
        get { return (m_bBuilt); }
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


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        m_fFunctionalRatio = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 1.0f);
        m_bBuiltPercent = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 0);
        m_bEnabled = _cRegistrar.CreateReliableNetworkVar<bool>(OnNetworkVarSync, false);
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


    public void SetModelVisible(bool _bVisible)
    {
        m_cModel.SetActive(_bVisible);
    }


    public static void RegisterPrefab(EType _eModuleType, CGameRegistrator.ENetworkPrefab _ePrefab)
    {
        s_mRegisteredPrefabs.Add(_eModuleType, _ePrefab);
    }


    public static CGameRegistrator.ENetworkPrefab GetPrefabType(EType _eModuleType)
    {
        if (!s_mRegisteredPrefabs.ContainsKey(_eModuleType))
        {
            //Debug.LogError(string.Format("Module type ({0}) has not been registered a prefab", _eModuleType));

            return (CGameRegistrator.ENetworkPrefab.INVALID);
        }

        return (s_mRegisteredPrefabs[_eModuleType]);
    }


	void Awake()
	{
        if (CNetwork.IsServer)
        {
            CGameShips.Ship.GetComponent<CShipModules>().RegisterModule(gameObject);
        }
	}


	void Start()
	{
        if (!CNetwork.IsServer)
        {
            CGameShips.Ship.GetComponent<CShipModules>().RegisterModule(gameObject);
        }

        if (!IsBuilt)
        {
            m_cPrecipitativeModel = GameObject.Instantiate(m_cPrecipitativeModel) as GameObject;
            m_cPrecipitativeModel.transform.parent = gameObject.transform;
            m_cPrecipitativeModel.transform.localPosition = Vector3.zero;
            m_cPrecipitativeModel.transform.localRotation = Quaternion.identity;
            m_bPrecipitativeModelInstanced = true;

            if (m_cPrecipitativeModel.GetComponent<CPrecipitativeMeshBehaviour>().m_cParticles != null)
            {
                m_cPrecipitativeModel.GetComponent<CPrecipitativeMeshBehaviour>().m_cParticles.Play();
            }
        }

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
            SetModelVisible(false);
        }

        /*
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
         * */
	}


	void OnDestroy()
	{
        // Empty
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
            if (m_cPrecipitativeModel != null)
            {
                m_cPrecipitativeModel.GetComponent<CPrecipitativeMeshBehaviour>().SetProgressRatio(m_bBuiltPercent.Value / 100.0f);
            }

            // Check is completely built
            if (!m_bBuilt &&
                 m_bBuiltPercent.Value == 100)
            {
                m_bBuilt = true;

                SetModelVisible(true);

                if (m_bPrecipitativeModelInstanced)
                {
                    Destroy(m_cPrecipitativeModel);
                    m_cPrecipitativeModel = null;
                    m_bPrecipitativeModelInstanced = false;
                }

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
        GameObject cPrecipitativePrefab = PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/Modules/_Precipitative/" + gameObject.name + " Precipitative Model" + ".prefab", combinationMesh);
        cPrecipitativePrefab.AddComponent<CPrecipitativeMeshBehaviour>();

        GameObject cParticles = PrefabUtility.CreatePrefab("Assets/Resources/Prefabs/Modules/_Precipitative/_Particle System2222.prefab", (GameObject)Resources.Load("Prefabs/Modules/_Precipitative/_Particle System", typeof(GameObject)));
		
		// Save assets and reposition original
		AssetDatabase.SaveAssets();
		transform.position = oldPos;
		DestroyImmediate(combinationMesh);
	}
#endif


// Member Fields


    public string m_sDisplayName = "";
    public string m_sDescription = "";
    public GameObject m_cModel = null;
    public GameObject m_cPrecipitativeModel = null;
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
	Camera m_CubemapCam = null;
	Cubemap m_CubemapSnapshot = null;


    bool m_bBuilt = false;
    bool m_bPrecipitativeModelInstanced = false;


    static Dictionary<CModuleInterface.EType, CGameRegistrator.ENetworkPrefab> s_mRegisteredPrefabs = new Dictionary<CModuleInterface.EType, CGameRegistrator.ENetworkPrefab>();


// Server Member Fields


    float m_fServerBuiltRatio = 0.0f; // 0.0f (0%) => 1.0f (100%)


};
