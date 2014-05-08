//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHudTurretSelectBehaviour.cs
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


public class CHudTurretSelectBehaviour : MonoBehaviour
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
                m_cTurretList.enabled = true;
                m_cTurretDashboard.enabled = false;
                break;

            case EPanel.Dashboard:
                m_cTurretList.enabled = false;
                m_cTurretDashboard.enabled = true;
                break;
        }
    }


    public void OpenVisible(bool _bVisible)
    {
    }


	void Start()
	{
        m_cTemplateModuleGridItem.transform.parent = null;
        m_cTemplateModuleGridItem.SetActive(false);

        SetActivePanel(EPanel.TurretList);
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


    void ClearModuleGrid()
    {
    }


    void AddGridItem(CTurretBehaviour _eTurretBehaviour)
    {
        GameObject cNewItem = GameObject.Instantiate(m_cTemplateModuleGridItem) as GameObject;
        cNewItem.SetActive(true);

        cNewItem.transform.FindChild("Label_Name").GetComponent<UILabel>().text = _eTurretBehaviour.gameObject.name;

        // Append item to grid
        cNewItem.transform.parent = m_cTurretGrid.gameObject.transform;
        cNewItem.transform.localPosition = Vector3.zero;
        cNewItem.transform.localScale = Vector3.one;
        cNewItem.transform.localRotation = Quaternion.identity;

        // Set meta data
        cNewItem.GetComponent<CMetaData>().SetMeta("ViewId", _eTurretBehaviour.GetComponent<CNetworkView>().ViewId);

        // Refresh grid
        m_cTurretGrid.Reposition();
    }


// Member Fields


    public GameObject m_cTemplateModuleGridItem = null;
    public UIPanel m_cTurretDashboard = null;
    public UIPanel m_cTurretList = null;
    public UIGrid m_cTurretGrid = null;


    EPanel m_eActivePanel = EPanel.INVALID;


};
