//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHudTurretCockpitInterface.cs
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


public class CHudTurretCockpitInterface : MonoBehaviour
{

// Member Types


    public enum EPanel
    {
        INVALID,

        TurretList,
        Dashboard,

        MAX
    }


// Member Delegates & Events


    public delegate void TakeTurretControlHandler(CHudTurretCockpitInterface _cSender, TNetworkViewId _cTurretViewId);
    public event TakeTurretControlHandler EventButtonTakeControlPressed;


    public delegate void EjectButtonClickHandler(CHudTurretCockpitInterface _cSender);
    public event EjectButtonClickHandler EventButtonEjectClicked;


// Member Properties


// Member Methods


    public void SetActivePanel(EPanel _ePanel)
    {
        switch (_ePanel)
        {
            case EPanel.TurretList:
                m_cPanelTurretList.enabled = true;
                m_cPanelActiveTurret.enabled = false;
                break;

            case EPanel.Dashboard:
                m_cPanelTurretList.enabled = false;
                m_cPanelActiveTurret.enabled = true;
                break;
        }
    }


    public void OpenVisible(bool _bVisible)
    {
    }


    public void OnEventTurretItemClick()
    {
        SetItemSelected(UICamera.hoveredObject);
    }


    public void OnEventTakeControlButtonClick()
    {
        if (m_cSelectedTurretItem == null)
            return;

        if (EventButtonTakeControlPressed != null)
            EventButtonTakeControlPressed(this, m_cSelectedTurretItem.GetComponent<CMetaData>().GetMeta<TNetworkViewId>("TurretViewId"));
    }


    public void OnEventEjectButtonClick()
    {
        if (EventButtonEjectClicked != null)
            EventButtonEjectClicked(this);
    }


	void Start()
	{
        m_cTemplateGridItem.transform.parent = null;
        m_cTemplateGridItem.SetActive(false);

        SetActivePanel(EPanel.TurretList);
	}


	void OnDestroy()
	{
        Destroy(m_cTemplateGridItem);
	}


	void Update()
	{
        UpdateShipTurrets();
        UpdateTurretList();
	}


    void UpdateShipTurrets()
    {
        m_fUpdateTimer += Time.deltaTime;

        if (m_fUpdateTimer > m_fUpdateInterval)
        {
            if (CNetwork.IsConnectedToServer &&
                CGameShips.Ship != null)
            {
                List<GameObject> aTurrets = CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.TurretPulseSmall);
                aTurrets.AddRange(CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.TurretPulseMedium));
                aTurrets.AddRange(CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.TurretMissleSmall));
                aTurrets.AddRange(CGameShips.Ship.GetComponent<CShipModules>().FindModulesByType(CModuleInterface.EType.TurretMissileMedium));

                foreach (GameObject cTurret in aTurrets)
                {
                    AddGridItem(cTurret.GetComponent<CTurretInterface>());
                }
            }

            m_fUpdateTimer = 0.0f;
        }
    }


    void UpdateTurretList()
    {
        foreach (KeyValuePair<TNetworkViewId, GameObject> tEntry in m_mTurretItems)
        {
            GameObject cTurret = tEntry.Key.GameObject;
            CTurretInterface cTurretInterface = cTurret.GetComponent<CTurretInterface>();

            CMetaData cButtonMetaData = tEntry.Value.GetComponent<CMetaData>();

            if (cTurretInterface.IsUnderControl)
            {
                SetItemUnavailable(tEntry.Value);
            }
            else if (!cTurretInterface.IsUnderControl)
            {
                SetItemAvailable(tEntry.Value);
            }
        }
    }


    void SetItemSelected(GameObject _cItem)
    {
        if (m_cSelectedTurretItem != null)
        {
            if (!m_cSelectedTurretItem.GetComponent<CMetaData>().GetMeta<bool>("IsAvailable"))
            {
                SetItemUnavailable(m_cSelectedTurretItem);
            }
            else
            {
                SetItemAvailable(m_cSelectedTurretItem);
            }

            // Save new state
            m_cSelectedTurretItem.GetComponent<CMetaData>().SetMeta("IsSelected", false);
        }

        m_cSelectedTurretItem = _cItem;
        m_cSelectedTurretItem.GetComponent<UIButtonColor>().defaultColor    = m_vSelectedColour;
        m_cSelectedTurretItem.GetComponent<UIButtonColor>().hover           = m_vSelectedColour;
        m_cSelectedTurretItem.GetComponent<UIButtonColor>().pressed         = m_vSelectedColour;

        // Refresh Ui
        m_cSelectedTurretItem.GetComponent<UIButton>().OnHover(true);

        // Save new state
        CMetaData cMetaData = m_cSelectedTurretItem.GetComponent<CMetaData>();

        cMetaData.SetMeta("IsSelected", true);

        m_cLabelStatus.text = "Available";
        m_cLabelPlayerName.text = "";

        if (!cMetaData.GetMeta<bool>("IsAvailable"))
        {
            ulong ulControllerPlayerId = cMetaData.GetMeta<TNetworkViewId>("TurretViewId").GameObject.GetComponent<CTurretInterface>().ControllerPlayerId;

            if (ulControllerPlayerId != 0)
            {
                m_cLabelStatus.text = "In-use";
                m_cLabelPlayerName.text = CGamePlayers.GetPlayerName(ulControllerPlayerId);
            }
        }
    }


    void SetItemAvailable(GameObject _cItem)
    {
        if (!_cItem.GetComponent<CMetaData>().GetMeta<bool>("IsSelected"))
        {
            ResetItemColours(_cItem);
        }
        _cItem.GetComponent<CMetaData>().SetMeta("IsAvailable", true);
    }


    void SetItemUnavailable(GameObject _cItem)
    {
        if (!_cItem.GetComponent<CMetaData>().GetMeta<bool>("IsSelected"))
        {
            _cItem.GetComponent<UIButtonColor>().defaultColor   = m_vUnavailableColour;
            _cItem.GetComponent<UIButtonColor>().hover          = m_vUnavailableColour;
            _cItem.GetComponent<UIButtonColor>().pressed        = m_vUnavailableColour;

            // Refresh Ui
            _cItem.GetComponent<UIButton>().OnHover(false);
        }

        _cItem.GetComponent<CMetaData>().SetMeta("IsAvailable", false);
    }


    void ResetItemColours(GameObject _cItem)
    {
        _cItem.GetComponent<UIButtonColor>().defaultColor = m_cTemplateGridItem.GetComponent<UIButtonColor>().defaultColor;
        _cItem.GetComponent<UIButtonColor>().hover        = m_cTemplateGridItem.GetComponent<UIButtonColor>().hover;
        _cItem.GetComponent<UIButtonColor>().pressed      = m_cTemplateGridItem.GetComponent<UIButtonColor>().pressed;

        // Refresh Ui
        _cItem.GetComponent<UIButton>().OnHover(false);
    }


    void RemoveGridItem(CTurretInterface _eTurretBehaviour)
    {
        if (m_mTurretItems.ContainsKey(_eTurretBehaviour.NetworkViewId))
        {
            m_mTurretItems.Remove(_eTurretBehaviour.NetworkViewId);

            // Refresh grid
            m_cGridTurrets.Reposition();
        }
    }


    void AddGridItem(CTurretInterface _eTurretBehaviour)
    {
        if (m_mTurretItems.ContainsKey(_eTurretBehaviour.NetworkViewId))
            return;

        GameObject cNewItem = GameObject.Instantiate(m_cTemplateGridItem) as GameObject;
        cNewItem.SetActive(true);

        cNewItem.transform.FindChild("Label_Name").GetComponent<UILabel>().text = _eTurretBehaviour.gameObject.name;

        // Append item to grid
        cNewItem.transform.parent = m_cGridTurrets.gameObject.transform;
        cNewItem.transform.localPosition = Vector3.zero;
        cNewItem.transform.localScale = Vector3.one;
        cNewItem.transform.localRotation = Quaternion.identity;

        // Set meta data
        cNewItem.GetComponent<CMetaData>().SetMeta("TurretViewId", _eTurretBehaviour.GetComponent<CNetworkView>().ViewId);
        cNewItem.GetComponent<CMetaData>().SetMeta("IsAvailable", true);
        cNewItem.GetComponent<CMetaData>().SetMeta("IsSelected", false);

        // Refresh grid
        m_cGridTurrets.Reposition();

        m_mTurretItems[_eTurretBehaviour.NetworkViewId] = cNewItem;
    }


// Member Fields


    public GameObject m_cTemplateGridItem = null;
    public UIPanel m_cPanelActiveTurret = null;
    public UIPanel m_cPanelTurretList = null;
    public UIGrid m_cGridTurrets = null;
    public UIButton m_cButtonTakeControl = null;

    public UILabel m_cLabelStatus = null;
    public UILabel m_cLabelPlayerName = null;


    Color m_vSelectedColour = Color.red;
    Color m_vUnavailableColour = Color.grey;
    GameObject m_cSelectedTurretItem = null;

    EPanel m_eActivePanel = EPanel.INVALID;

    float m_fUpdateInterval = 1.0f;
    float m_fUpdateTimer = 1.0f;

    Dictionary<TNetworkViewId, GameObject> m_mTurretItems = new Dictionary<TNetworkViewId, GameObject>();


};
