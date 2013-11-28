//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   NetworkPlayer.h
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CNetworkPlayer : MonoBehaviour
{

// Member Types


// Member Functions

    // public:


    public void Start()
    {
		// Empty
    }


    public void OnDestroy()
    {
		// Start
    }


	public void Initialise(RakNet.SystemAddress _cSystemAddress, RakNet.RakNetGUID _cGuid, bool _bHost)
	{
		ResetNetworkViewSteam();

		m_cSystemAddress = new RakNet.SystemAddress(_cSystemAddress.ToString(), _cSystemAddress.GetPort());
		m_cGuid = new RakNet.RakNetGUID(_cGuid.g);
		m_bHost = _bHost;
	}


	public void SetDownloadingInitialGameStateComplete()
	{
		Logger.WriteErrorOn(!m_bDownloadingInitialGameState, "Downloading initial game data has already been set to complete");


		if (m_bDownloadingInitialGameState)
		{
			gameObject.GetComponent<CNetworkConnection>().InvokeRpc(PlayerId, "NotifyDownloadingInitialGameStateComplete");

			m_bDownloadingInitialGameState = false;
		}
	}


	public void ResetNetworkViewSteam()
	{
		m_cNetworkViewStream.Clear();
		m_cNetworkViewStream.Write((byte)RakNet.DefaultMessageIDTypes.ID_TIMESTAMP);
		m_cNetworkViewStream.Write(RakNet.RakNet.GetTime());
		m_cNetworkViewStream.Write((byte)CNetworkConnection.EPacketId.NetworkView);
	}


    public bool IsHost
    {
		get { return (m_bHost); }
    }


	public bool IsDownloadingInitialGameState
	{
		get { return (m_bDownloadingInitialGameState); }
	}


    public uint AccountId
    {
        get { return (m_uiAccountId); }
    }


	public ulong PlayerId
	{
		get { return (Guid.g); }
	}


    public RakNet.SystemAddress SystemAddress
    {
        get { return (m_cSystemAddress); }
    }


    public RakNet.RakNetGUID Guid
    {
        get { return (m_cGuid); }
    }


	public CNetworkStream NetworkViewStream
	{
		get { return (m_cNetworkViewStream); }
	}


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


    uint m_uiAccountId = 0;


	bool m_bHost = false;
	bool m_bDownloadingInitialGameState = true;


    RakNet.SystemAddress m_cSystemAddress = null;
    RakNet.RakNetGUID m_cGuid = null;
	CNetworkStream m_cNetworkViewStream = new CNetworkStream();


};
