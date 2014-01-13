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
	
	
	// Member Fields
	private Dictionary<uint, GameObject> m_ExpansionPorts = new Dictionary<uint, GameObject>();
	
	
	// Member Properties
	public List<GameObject> ExpansionPorts
	{
		get { return(new List<GameObject>(m_ExpansionPorts.Values)); }
	}

	// Member Methods
	public void Awake()
	{	
		DebugAddPortNames();
	}
	
	public void Start()
	{
	}

	public void SearchExpansionPorts()
	{
		uint counter = 0;
		foreach(CExpansionPortInterface port in gameObject.GetComponentsInChildren<CExpansionPortInterface>())
		{
			m_ExpansionPorts.Add(counter++, port.gameObject);
			port.ExpansionPortId = counter;
		}
	}

	public GameObject GetExpansionPort(uint _ExpansionPortId)
	{
		return(m_ExpansionPorts[_ExpansionPortId]);
	}

	private void DebugAddPortNames()
	{
		foreach(uint portId in m_ExpansionPorts.Keys)
		{
			// Create the text field object
			GameObject TextField = new GameObject(m_ExpansionPorts[portId].name + portId.ToString());
			TextField.transform.parent = m_ExpansionPorts[portId].transform;
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
};
