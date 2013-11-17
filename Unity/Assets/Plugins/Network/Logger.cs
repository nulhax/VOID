//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CLogger.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class Logger
{

// Member Types


    public enum EGroup
    {
        GLOBAL,
        NETWORKING,
    }


// Member Functions

    // public:


    public static void WriteError(EGroup _eGroup, string _sMessageFormat, params object[] _caParameters)
    {
        WriteError(_sMessageFormat, _caParameters);
    }


    public static void WriteWarning(EGroup _eGroup, string _sMessageFormat, params object[] _caParameters)
    {
        WriteWarning(_sMessageFormat, _caParameters);
    }


    public static void WriteMessage(EGroup _eGroup, string _sMessageFormat, params object[] _caParameters)
    {
        Write(_sMessageFormat, _caParameters);
    }


    public static void WriteErrorOn(bool _bCondition, string _sMessageFormat, params object[] _caParameters)
    {
        if (_bCondition)
        {
            WriteError(_sMessageFormat, _caParameters);
        }
    }


    public static void WriteWarningOn(bool _bCondition, string _sMessageFormat, params object[] _caParameters)
    {
        if (_bCondition)
        {
            WriteWarning(_sMessageFormat, _caParameters);
        }
    }


    public static void WriteError(string _sMessageFormat, params object[] _caParameters)
    {
#if UNITY_EDITOR
        Debug.LogError(string.Format(_sMessageFormat, _caParameters));
#endif
    }


    public static void WriteWarning(string _sMessageFormat, params object[] _caParameters)
    {
#if UNITY_EDITOR
        Debug.LogWarning(string.Format(_sMessageFormat, _caParameters));
#endif
    }


    public static void Write(string _sMessageFormat, params object[] _caParameters)
    {
#if UNITY_EDITOR
        //Debug.Log(string.Format(_sMessageFormat, _caParameters));
#endif
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


};
