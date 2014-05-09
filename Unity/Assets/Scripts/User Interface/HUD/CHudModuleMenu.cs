//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CHudModuleMenu.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CHudModuleMenu : MonoBehaviour
{

// Member Types


// Member Delegates & Events


    public delegate void CreateModuleHandler(CHudModuleMenu _cSender, CModuleInterface.EType _eType);
    public event CreateModuleHandler EventCreateModule;


// Member Properties


// Member Methods



    public void OnEventModuleItemClicked()
    {
        // Get selected item
        m_cSelectedModuleItem = UICamera.hoveredObject;

        CMetaData cMetaData = m_cSelectedModuleItem.GetComponent<CMetaData>();

        float fModuleCost = cMetaData.GetMeta<float>("ModuleCost");

        CModuleInterface.EType eModuleType = cMetaData.GetMeta<CModuleInterface.EType>("ModuleType");
        m_cLabelModuleDescription.text = cMetaData.GetMeta<string>("ModuleDescription");
        m_cLabelModuleNaniteCost.text = cMetaData.GetMeta<float>("ModuleCost").ToString();

        if (CGameShips.Ship.GetComponent<CShipNaniteSystem>().NanaiteQuanity >= fModuleCost)
        {
            m_cLabelModuleNaniteCost.color = Color.white;
        }
        else
        {
            m_cLabelModuleNaniteCost.color = Color.red;
        }
    }


    public void OnEventCreateButtonClick()
    {
        if (m_cSelectedModuleItem != null)
        {
            if (EventCreateModule != null)
                EventCreateModule(this, m_cSelectedModuleItem.GetComponent<CMetaData>().GetMeta<CModuleInterface.EType>("ModuleType"));
        }
    }


	void Start()
	{
        m_cTemplateModuleGridItem.transform.parent = null;
        m_cTemplateModuleGridItem.SetActive(false);

        foreach (CModuleInterface.EType eModuleType in Enum.GetValues(typeof(CModuleInterface.EType)))
        {
            GameObject cModule = LoadModulePrefab(eModuleType);

            if (cModule == null)
                continue;

            CModuleInterface cModuleInterface = cModule.GetComponent<CModuleInterface>();

            if (cModuleInterface.m_bBuildable)
            {
                AddModuleGridItem(cModuleInterface.m_sDisplayName, cModuleInterface);
            }
        }

        m_bOpen = true;
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        if (m_bOpen &&
            CGameShips.Ship != null)
        {
            m_cLabelShipNanites.text = CGameShips.Ship.GetComponent<CShipNaniteSystem>().NanaiteQuanity + " / " + CGameShips.Ship.GetComponent<CShipNaniteSystem>().NanaiteCapacity;
        }
	}


    void AddModuleGridItem(string _sTitle, CModuleInterface _eModuleInterface)
    {
        GameObject cNewItem = GameObject.Instantiate(m_cTemplateModuleGridItem) as GameObject;
        cNewItem.SetActive(true);

        cNewItem.transform.FindChild("Label_ModuleName").GetComponent<UILabel>().text = _sTitle;

        // Append item to grid
        cNewItem.transform.parent = m_cGridModules.gameObject.transform;
        cNewItem.transform.localPosition = Vector3.zero;
        cNewItem.transform.localScale = Vector3.one;
        cNewItem.transform.localRotation = Quaternion.identity;

        // Set meta data
        cNewItem.GetComponent<CMetaData>().SetMeta("ModuleType", _eModuleInterface.ModuleType);
        cNewItem.GetComponent<CMetaData>().SetMeta("ModuleDescription", _eModuleInterface.m_sDescription);
        cNewItem.GetComponent<CMetaData>().SetMeta("ModuleCost", _eModuleInterface.m_fNanitesCost);

        // Refresh grid
        m_cGridModules.Reposition();
    }


    GameObject LoadModulePrefab(CModuleInterface.EType _eModuleType)
    {
        string sModulePrefabFile = CNetwork.Factory.GetRegisteredPrefabFile(CModuleInterface.GetPrefabType(_eModuleType));

        if (sModulePrefabFile == null)
            return (null);

        GameObject cModule = Resources.Load(sModulePrefabFile, typeof(GameObject)) as GameObject;

        return (cModule);
    }


// Member Fields


    public UIGrid m_cGridModules = null;
    public GameObject m_cTemplateModuleGridItem = null;
    public UILabel m_cLabelModuleDescription = null;
    public UILabel m_cLabelModuleNaniteCost = null;
    public UILabel m_cLabelShipNanites = null;
    public Transform m_cTransModulePreview = null;


    GameObject m_cSelectedModuleItem = null;


    int m_iDepthCounter = 0;


    bool m_bOpen = false;


};
