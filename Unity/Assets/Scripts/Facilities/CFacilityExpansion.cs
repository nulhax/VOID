//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CFacilityInfo.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityExpansion : MonoBehaviour
{
	
// Member Types
	
	
// Member Delegates & Events
	
	
// Member Properties



    public int ExpansionPortCount
    {
        get { return (m_aExpansionPorts.Length); }
    }


    public GameObject[] ExpansionPorts
	{
        get { return (m_aExpansionPorts); }
	}


// Member Methods


    public GameObject GetExpansionPort(uint _uiExpansionPortId)
    {
        return (m_aExpansionPorts[_uiExpansionPortId]);
    }


    void Awake()
    {
        DebugAddPortNames();
    }


	void DebugAddPortNames()
	{
        uint uiCount = 0;

        foreach (GameObject cExpansionPort in m_aExpansionPorts)
		{
			// Create the text field object
            GameObject TextField = new GameObject(cExpansionPort.name + (uiCount++).ToString());
            TextField.transform.parent = cExpansionPort.transform;
			TextField.transform.localPosition = Vector3.zero;
			TextField.transform.localRotation = Quaternion.identity;
			
			// Add the mesh renderer
			MeshRenderer mr = TextField.AddComponent<MeshRenderer>();
			mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));
			
			// Add the text mesh
			TextMesh textMesh = TextField.AddComponent<TextMesh>();
			textMesh.fontSize = 72;
			textMesh.characterSize = 0.10f;
			textMesh.color = Color.green;
			textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
			textMesh.anchor = TextAnchor.MiddleCenter;
			textMesh.offsetZ = -0.01f;
			textMesh.fontStyle = FontStyle.Italic;
			textMesh.text = TextField.name;
		}
	}


// Member Fields


    public GameObject[] m_aExpansionPorts = null;


};
