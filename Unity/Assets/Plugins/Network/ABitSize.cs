//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   AClientMethod.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System;


/* Implementation */


[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public sealed class ABitSize : Attribute
{

// Member Types


    public byte BitCount
    {
        get { return (m_bBitCount); }
    }


// Member Functions
    
    // public:


    public ABitSize(byte _bNumBytes)
    {
        m_bBitCount = _bNumBytes;
    }


    // protected:


    // private:


// Member Variables
    
    // protected:


    // private:


    byte m_bBitCount = 0;


};