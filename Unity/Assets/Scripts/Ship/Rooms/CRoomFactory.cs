//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomFactory.cs
//  Description :   Class script for the factory facility
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

/* Implementation */
public class CRoomFactory : MonoBehaviour
{

// Member Types
	struct TData
	{
		// XML Imports
		float m_fHealth;
		float m_fPowerCost;
		float m_fRecharge;
		float m_fRadiationLevel;
		float m_fRadiationRadius;
		
		// Class
		ushort m_sCurrentToolID;
	};

// Member Delegates & Events

// Member Properties

// Member Functions
	public void Start()
	{
        // Load the XML reader and document for parsing information
        TextAsset ImportedXml = new TextAsset();
        ImportedXml.name = Utility.GetXmlPathFacilities();

        XmlTextReader XReader = new XmlTextReader(new StringReader(ImportedXml.text));
        XmlDocument XDoc = new XmlDocument();
        XDoc.Load(XReader);

        string T = XDoc.Attributes.ToString();
        Debug.Break();
	}

	public void OnDestroy() {}
	public void Update() {}

// Member Fields

};
