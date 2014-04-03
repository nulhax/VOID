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


// Member Properties


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


    public CNetworkStream BufferedStream
    {
        get { return (m_cBufferedStream); }
    }


    public CNetworkStream UnbufferedStream
    {
        get { return (m_cUnbufferedStream); }
    }


// Member Functions


    public void Start()
    {
		// Empty
    }


	public void Initialise(RakNet.SystemAddress _cSystemAddress, RakNet.RakNetGUID _cGuid, bool _bHost)
	{
		ResetBufferedSteam();
        ResetUnbufferedSteam();

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


	public void ResetBufferedSteam()
	{
		m_cBufferedStream.Clear();
		m_cBufferedStream.Write((byte)CNetworkConnection.EPacketId.NetworkView);
	}


    public void ResetUnbufferedSteam()
    {
        m_cUnbufferedStream.Clear();
        m_cUnbufferedStream.Write((byte)CNetworkConnection.EPacketId.NetworkView);
    }


// Member Fields


    RakNet.SystemAddress m_cSystemAddress = null;
    RakNet.RakNetGUID m_cGuid = null;


	CNetworkStream m_cBufferedStream = new CNetworkStream();
    CNetworkStream m_cUnbufferedStream = new CNetworkStream();


    uint m_uiAccountId = 0;


    bool m_bHost = false;
    bool m_bDownloadingInitialGameState = true;


};
