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
            string sStringValue = _cObject as string;
            iObjectSize = sStringValue.Length * sizeof(char);
            baByteData = new byte[iObjectSize];


            System.Buffer.BlockCopy(sStringValue.ToCharArray(), 0, baByteData, 0, baByteData.Length);
        }

        return (baByteData);
    }


    public static byte[] ToByteArray<TYPE>(TYPE _tStruct) where TYPE : struct
    {
        int iObjectSize = Marshal.SizeOf(typeof(TYPE));
        Byte[] baByteData = new Byte[iObjectSize];
        
        
		// Create buffer
        IntPtr ipBuffer = Marshal.AllocHGlobal(iObjectSize);


		// Copy structure to buffer
        Marshal.StructureToPtr(_tStruct, ipBuffer, true);

		
		// Copy buffer to byte array
        Marshal.Copy(ipBuffer, baByteData, 0, iObjectSize);


		// Free buffer
        Marshal.FreeHGlobal(ipBuffer);


        return (baByteData);
    }


    public static TYPE ToStruct<TYPE>(byte[] _bData) where TYPE : struct
    {
        int iObjectSize = Marshal.SizeOf(typeof(TYPE));
        IntPtr ipBuffer = Marshal.AllocHGlobal(iObjectSize);
        
       
        Marshal.Copy(_bData, 0, ipBuffer, iObjectSize);
        TYPE tStruct = (TYPE)Marshal.PtrToStructure(ipBuffer, typeof(TYPE));
        
        
        Marshal.FreeHGlobal(ipBuffer);


        return (tStruct);
    }


    public static object ToType(byte[] _baByteArray, Type _cType)
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
            char[] chars = new char[_baByteArray.Length / sizeof(char)];
            System.Buffer.BlockCopy(_baByteArray, 0, chars, 0, _baByteArray.Length);


            cConvertedObject = Convert.ChangeType(new string(chars), typeof(string));
        }


        return (cConvertedObject);
    }


	private byte[] ObjectToByteArray(object source)
	{
		var formatter = new BinaryFormatter();
		using (var stream = new MemoryStream())
		{
			formatter.Serialize(stream, source);                
			return stream.ToArray();
		}
	}


    // protected:


    // private:



// Member Variables
    
    // protected:


    // private:


};
