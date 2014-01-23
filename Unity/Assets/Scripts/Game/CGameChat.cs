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
        INVALID = -1,

        ActionSendPlayerMessage,

        MAX
    }


    // Member Delegates & Events


    // Member Properties
    public static CGameChat Instance
    {
        get { return (m_Instance); }
    }


    // Member Methods


    public void Awake()
    {
        // Save a static reference to this class
        m_Instance = this;
    }


    void Start()
    {
        CUserInput.EventReturnKey += new CUserInput.NotifyKeyChange(ReturnKeyChanged);

        // Deprecated function
        // NOTE: Causes input to be shared across the entire
        // application even if a GUI component is focussed
        Input.eatKeyPressOnTextFieldFocus = false;
    }


    void OnDestroy() {}


    void Update() {}


    public override void InstanceNetworkVars()
    {
        //m_sNetworkedPlayerName = new CNetworkVar<string>(OnNetworkVarSync, "");
    }


    [AClientOnly]
    void ReturnKeyChanged(bool _b)
    {
        if (_b)
        {
            m_bProcessChat = true;
        }
    }


    void ProcessChatFrame()
    {
        // Switch on currently focused GUI frame
        switch (GUI.GetNameOfFocusedControl())
        {
            // If current focus is chat window input
            case m_sChatInputControlName:
            {
                // If the player input string has something in it
                if (m_sPlayerChatInput.Length != 0)
                {
                    // Send message
                    m_bSendMessage = true;
                    Debug.Log("Send bool: " + m_bSendMessage.ToString());
                }

                // De-focus chat frame
                GUI.FocusControl(null);

                break;
            }

            // Else
            default:
            {
                // Focus chat frame
                GUI.FocusControl(m_sChatInputControlName);

                break;
            }
        }
    }

    public static void SerializeData(CNetworkStream _cStream)
    {
        if (Instance.m_bSendMessage)
        {
            Debug.Log("Serialize entered");

            // Write the first byte to the stream as a network action
            _cStream.Write((byte)ENetworkAction.ActionSendPlayerMessage);

            // Write player's name to stream

            // Write player's message to string
            _cStream.WriteString(m_sPlayerChatInput);
            Debug.Log(m_sPlayerChatInput);

            // Clear player input
            m_sPlayerChatInput = "";

            Instance.m_bSendMessage = false;

            Debug.Log("Send bool: " + Instance.m_bSendMessage.ToString());
        }
    }

    public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        // While there is unread data in the stream
        while (_cStream.HasUnreadData)
        {
            Debug.Log("Unserialize entered");

            // Save the first byte as the network action
            ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();

            // Switch on the network action
            switch (eNetworkAction)
            {
                    // New player message was sent
                case ENetworkAction.ActionSendPlayerMessage:
                    {
                        Debug.Log("Reading stream");

                        string strName    = _cStream.ReadString();
                        string strMessage = _cStream.ReadString();

                        // Read out message into output string
                        Instance.InvokeRpcAll("ReceivePlayerMessage", strName, strMessage);

                        break;
                    }
            }
        }
    }

    [ANetworkRpc]
    void ReceivePlayerMessage(string _strPlayerName, string _strMessage)
    {
        Debug.Log("RPC entered");

        m_sPlayerChatOuput += "[" + _strPlayerName + "]: " + _strMessage + "\n";

        Debug.Log(m_sPlayerChatOuput);
    }
    

    public void OnGUI()
    {
        if (CNetwork.Connection.IsConnected)
        {
            // Game screen and chat box dimenions and positional information
            float fScreenCenterX = Screen.width / 2;                                    // Centre of the screen relative to the origin on the X axis
            float fScreenCenterY = Screen.height / 2;                                   // Centre of the screen relative to the origin on the Y axis
            float fChatWidth     = 400.0f;                                              // Width of the chat frame
            float fChatHeight    = 100.0f;                                              // Height of the chat frame
            float fChatOriginX   = fScreenCenterX - (fChatWidth * 0.5f);                // Origin of the chat frame on the X axis
            float fChatOriginY   = fScreenCenterY + fScreenCenterY - fChatHeight;       // Origin of the chat frame on the Y axis
            float fInputOffset   = 5.0f;                                                // Offset distance between the input bar and the edge of the chat frame
            float fInputWidth    = fChatWidth - (fInputOffset * 2.0f);                  // Input bar width
            float fInputHeight   = 20.0f;                                               // Input bar height
            float fInputOriginX  = fChatOriginX + fInputOffset;                         // Input bar origin on the X axis
            float fInputOriginY  = fChatOriginY + fChatHeight - fInputHeight - 1.0f;    // Input bar origin on the Y axis

            // Create and render background for chat box
            GUI.TextArea(new Rect(fChatOriginX, fChatOriginY, fChatWidth, fChatHeight), m_sPlayerChatOuput);

            // Assign new text input field to input string
            GUI.SetNextControlName(m_sChatInputControlName);
            m_sPlayerChatInput = GUI.TextField(new Rect(fInputOriginX, fInputOriginY, fInputWidth, fInputHeight), m_sPlayerChatInput, 255);

            Debug.Log(m_sPlayerChatInput);

            // If return key was pressed
            if (m_bProcessChat)
            {
                // Process the chat frame
                ProcessChatFrame();

                // Stop processing chat
                m_bProcessChat = false;
            }
        }
    }


    bool m_bSendMessage = false;
    bool m_bProcessChat       = false;
    static string m_sPlayerChatInput = "";
    static string m_sPlayerChatOuput = "";
    //string[] m_sStringArray = new string[5];
    const string m_sChatInputControlName = "ChatInputTextField";
    static string m_sPlayerName = "[" + System.Environment.UserName + "]: ";
    static CGameChat m_Instance;

    //Dictionary<ulong, CNetworkViewId> m_mPlayersActor = new Dictionary<ulong, CNetworkViewId>();
    //List<ulong> m_aUnspawnedPlayers = new List<ulong>();

    //CNetworkVar<string> m_sNetworkedPlayerName = null;
    //List<string> m_PlayerNamesList = new List<string>();

    //static string m_sPlayerName = System.Environment.UserDomainName + ": " + System.Environment.UserName;

    //static CGamePlayers s_cInstance = null;

    /////////////////////////////////////////////////////////////////

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


};
