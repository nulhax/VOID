//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHudTurretCockpitControlInterface.cs
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


public class CHudTurretCockpitControlInterface : MonoBehaviour
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
                    AddGridItem(cTurret.GetComponent<CTurretBehaviour>());
                }
            }

            m_fUpdateTimer = 0.0f;
        }
	}


    void RemoveGridItem(CTurretBehaviour _eTurretBehaviour)
    {
        if (m_aTurretItems.ContainsKey(_eTurretBehaviour.NetworkViewId))
        {
            m_aTurretItems.Remove(_eTurretBehaviour.NetworkViewId);

            // Refresh grid
            m_cGridTurrets.Reposition();
        }
    }


    void AddGridItem(CTurretBehaviour _eTurretBehaviour)
    {
        if (m_aTurretItems.ContainsKey(_eTurretBehaviour.NetworkViewId))
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
        cNewItem.GetComponent<CMetaData>().SetMeta("ViewId", _eTurretBehaviour.GetComponent<CNetworkView>().ViewId);

        // Refresh grid
        m_cGridTurrets.Reposition();

        m_aTurretItems[_eTurretBehaviour.NetworkViewId] = cNewItem;
    }


// Member Fields


    public GameObject m_cTemplateGridItem = null;
    public UIPanel m_cPanelActiveTurret = null;
    public UIPanel m_cPanelTurretList = null;
    public UIGrid m_cGridTurrets = null;


    EPanel m_eActivePanel = EPanel.INVALID;

    float m_fUpdateInterval = 1.0f;
    float m_fUpdateTimer = 0.0f;

    Dictionary<TNetworkViewId, GameObject> m_aTurretItems = new Dictionary<TNetworkViewId, GameObject>();


};
