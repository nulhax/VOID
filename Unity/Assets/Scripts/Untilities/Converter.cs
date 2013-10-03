//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Converter.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


/* Implementation */
// http://wingerlang.blogspot.co.nz/2011/11/c-struct-to-byte-array-and-back.html


public class Converter
{

// Member Types


// Member Functions
    
    // public:


    public static int GetSizeOf(Type _cType)
    {
        int iSize = 0;

        if (_cType.IsEnum)
        {
            iSize = Marshal.SizeOf(Enum.GetUnderlyingType(_cType));
        }
        else if (_cType == typeof(string))
        {
            
        }
        else
        {
            iSize = Marshal.SizeOf(_cType);
        }

        return (iSize);
    }


    public static int GetSizeOf(object _cObject)
    {
        Type cObjectType = _cObject.GetType();
        int iSize = 0;

        if (cObjectType.IsEnum)
        {
            iSize = Marshal.SizeOf(Enum.GetUnderlyingType(cObjectType));
        }
        else if (cObjectType == typeof(string))
        {
            string sValue = _cObject as string;
            iSize = sValue.Length;
        }
        else
        {
            iSize = Marshal.SizeOf(cObjectType);
        }

        return (iSize);
    }


    public static byte[] ToByteArray(object _cObject, Type _cType = null)
    {
        Type cObjectType = _cType;


        if (_cType == null)
        {
            cObjectType = _cObject.GetType();
        }


        byte[] baByteData = null;
        int iObjectSize = GetSizeOf(cObjectType);
        

        if (cObjectType != typeof(string))
        {
            baByteData = new byte[iObjectSize];

            // Create buffer
            IntPtr ipBuffer = Marshal.AllocHGlobal(iObjectSize);

            // Copy structure to buffer
            Marshal.StructureToPtr(_cObject, ipBuffer, true);

            // Copy buffer to byte array
            Marshal.Copy(ipBuffer, baByteData, 0, iObjectSize);

            // Free buffer
            Marshal.FreeHGlobal(ipBuffer);
        }
        else
        {
            //string sStringValue = _cObject as string;
            //iObjectSize = sStringValue.Length * sizeof(char);
            //baByteData = new byte[iObjectSize];


            //System.Buffer.BlockCopy(sStringValue.ToCharArray(), 0, baByteData, 0, baByteData.Length);

            baByteData = Encoding.ASCII.GetBytes((string)_cObject);
        }

        return (baByteData);
    }


    public static object ToObject(byte[] _baByteArray, Type _cType)
    {
        object cConvertedObject = null;


        if (_cType != typeof(string))
        {
            int iTypeSize = GetSizeOf(_cType);


            IntPtr ipBuffer = Marshal.AllocHGlobal(iTypeSize);


            Marshal.Copy(_baByteArray, 0, ipBuffer, iTypeSize);
            cConvertedObject = Marshal.PtrToStructure(ipBuffer, _cType);


            Marshal.FreeHGlobal(ipBuffer);
        }
        else
        {
            //int iStringLength = _baByteArray.Length;
            //Logger.WriteError(iStringLength / sizeof(char));

            //char[] chars = new char[iStringLength / sizeof(char)];
            //System.Buffer.BlockCopy(_baByteArray, 0, chars, 0, iStringLength);


            //cConvertedObject = Convert.ChangeType(new string(chars), typeof(string));
            cConvertedObject = Encoding.UTF8.GetString(_baByteArray, 0, _baByteArray.Length);
        }


        return (cConvertedObject);
    }


    // protected:


    // private:



// Member Variables
    
    // protected:


    // private:


};
