//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


public class CMetaData : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


    public void SetMeta(string _sName, object _cValue)
    {
        if (m_mData.ContainsKey(_sName))
        {
            Debug.LogError(string.Format("Meta data key ({0}) already exists in directory.", _sName));

            return ;
        }

        m_mData.Add(_sName, _cValue);
    }


    public TYPE GetMeta<TYPE>(string _sName)
    {
        return ((TYPE)m_mData[_sName]);
    }



// Member Fields


    Dictionary<string, object> m_mData = new Dictionary<string, object>();


};
