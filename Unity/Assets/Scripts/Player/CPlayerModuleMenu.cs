//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerModuleMenu.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;


/* Implementation */


public class CPlayerModuleMenu : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
    public enum ENetworkAction
    {
        CreateModule
    }


    public enum EState
    {
        INVALID,

        Idle,
        BrowsingMenu,
        PreviewingModule,

        MAX
    }


// Member Delegates & Events


// Member Properties


// Member Methods


    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteNotifyBuildResponse");
    }


    public void SetState(EState _eState, params object[] _caParameters)
    {
        DestroyModulePreview();

        switch (_eState)
        {
            case EState.Idle:
                SetMenuOpened(false);
                break;

            case EState.BrowsingMenu:
                SetMenuOpened(true);
                break;

            case EState.PreviewingModule:
                SetMenuOpened(false);
                LoadModulePreview((CModuleInterface.EType)_caParameters[0]);
                break;

            default:
                Debug.LogError("Unknown state: " + _eState);
                break;
        }

        m_eState = _eState;
    }


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        _cStream.Write(s_cSerializedStream);
        s_cSerializedStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId);

        while (_cStream.HasUnreadData)
        {
            ENetworkAction eNetworkAction = _cStream.Read<ENetworkAction>();

            switch (eNetworkAction)
            {
                case ENetworkAction.CreateModule:
                    {
                        bool bCreated = CGameShips.Ship.GetComponent<CShipModules>().CreateModule(_cStream.Read<CModuleInterface.EType>(),
                                                                                                  _cStream.Read<Vector3>(),
				                                                                          		  Quaternion.Euler(_cStream.Read<Vector3>()));

                        cPlayerActor.GetComponent<CPlayerModuleMenu>().InvokeRpc(_cNetworkPlayer.PlayerId, "RemoteNotifyBuildResponse", bCreated);
                    }
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eNetworkAction);
                    break;
            }
        }
    }


	void Start()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.SubscribeInputChange(CUserInput.EInput.ModuleMenu_ToggleDisplay, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Escape, OnEventInput);

            m_cHudModuleMenuInterface = CGameHUD.Hud2dInterface.ModuleBuildMenuInterface;
            m_cHudModuleMenuInterface.EventCreateModule += OnEventCreateModule;
        }
	}


	void OnDestroy()
	{
        if (GetComponent<CPlayerInterface>().IsOwnedByMe)
        {
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.ModuleMenu_ToggleDisplay, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Escape, OnEventInput);
        }
	}


	void Update()
	{
        UpdateModulePreview();
	}


    [ALocalOnly]
    void UpdateModulePreview()
    {
        if (m_cPreviewModulePrecipitative == null)
            return;

        m_bPreviewPlacementValid = false;

        // Raycast to find module placement
        RaycastHit tTileRaycastHit = new RaycastHit();
        CTileInterface cTileInterface = null;
        bool bModuleVisible = false;

        if (RaycastFindTile(ref tTileRaycastHit, ref cTileInterface))
        {
            GameObject cHitTile = tTileRaycastHit.transform.gameObject;

            CTile.EType eTileType = cTileInterface.GetTileType(tTileRaycastHit.transform.gameObject);

            if (eTileType == CTile.EType.Interior_Floor ||
                eTileType == CTile.EType.Exterior_Upper ||
                eTileType == CTile.EType.Exterior_Lower ||
                eTileType == CTile.EType.Exterior_Wall)
            {
                //m_cPreviewModulePrecipitative.layer = CGameCameras.MainCamera.layer;
                m_cPreviewModulePrecipitative.transform.position = tTileRaycastHit.point;
                m_cPreviewModulePrecipitative.SetActive(true);
                m_cPreviewModulePrecipitative.transform.rotation = Quaternion.FromToRotation(Vector3.up, tTileRaycastHit.normal);

                float fSphereRadius = 0.0f;
    
                switch (m_ePreviewModuleSize)
                {
                    case CModuleInterface.ESize.Small:
                        fSphereRadius = 3.0f;
                        break;

                    case CModuleInterface.ESize.Medium:
                        fSphereRadius = 3.0f;
                        break;

                    default:
                        Debug.LogError("lazy");
                        break;
                }

                RaycastHit[] atRaycastHits = Physics.SphereCastAll(m_cPreviewModulePrecipitative.transform.position, fSphereRadius / 2, Vector3.up, 0.1f, 1 << LayerMask.NameToLayer("Default"));

                if ((eTileType != CTile.EType.Interior_Floor &&
                     m_bPreviewModuleInternal) ||
                    (eTileType == CTile.EType.Interior_Floor &&
                     !m_bPreviewModuleInternal) ||
                     atRaycastHits.Length > 0)
                {
                    m_cPreviewModulePrecipitative.renderer.material.SetVector("_Tint", new Vector4(1.0f, 0.0f, 0.0f, 0.2f));
                }
                else
                {
                    m_cPreviewModulePrecipitative.renderer.material.SetVector("_Tint", new Vector4(0.0f, 1.0f, 0.0f, 0.2f));
                    m_bPreviewPlacementValid = true;
                }

                bModuleVisible = true;
            }
        }

        SetModulePreviewVisible(bModuleVisible);
    }


    [ALocalOnly]
    bool RaycastFindTile(ref RaycastHit _rtTileRaycastHit, ref CTileInterface _rcTileInterface)
    {
        Ray cMainCameraRay = new Ray(CGameCameras.ShipCamera.transform.position, CGameCameras.ShipCamera.transform.forward);
        bool bHitTile = false;
        
        RaycastHit[] cMainCameraRaycastHits = Physics.RaycastAll(cMainCameraRay, 10.0f, 1 << LayerMask.NameToLayer("Default"));
        cMainCameraRaycastHits = cMainCameraRaycastHits.OrderBy((_tItem) => _tItem.distance).ToArray();


        // Iterate through all hit 
        foreach (RaycastHit cRaycastHit in cMainCameraRaycastHits)
        {
            // Check hit object has parent with tile interface
            _rcTileInterface = CUtility.FindInParents<CTileInterface>(cRaycastHit.transform.gameObject);

            if (_rcTileInterface != null)
            {
                _rtTileRaycastHit = cRaycastHit;
                bHitTile = true;
                break;
            }
        }

        return (bHitTile);
    }


    [ALocalOnly]
    void SetMenuOpened(bool _bOpen)
    {
        if (_bOpen &&
            m_eState == EState.BrowsingMenu)
            return;

        if (_bOpen)
        {
            CGameHUD.Hud2dInterface.ShowHud(CHud2dInterface.EHud.ModuleMenu);
        }
        else
        {
            CGameHUD.Hud2dInterface.HideHud(CHud2dInterface.EHud.ModuleMenu);
        }

        if (_bOpen)
        {
            m_eState = EState.BrowsingMenu;
        }

        if (_bOpen)
        {
            GetComponent<CPlayerMotor>().DisableInput(this);
        }
        else
        {
            GetComponent<CPlayerMotor>().EnableInput(this);
        }
    }


    [ALocalOnly]
    void AttemptPlaceModule()
    {
        if (m_eState != EState.PreviewingModule)
            return;

        if (m_ePreviewModuleType == CModuleInterface.EType.INVALID)
            return;

        if (!m_bPreviewPlacementValid)
            return;

        s_cSerializedStream.Write(ENetworkAction.CreateModule);
        s_cSerializedStream.Write(m_ePreviewModuleType);
        s_cSerializedStream.Write(m_cPreviewModulePrecipitative.transform.position);
        s_cSerializedStream.Write(m_cPreviewModulePrecipitative.transform.eulerAngles);

        DestroyModulePreview();
    }


    [ALocalOnly]
    void LoadModulePreview(CModuleInterface.EType _eType)
    {
        m_ePreviewModuleType = _eType;

        m_cPreviewModulePrecipitative = CNetwork.Factory.LoadPrefab(CModuleInterface.GetPrefabType(m_ePreviewModuleType));
        m_ePreviewModuleSize = m_cPreviewModulePrecipitative.GetComponent<CModuleInterface>().ModuleSize;
        m_bPreviewModuleInternal = m_cPreviewModulePrecipitative.GetComponent<CModuleInterface>().m_bInternal;

        m_cPreviewModulePrecipitative = GameObject.Instantiate(m_cPreviewModulePrecipitative.GetComponent<CModuleInterface>().m_cPrecipitativeModel) as GameObject;
        m_cPreviewModulePrecipitative.SetActive(false);

        m_bPreviewPlacementValid = false;
    }


    [ALocalOnly]
    void DestroyModulePreview()
    {
        if (m_cPreviewModulePrecipitative != null)
        {
            Destroy(m_cPreviewModulePrecipitative);
        }

        // Reset preview variables
        m_ePreviewModuleType = CModuleInterface.EType.INVALID;
        m_ePreviewModuleSize = CModuleInterface.ESize.INVALID;
        m_cPreviewModulePrecipitative = null;
        m_bPreviewPlacementValid = false;
        m_bPreviewModuleInternal = false;
    }


    [ALocalOnly]
    void SetModulePreviewVisible(bool _bVisible)
    {
        m_cPreviewModulePrecipitative.SetActive(_bVisible);
    }


    [ALocalOnly]
    void OnEventCreateModule(CHudModuleBuildMenuInterface _cSender, CModuleInterface.EType _eModuleType)
    {
        if (_eModuleType == CModuleInterface.EType.INVALID)
            return;

        SetState(EState.PreviewingModule, _eModuleType);
    }


    [ALocalOnly]
    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        if (_bDown)
        {
            switch (_eInput)
            {
                case CUserInput.EInput.ModuleMenu_ToggleDisplay:
                    {
                        if (!GetComponent<CPlayerCockpitBehaviour>().IsMounted)
                        {
                            SetState((m_eState == EState.BrowsingMenu) ? EState.Idle : EState.BrowsingMenu);
                        }
                    }
                    break;

                case CUserInput.EInput.Primary:
                    AttemptPlaceModule();
                    break;

                case CUserInput.EInput.Secondary:
                    SetState(EState.Idle);
                    break;

                case CUserInput.EInput.Escape:
                    SetState(EState.Idle);
                    break;
            }
        }
    }


    [ANetworkRpc]
    void RemoteNotifyBuildResponse(bool _bBuilt)
    {
        if (!_bBuilt)
        {
            Debug.LogError("Not enough nanites to build modules");
        }
        else
        {
        }
    }


// Member Fields


    CHudModuleBuildMenuInterface m_cHudModuleMenuInterface = null;


    EState m_eState = EState.INVALID;


    CModuleInterface.EType m_ePreviewModuleType = CModuleInterface.EType.INVALID;
    CModuleInterface.ESize m_ePreviewModuleSize = CModuleInterface.ESize.INVALID;
    GameObject m_cPreviewModulePrecipitative = null;
    bool m_bPreviewModuleInternal = false;
    bool m_bPreviewPlacementValid = false;


    static CNetworkStream s_cSerializedStream = new CNetworkStream();


};
