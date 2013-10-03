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


    public bool HasOutboundData()
    {
        return (m_cStream.GetSize() > 1);
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
                Logger.WriteError("The account id of network players cannot be changed once set");
            }
            else
            {
                m_uiAccountId = value;
            }
        }

        get { return (m_uiAccountId); }
    }


    public GameObject Actor
    {
        set
        {
            m_usActorNetworkViewId = value.GetComponent<CNetworkView>().ViewId;
        }

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


    public RakNet.RakNetGUID Guid
    {
        set
        {
            if (m_cGuid != null)
            {
                Logger.WriteError("Player GUIDs cannot be changed once set. Player id ({0})", this.PlayerId);
            }
            else
            {
                m_cGuid = value;
                s_mGuidNetworkPlayers.Add(m_cGuid.g, this);
            }
        }

        get { return (m_cGuid); }
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


    public static CNetworkPlayer FindUsingGuid(RakNet.RakNetGUID _cGuid)
    {
        return (s_mGuidNetworkPlayers[_cGuid.g]);
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
    RakNet.RakNetGUID m_cGuid = null;
    CPacketStream m_cStream = new CPacketStream();


    static Dictionary<uint, CNetworkPlayer> s_mNetworkPlayers = new Dictionary<uint, CNetworkPlayer>();
    static Dictionary<ulong, CNetworkPlayer> s_mGuidNetworkPlayers = new Dictionary<ulong, CNetworkPlayer>();


};
