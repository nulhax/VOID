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


    public void Awake()
    {
        // Assign first avaiable network player id
        for (uint i = 1; i < uint.MaxValue; ++ i)
        {
            if (!s_mNetworkPlayers.ContainsKey(i))
            {
                s_mNetworkPlayers.Add(i, this);
                m_uiPlayerId = i;
                break;
            }
        }

        // Initial reset of stream
        ResetPacketStream();
    }


    public void ResetPacketStream()
    {
        m_cStream.Clear();

        // Set packet identifier
        m_cStream.Write((byte)CNetworkConnection.EPacketId.NetworkView);
    }


    public bool IsHost()
    {
        return (m_uiPlayerId == 1);
    }


    public uint PlayerId
    {
        get { return (m_uiPlayerId); }
    }


    public uint AccountId
    {
        set
        {
            if (m_uiAccountId != 0)
            {
                Debug.LogError(string.Format("The account id of network players cannot be changed once set"));
            }
            else
            {
                m_uiAccountId = value;
            }
        }

        get { return (m_uiAccountId); }
    }


    public ushort ActorNetworkViewId
    {
        set
        {
            if (m_usActorNetworkViewId != 0)
            {
                Debug.LogError(string.Format("Actor network view id ({0}) for player id ({1}) cannot be change once set", m_usActorNetworkViewId, this.PlayerId));
            }
            else
            {
                m_usActorNetworkViewId = value;
            }
        }

        get { return (m_usActorNetworkViewId); }
    }


    public GameObject ActorGameObject
    {
        get
        {
            return (CNetworkView.FindUsingViewId(m_usActorNetworkViewId).gameObject);
        }
    }


    public RakNet.SystemAddress SystemAddress
    {
        set { m_cSystemAddress = value; }
        get { return (m_cSystemAddress); }
    }


    public CPacketStream PacketStream
    {
        get { return (m_cStream); }
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
    uint m_uiAccountId = 0;


    ushort m_usActorNetworkViewId = 0;


    RakNet.SystemAddress m_cSystemAddress = null;
    CPacketStream m_cStream = new CPacketStream();


    static Dictionary<uint, CNetworkPlayer> s_mNetworkPlayers = new Dictionary<uint, CNetworkPlayer>();


};
