//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CNetworkPlayer.h
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


    public void OnAwake()
    {
        // Assign first avaiable network player id
        for (uint i = 0; i < uint.MaxValue; ++ i)
        {
            if (!s_mNetworkPlayers.ContainsKey(i))
            {
                s_mNetworkPlayers.Add(i, this);
                m_uiPlayerId = i;
            }
        }
    }


    public void SendNetworkViewData(ushort _usNetworkViewId, CPacketStream _cStream)
    {
        CGame.Server.GetPeer().Send(_cStream.GetBitStream(), RakNet.PacketPriority.HIGH_PRIORITY, RakNet.PacketReliability.RELIABLE_ORDERED, (char)0, m_cSystemAddress, false);
    }


    uint PlayerId
    {
        get { return (m_uiPlayerId); }
    }


    RakNet.SystemAddress SystemAddress
    {
        set { m_cSystemAddress = value; }
        get { return (m_cSystemAddress); }
    }


    public static Dictionary<uint, CNetworkPlayer> FindAll()
    {
        return (s_mNetworkPlayers);
    }


    public static CNetworkPlayer FindUsingPlayerId(uint _uiPlayerId)
    {
        return (s_mNetworkPlayers[_uiPlayerId]);
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


    uint m_uiPlayerId = 0;


    RakNet.SystemAddress m_cSystemAddress = new RakNet.SystemAddress();
    CPacketStream m_cTransmissionStream = new CPacketStream();
    
    
    static Dictionary<uint, CNetworkPlayer> s_mNetworkPlayers = new Dictionary<uint, CNetworkPlayer>();


};
