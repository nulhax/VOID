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


public class CMiningTurretBehaviour : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{

	}


// Member Delegates & Events


// Member Properties


// Member Methods


	public override void InstanceNetworkVars()
	{

	}


	[AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
	{
		_cStream.Write(s_cSerializeStream);
		s_cSerializeStream.Clear();
	}
	
	
	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		while (_cStream.HasUnreadData)
		{
			CNetworkViewId cTurretViewId = _cStream.ReadNetworkViewId();
			GameObject cTurretObject = CNetwork.Factory.FindObject(cTurretViewId);
			
			if (cTurretObject != null)
			{
				CMiningTurretBehaviour cMiningTurretBehaviour = cTurretObject.GetComponent<CMiningTurretBehaviour>();		
				ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();
				
				switch (eAction)
				{
				//case ENetworkAction.FireLasers:
				//	break;
					
				default:
					Debug.LogError(string.Format("Unknown network action ({0})", eAction));
					break;
				}
			}
		}
	}


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
	}


	void OnNetworkVarSync(INetworkVar _cSyncedVar)
	{
	}


// Member Fields
	
	
	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
