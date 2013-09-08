//
//  Team S.H.I.T
//  Auckland
//  New Zealand
//
//  Auckland
//  New Zealand
//
//  (c) 2013 S.H.I.T
//
//  File Name   :   Game.h
//  Description :   --------------------------
//
//  Author      :   
//  Mail        :   
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


class CGame : MonoBehaviour
{

    // Member Types


    // Member Functions
    
// public:


    public void Start()
    {
    }
    
    
    public void OnDestroy()
    {
    }
	
	
	public CNetwork GetNetwork()
	{
		if (m_cNetwork == null)
		{
			m_cNetwork = new CNetwork();
		}
		
		return (m_cNetwork);
	}
	
	
	public static CGame GetInstance()
	{
		if (s_cInstance == null)
		{
			s_cInstance = new CGame();
		}
		
		return (s_cInstance);
	}


// protected:


// private:



    // Member Variables
    
// protected:


// private:
	
	
	CNetwork m_cNetwork = null;
	
	
	static CGame s_cInstance = null;


};


/*

public class CGame2 : MonoBehaviour {

	// Use this for initialization
	 
	
	bool isServer;
	void Start ()
	{
		Application.runInBackground = true;
		string str;


		Debug.LogError("(C) or (S)erver?\n");
		str = "c";
	
		if ((str[0]=='c')||(str[0]=='C'))
		{
			SocketDescriptor sd = new SocketDescriptor();
			peer.Startup(1,sd, 1);
			isServer = false;
		} else {
			SocketDescriptor sd = new SocketDescriptor(9000,"");
			peer.Startup(10, sd, 1);
			isServer = true;
		}
	
		if (isServer)
		{
			Debug.LogError("Starting the server.\n");
			// We need to let the server accept incoming connections from the clients
			peer.SetMaximumIncomingConnections(10);
		} else {
			str = "127.0.0.1";
	
			Debug.LogError("Starting the client.\n");
			peer.Connect(str, 9000, "",0);
	
		}
	}
	
	void OnDestroy()
	{
		RakPeerInterface.DestroyInstance(peer);
	}

	
	// Update is called once per frame
	void Update ()
	{
		Packet packet = new Packet();
		
		
	for (packet=peer.Receive(); packet != null; peer.DeallocatePacket(packet), packet=peer.Receive())
		{
			switch (packet.data[0])
			{
			case (byte)DefaultMessageIDTypes.ID_REMOTE_DISCONNECTION_NOTIFICATION:
				Debug.LogError("Another client has disconnected.\n");
				break;
			case (byte)DefaultMessageIDTypes.ID_REMOTE_CONNECTION_LOST:
				Debug.LogError("Another client has lost the connection.\n");
				break;
			case (byte)DefaultMessageIDTypes.ID_REMOTE_NEW_INCOMING_CONNECTION:
				Debug.LogError("Another client has connected.\n");
				break;
			case (byte)DefaultMessageIDTypes.ID_CONNECTION_REQUEST_ACCEPTED:
				{
					Debug.LogError("Our connection request has been accepted.\n");

					// Use a BitStream to write a custom user message
					// Bitstreams are easier to use than sending casted structures, and handle endian swapping automatically
					RakNet.BitStream bsOut = new RakNet.BitStream();
					bsOut.Write((byte)122);
					bsOut.Write("Hello world");
					peer.Send(bsOut,PacketPriority.HIGH_PRIORITY,PacketReliability.RELIABLE_ORDERED,(char)0,packet.systemAddress,false);
				}
				break;
			case (byte)DefaultMessageIDTypes.ID_NEW_INCOMING_CONNECTION:
				Debug.LogError("A connection is incoming.\n");
				break;
			case (byte)DefaultMessageIDTypes.ID_NO_FREE_INCOMING_CONNECTIONS:
				Debug.LogError("The server is full.\n");
				break;
			case (byte)DefaultMessageIDTypes.ID_DISCONNECTION_NOTIFICATION:
				if (isServer){
					Debug.LogError("A client has disconnected.\n");
				} else {
					Debug.LogError("We have been disconnected.\n");
				}
				break;
			case (byte)DefaultMessageIDTypes.ID_CONNECTION_LOST:
				if (isServer){
					Debug.LogError("A client lost the connection.\n");
				} else {
					Debug.LogError("Connection lost.\n");
				}
				break;
				
			case (byte)122:
				{
					RakString rs = new RakString();
					RakNet.BitStream bsIn = new RakNet.BitStream(packet.data,packet.length,false);
					bsIn.IgnoreBytes(sizeof(byte));
					bsIn.Read(rs);
					string convert = rs.C_String();
					Debug.LogError(convert + "\n");
				}
				break;
			
			default:
				Debug.LogError("Message with identifier "+ (byte)packet.data[0] +" has arrived.\n");
				break;
			}
		}
	}
}
*/