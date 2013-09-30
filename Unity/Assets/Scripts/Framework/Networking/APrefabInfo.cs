//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   PrefabInfo.h
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;


/* Implementation */


[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class APrefabInfo : Attribute
{

// Member Types


// Member Functions

    // public:


    public APrefabInfo(string _sResourceName)
    {
        m_sResourceName = _sResourceName;
    }


    public string GetResourceName()
    {
        return (m_sResourceName);
    }


    // protected:


    // private:



// Member Variables

    // protected:


    // private:


    string m_sResourceName;


};
