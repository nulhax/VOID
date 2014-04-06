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
        // Sign up for events
        CNetwork.Connection.EventConnectionAccepted += new CNetworkConnection.OnConnect(OnConnectEventSignup);

        // Deprecated function
        // NOTE: Causes input to be shared across the entire
        // application even if an OnGui component is focussed
        Input.eatKeyPressOnTextFieldFocus = false;
    }


    void OnDestroy()
    {
        CUserInput.UnsubscribeInputChange(CUserInput.EInput.ReturnKey, ReturnKeyChanged);
    }


    void Update() {}


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "ReceivePlayerMessage");
    }


    [ALocalOnly]
    void ReturnKeyChanged(CUserInput.EInput _eInput, bool _b)
    {
		// This will only be able to trigger every 0.5 seconds
		if (Time.time > m_fTimeOfEnterKeyPress + 0.5f) 
		{
			// If keydown
			if (_b) 
			{
				// Toggle operation on each key press down
				// If chat was enabled, it will be disabled and movement will be re-enabled
				if(m_bProcessChat)
				{
					m_bProcessChat = false;

                    // TODO: Enable actor movement
					//CGamePlayers.SelfActor.GetComponent<CPlayerGroundMotor> ().UndisableInput (this); 
					          
				}

				// If chat was disabled, it will be enabled and movement will be disabled
				else
				{
					m_bProcessChat = true;

                    // TODO: Disable actor movement
					//CGamePlayers.SelfActor.GetComponent<CPlayerGroundMotor> ().DisableInput (this);  
				}	
			}

			m_fTimeOfEnterKeyPress = Time.time;
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
            // Write the first byte to the stream as a network action
            _cStream.Write((byte)ENetworkAction.ActionSendPlayerMessage);

            // Write player's name to stream
            _cStream.Write(CGamePlayers.Instance.LocalPlayerName);

            // Write player's message to string
            _cStream.Write(m_sPlayerChatInput);

            // Clear player input
            m_sPlayerChatInput = "";

            // Stop sending the message
            Instance.m_bSendMessage = false;
        }
    }


    public static void UnserializeData(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        // While there is unread data in the stream
        while (_cStream.HasUnreadData)
        {
            // Save the first byte as the network action
            ENetworkAction eNetworkAction = (ENetworkAction)_cStream.Read<byte>();

            // Switch on the network action
            switch (eNetworkAction)
            {
                    // New player message was sent
                case ENetworkAction.ActionSendPlayerMessage:
                {
                    // Read out messages into output strings
                    string strName    = _cStream.Read<string>();
                    string strMessage = _cStream.Read<string>();

                    // Find all players within 'hearing' range of the source player
                    foreach (CNetworkPlayer Player in CheckPlayerDistances(_cNetworkPlayer))
                    {
                        // Invoke RPC call to send the message to each player
                        Instance.InvokeRpc(Player.PlayerId, "ReceivePlayerMessage", strName, strMessage);
                    }

                    break;
                }
            }
        }
    }


    [ANetworkRpc]
    void ReceivePlayerMessage(string _strPlayerName, string _strMessage)
    {
        // Concatonate all of the strings into a single output
        m_sPlayerChatOuput += "[" + _strPlayerName + "]: " + _strMessage + "\n";
    }


    void OnConnectEventSignup()
    {
        // Sign up for events
        CUserInput.SubscribeInputChange(CUserInput.EInput.ReturnKey, ReturnKeyChanged);
    }


    [AServerOnly]
    static List<CNetworkPlayer> CheckPlayerDistances(CNetworkPlayer _SourcePlayer)
    {
        // Temporary return list of players within range of the source
        List<CNetworkPlayer> ReturnPlayerList = new List<CNetworkPlayer>();

        // Dictionary of all connected players
        // NOTE: This dictionary includes the source player
        Dictionary<ulong, CNetworkPlayer> PlayerList = CNetwork.Server.GetNetworkPlayers();

        // Loop to iterate through all players including the source
        foreach (KeyValuePair<ulong, CNetworkPlayer> Player in PlayerList)
        {
            // If the current player is not the source player
            if (Player.Key != _SourcePlayer.PlayerId)
            {
                // Add the source's position vector to the current player's vector
                // If the magnitude of the resulting vector is greater than 10.0f
                if ((Player.Value.transform.position + _SourcePlayer.transform.position).magnitude >= 10.0f)
                {
                    Debug.Log((Player.Value.transform.position + _SourcePlayer.transform.position).magnitude.ToString());
                    // Add the player to the return list
                    ReturnPlayerList.Add(Player.Value);
                }
            }
        }

        // If the return list is empty
        if (ReturnPlayerList.Count == 0)
        {
            // Invoke an RPC call on the source player
            Instance.InvokeRpc(_SourcePlayer.PlayerId, "ReceivePlayerMessage", "Server", "There are no players nearby who can hear your message.");
        }

        // Return list of players within range of the source
        return (ReturnPlayerList);
    }


    public void OnGUI()
    {
		return;
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


    // Members
    bool m_bSendMessage                  = false;
    bool m_bProcessChat                  = false;
    static bool m_bNoPlayersInRange      = false;
	float m_fTimeOfEnterKeyPress		 = 0.0f;
    static string m_sPlayerChatInput     = "";
    static string m_sPlayerChatOuput     = "";
    const string m_sChatInputControlName = "ChatInputTextField";
    static string m_sPlayerName          = "[" + System.Environment.UserName + "]: ";
    static CGameChat m_Instance;
};
