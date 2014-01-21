//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CGameChat.cs
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


public class CGameChat : CNetworkMonoBehaviour
{

    // Member Types
    enum ENetworkAction
    {
        INVALID,

     //   ActionSendPlayerName,

        MAX
    }

    // Member Delegates & Events


    // Member Properties


    // Member Methods


    public void Awake() {}


    void Start() {}


    void OnDestroy() {}


    void Update()
    {
        CUserInput.EventReturnKey += new CUserInput.NotifyKeyChange(ReturnKeyChanged);
    }

    [AClientOnly]
    void ReturnKeyChanged(bool _b)
    {

    }


    public override void InstanceNetworkVars()
    {
        //m_sNetworkedPlayerName = new CNetworkVar<string>(OnNetworkVarSync, "");
    }

    public void OnGUI()
    {
        // Game screen and chat box dimenions nad positional information
        float fScreenCenterX = Screen.width / 2;
        float fScreenCenterY = Screen.height / 2;
        float fChatWidth     = 400.0f;
        float fChatHeight    = 100.0f;
        float fChatOriginX   = fScreenCenterX - (fChatWidth * 0.5f);
        float fChatOriginY   = fScreenCenterY + fScreenCenterY - fChatHeight;
        float fInputOffset   = 5.0f;
        float fInputWidth    = fChatWidth - (fInputOffset * 2.0f);
        float fInputHeight   = 20.0f;
        float fInputOriginX  = fChatOriginX + fInputOffset;
        float fInputOriginY  = fChatOriginY + fChatHeight - fInputHeight - 1.0f;

        // Create and render background for chat box
        GUI.Box(new Rect(fChatOriginX, fChatOriginY, fChatWidth, fChatHeight), "Chat Window");

        // Assign new text input field to input string
        m_sPlayerChatInput = GUI.TextField(new Rect(fInputOriginX, fInputOriginY, fInputWidth, fInputHeight), m_sPlayerChatInput, 32);
        m_TextInputBoxID = GUIUtility.keyboardControl;
        GUIUtility.keyboardControl = 0;
    }

    //void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    //{
        //if (_cSyncedNetworkVar == m_sNetworkedPlayerName)
        //{
        //    bool bAddToList = true;

        //    foreach (string Name in m_PlayerNamesList)
        //    {
        //        if (Name == m_sNetworkedPlayerName.Get())
        //        {
        //            bAddToList = false;
        //        }
        //    }

        //    if (bAddToList)
        //    {
        //        m_PlayerNamesList.Add(m_sNetworkedPlayerName.Get());
        //        Debug.Log("Added " + m_sNetworkedPlayerName.Get() + " to game");
        //    }
        //    else
        //    {
        //        Debug.Log("Name " + m_sNetworkedPlayerName.Get() + " Was already taken!");
        //    }
        //}
  //  }

//    public static void SerializeData(CNetworkStream _cStream)
  //  {
        //_cStream.Write((byte)ENetworkAction.ActionSendPlayerName);
        //_cStream.WriteString(m_sPlayerName);
   // }


 //   public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
   // {
        //ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();

        //switch (eNetworkAction)
        //{
        //    case ENetworkAction.ActionSendPlayerName:
        //        {
        //            m_sPlayerName = _cStream.ReadString();
        //            s_cInstance.m_sNetworkedPlayerName.Set(m_sPlayerName);

        //            break;
        //        }
        //}
    //}


    


    //[ANetworkRpc]
    //void RegisterPlayerActor(ulong _ulPlayerId, CNetworkViewId _cPlayerActorId)
    //{
    //    m_mPlayersActor.Add(_ulPlayerId, _cPlayerActorId);
    //}


    //[ANetworkRpc]
    //void UnregisterPlayerActor(ulong _ulPlayerId)
    //{
    //    m_mPlayersActor.Remove(_ulPlayerId);
    //    m_aUnspawnedPlayers.Remove(_ulPlayerId);
    //}


    //void OnGUI()
    //{
    //    if (CGamePlayers.SelfActor == null)
    //    {
    //        // Draw un-spawned message
    //        GUIStyle cStyle = new GUIStyle();
    //        cStyle.fontSize = 40;
    //        cStyle.normal.textColor = Color.white;

    //        GUI.Label(new Rect(Screen.width / 2 - 290, Screen.height / 2 - 50, 576, 100),
    //                  "Waiting for spawner to be available...", cStyle);
    //    }
    //}


    // Member Fields


    string m_sPlayerChatInput = "";
    int m_TextInputBoxID = 0;

    //Dictionary<ulong, CNetworkViewId> m_mPlayersActor = new Dictionary<ulong, CNetworkViewId>();
    //List<ulong> m_aUnspawnedPlayers = new List<ulong>();

    //CNetworkVar<string> m_sNetworkedPlayerName = null;
    //List<string> m_PlayerNamesList = new List<string>();

    //static string m_sPlayerName = System.Environment.UserDomainName + ": " + System.Environment.UserName;

    //static CGamePlayers s_cInstance = null;


};
