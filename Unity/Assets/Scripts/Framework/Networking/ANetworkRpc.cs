//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkRpc.h
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


[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class ANetworkRpc : Attribute
{

// Member Types


// Member Functions
    
    // public:


    public ANetworkRpc()
    {
    }


	public static System.Type[] GetValidParameterArrayTypes()
	{
		return (s_aValidArrayTypes);
	}


	public static System.Type[] GetValidParameterClassTypes()
	{
		return (s_aValidClassTypes);
	}


    // protected:


    // private:


// Member Variables
    
    // protected:


    // private:


	static System.Type[] s_aValidArrayTypes = new System.Type[] { typeof(byte[]), typeof(short[]), typeof(ushort[]), typeof(int[]), typeof(uint[]) };
	static System.Type[] s_aValidClassTypes = new System.Type[] { typeof(string) };


};