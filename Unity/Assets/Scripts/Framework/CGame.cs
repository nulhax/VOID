//
//  Team S.H.I.T
//  Auckland
//  New Zealand
//
//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   Game.h
//  Description :   --------------------------
//
//  Author      :   
//  Mail        :   
//


#pragma warning disable 0649


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


class CGame : MonoBehaviour
{

// Member Types


// Member Functions
    
    // public:


    public void Awake()
    {
		Application.runInBackground = true;
        s_cInstance = this;
    }
    
    
    public void OnDestroy()
    {
    }
	

	public void Update()
	{
		// Update timers
		CTimer.UpdateTimers(Time.deltaTime);


        if (Input.GetKeyDown(KeyCode.P))
        {
            Factory.CreateGameObject(CNetworkFactory.EPrefab.Player1);
        }
	}


    public void OnGUI()
    {
        float fWidth = 200;
        float fHeight = 100;


        if (!CGame.Connection.IsConnected() && 
            !CGame.Server.IsActive() &&
            GUI.Button(new Rect(Screen.width / 2 - fWidth / 2, Screen.height / 2 - fHeight, fWidth, fHeight), "Start Server"))
        {
            CGame.Server.HostServer("My Awesome Server", 20, 30001);
        }


        if (!CGame.Connection.IsConnected() &&
            GUI.Button(new Rect(Screen.width / 2 - fWidth / 2, Screen.height / 2 + fHeight, fWidth, fHeight), "Join Server"))
        {
            CGame.Connection.ConnectToServer((ushort)Random.Range(10000, 20000), "1916C013", 30001, "");
        }
    }


    public static bool IsServer()
    {
        return (Server.IsActive());
    }


    public static bool IsConnectedToServer()
    {
        return (Connection.IsConnected());
    }


    public static CNetworkConnection Connection
    {
        get { return (s_cInstance.GetComponent<CNetworkConnection>()); }
    }


    public static CNetworkServer Server
    {
        get { return (s_cInstance.GetComponent<CNetworkServer>()); }
    }


    public static CNetworkFactory Factory
    {
        get { return (s_cInstance.GetComponent<CNetworkFactory>()); }
    }


    public static CNetworkPlayerController PlayerController
    {
        get { return (s_cInstance.GetComponent<CNetworkPlayerController>()); }
    }


    // protected:


    // private:


    static CGame GetInstance()
    {
        return (s_cInstance);
    }



// Member Variables
    
    // public:


    // private:

	
	static CGame s_cInstance = null;


};