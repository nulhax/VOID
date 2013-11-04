//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   Utility.cs
//  Description :   Utility class containing helper functions
//                  and plain text resource locations
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.Boon@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */
public class Utility
{
    // Member Types
    static readonly string s_sXmlPath           = "../../Resources/XMLs/";
    static readonly string s_sXmlPathTools      = s_sXmlPath + "Tools.xml";
    static readonly string s_sXmlPathComponents = s_sXmlPath + "Components.xml";
    static readonly string s_sXmlPathFacilities = s_sXmlPath + "Facilities.xml";

    enum ETool
    {
        TOOL_INVALID = -1,
        TOOL_BLOWTORCH,
        TOOL_DETX,
        TOOL_EXTINGUISHER,
        TOOL_MEDPACK,
        TOOL_NANITEGUN,
        TOOL_PISTOL,
        TOOL_SEALER,
        TOOL_TECHKIT,
        TOOL_TORCH,
        TOOL_MAX
    };

    // Member Delegates & Events

    // Member Properties

    // Member Functions
    static public string GetXmlPath()           { return (s_sXmlPath);           }
    static public string GetXmlPathTools()      { return (s_sXmlPathTools);      }
    static public string GetXmlPathComponents() { return (s_sXmlPathComponents); }
    static public string GetXmlPathFacilities() { return (s_sXmlPathFacilities); }

    // Member Fields

};
