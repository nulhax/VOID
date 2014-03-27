//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerBelt.cs
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


public class CPlayerBelt : CNetworkMonoBehaviour
{

// Member Types


    [ABitSize(4)]
    public enum ENetworkAction : byte
    {
        INVALID,

		PickupTool,
		EquipTool,
		ReloadTool,
        DropTool,

        MAX
    }


    public enum EState
    {
        INVALID,

        Idle,
        ChaningTool,
        DroppingTool,

        MAX
    }


    public enum ESwitchToolState
    {
        INVALID,

        UnequipingTool,
        UnequipingToolOnly,
        EquipingTool,

        MAX
    }


// Member Delegates & Events


    [ALocalOnly]
    public delegate void HandleEquipedToolChanged(GameObject _cTool);
    public event HandleEquipedToolChanged EventEquipedToolChanged;


    [ALocalOnly]
    public delegate void HandleToolDropped(GameObject _cTool);
    public event HandleToolDropped EventToolDropped;


    [ALocalOnly]
    public delegate void HandleToolPickedup(GameObject _cTool);
    public event HandleToolPickedup EventToolPickedup;
	

// Member Properties


	public GameObject ActiveTool
	{
		get 
		{
			if (ActiveToolViewId != null)
			{
				return (CNetwork.Factory.FindObject(ActiveToolViewId));
			}

			return (null);
		}
	}


	public byte ActiveSlotId
	{
		get { return (m_bActiveToolId.Get()); }
	}


	public CNetworkViewId ActiveToolViewId
	{
		get { return (m_acToolsViewId[ActiveSlotId].Get()); }
	}


	public byte ToolCapacity
	{
		get { return (m_bToolCapacity.Get()); }
	}
	
	
// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteNotifySwitchingTool");

		m_acToolsViewId = new CNetworkVar<CNetworkViewId>[k_bMaxNumTools];

		for (uint i = 0; i < k_bMaxNumTools; ++i)
		{
			m_acToolsViewId[i] = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync);
		}

        m_eState = _cRegistrar.CreateNetworkVar<EState>(OnNetworkVarSync, EState.Idle);
		m_bToolCapacity = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 2);
        m_bActiveToolId = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, k_bNoActiveToolId);
    }


	[AServerOnly]
	public void PickupTool(GameObject _cTool)
	{
        // Check object is a tool
        if (_cTool.GetComponent<CToolInterface>() == null)
        {
            Debug.LogError(string.Format("Cannot pick up tool. Object does not have a CToolInterface component. GameObject({0})", _cTool));
        }
        
		// Check object exists
		else if (_cTool != null)
		{
			// Find free slot
			for (uint i = 0; i < ToolCapacity; ++i)
			{
                // Check slot if free
				if (GetToolViewId(i) == null)
				{
					// Retrieve tool interface script
					CToolInterface cToolInterface = _cTool.GetComponent<CToolInterface>();
					CNetworkView cToolNetworkView = _cTool.GetComponent<CNetworkView>();

					// Check tool is not owned
                    if (!cToolInterface.IsOwned)
                    {
                        // Set tool to slot
						m_acToolsViewId[i].Set(cToolNetworkView.ViewId);

                        // Notify observers
                        cToolInterface.NotifyPickedUp(m_ulOwnerPlayerId);

                        // Change to this tool
						ChangeTool((byte)i);

                        break;
					}
				}
			}
		}
	}


	[AServerOnly]
	public void ChangeTool(byte _bSlotId)
	{
        // Check slot if is valid
		// Check tool exists
		if (_bSlotId < 0 ||
            _bSlotId >= k_bMaxNumTools)
        {
            Debug.LogError(string.Format("Invalid tool slot id. SlotId({0})", _bSlotId));
        }
        else if (GetToolViewId(_bSlotId) != null)
		{
            m_bActiveToolId.Set(_bSlotId);
		}
	}


	[AServerOnly]
	public void ReloadTool(byte _bSlotId)
	{
		// Check tool exists
		if (GetToolViewId(_bSlotId) != null)
		{
			GetTool(_bSlotId).GetComponent<CToolInterface>().Reload();
		}
	}


	[AServerOnly]
	public void DropTool(byte _bSlotId)
	{
		// Check tool exists
		if (GetToolViewId(_bSlotId) != null)
		{
            bool bNewToolFound = false;

            // Notify tool about being dropped
            GetTool(_bSlotId).GetComponent<CToolInterface>().NotifyDropped();

            // Remove tool from slot
            m_acToolsViewId[_bSlotId].Set(null);

            // Change tool to next available tool
            for (int i = (int)k_bMaxNumTools - 1; i >= 0; --i)
            {
                if (m_acToolsViewId[i].Get() != null)
                {
                    ChangeTool((byte)i);
                    break;
                }
            }

            if (!bNewToolFound)
            {
                ChangeTool(k_bMaxNumTools);
            }
		}
	}


	public GameObject GetTool(uint _bSlotId)
	{
        // Check tool exists
        if (GetToolViewId(_bSlotId) == null)
        {
            return (null);
        }

		return (GetToolViewId(_bSlotId).GameObject);
	}


	public CNetworkViewId GetToolViewId(uint _bSlotId)
	{
		return (m_acToolsViewId[_bSlotId].Get());
	}


    [ALocalOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            // Extract network action
            ENetworkAction eAction = _cStream.Read<ENetworkAction>();

            // Get player belt instance
            CPlayerBelt cPlayerBelt = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerBelt>();

            switch (eAction)
            {
                case ENetworkAction.PickupTool:
                    {
                        CNetworkViewId cToolViewId = _cStream.Read<CNetworkViewId>();

                        cPlayerBelt.PickupTool(cToolViewId.GameObject);
                    }
                    break;

                case ENetworkAction.ReloadTool:
                    {
                        byte bToolSlot = _cStream.Read<byte>();

                        cPlayerBelt.ReloadTool(bToolSlot);
                    }
                    break;

                case ENetworkAction.DropTool:
                    {
                        byte bToolSlot = _cStream.Read<byte>();

                        cPlayerBelt.DropTool(bToolSlot);
                    }
                    break;

                case ENetworkAction.EquipTool:
                    {
                        byte bToolSlot = _cStream.Read<byte>();

                        cPlayerBelt.ChangeTool(bToolSlot);
                    }
                    break;

                default:
                    Debug.LogError("Unknown network action: " + eAction);
                    break;
            }
        }
    }


    void Start()
    {
        m_ulOwnerPlayerId = GetComponent<CPlayerInterface>().PlayerId;

        // Owner player subscribe to events
        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerInteractor>().EventUse += OnEventInteractionUse;

            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary,             OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary,           OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_Reload,         OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_Drop,           OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot1, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot2, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot3, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot4, OnEventInput);

            m_vToolEquipedPosition = GetComponent<CPlayerInterface>().Model.transform.FindChild("ToolActive").transform.localPosition;
            m_vToolUnequipedPosition = GetComponent<CPlayerInterface>().Model.transform.FindChild("ToolDeactive").transform.localPosition;
        }

        // Signup to pre destroy
        gameObject.GetComponent<CNetworkView>().EventPreDestory += OnEventPreDestroy;
    }


    void OnEventPreDestroy()
    {
        // Drop all tools
        if (CNetwork.IsServer)
        {
            for (byte i = 0; i < k_bMaxNumTools; ++i)
            {
                DropTool(i);
                /*
                if (m_acToolsViewId[i].Get() != null)
                {
                    GetTool(i).GetComponent<CToolInterface>().NotifyDropped();
                }
                */
            }
        }

        // Owner player unsubscribe from events
        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerInteractor>().EventUse -= OnEventInteractionUse;

            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_Reload, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_Drop, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot1, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot2, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot3, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Tool_EquipToolSlot4, OnEventInput);
        }

        gameObject.GetComponent<CNetworkView>().EventPreDestory -= OnEventPreDestroy;
    }


    void Update()
    {
        UpdateToolSwitching();
    }


    void UpdateToolSwitching()
    {
        // Check we are currently unequping a tool
        if (m_eSwitchToolState == ESwitchToolState.UnequipingTool ||
            m_eSwitchToolState == ESwitchToolState.UnequipingToolOnly)
        {
            // Increment timer
            m_fUnequipToolTimer += Time.deltaTime;

            // Update position lerp - For testing only
            GetTool(m_bActiveToolId.GetPrevious()).transform.localPosition = Vector3.Lerp(m_vToolEquipedPosition, m_vToolUnequipedPosition, m_fUnequipToolTimer / m_fUnequipToolDuration);

            // Check uneuiping has finished
            if (m_fUnequipToolTimer >= m_fUnequipToolDuration)
            {
                if (m_eSwitchToolState == ESwitchToolState.UnequipingTool)
                {
                    // Change state to equping tool
                    SetSwitchingToolState(ESwitchToolState.EquipingTool);
                }
                else
                {
                    // Change state to equping tool
                    SetSwitchingToolState(ESwitchToolState.INVALID);
                }
            }
        }

        // Check we are currently equiping a tool
        else if (m_eSwitchToolState == ESwitchToolState.EquipingTool)
        {
            // Increment timer
            m_fEquipToolTimer += Time.deltaTime;

            // Update position lerp - For testing only
            ActiveTool.transform.localPosition = Vector3.Lerp(m_vToolUnequipedPosition, m_vToolEquipedPosition, m_fEquipToolTimer / m_fEquipToolDuration);

            // Check euiping has finished
            if (m_fEquipToolTimer >= m_fEquipToolDuration)
            {
                // Change state to finished
                SetSwitchingToolState(ESwitchToolState.INVALID);
            }
        }
    }


    [ALocalOnly]
    void SwitchTool(byte _bSlotId)
    {
        // Check tool exists in that slot
        if (GetTool(_bSlotId) != null)
        {
            s_cSerializeStream.Write(ENetworkAction.EquipTool);
            s_cSerializeStream.Write(_bSlotId);
        }
    }


    void SetSwitchingToolState(ESwitchToolState _cNewState)
    {
        switch (_cNewState)
        {
            case ESwitchToolState.UnequipingTool:
            case ESwitchToolState.UnequipingToolOnly:
                {
                    m_fUnequipToolTimer = 0.0f;
                }
                break;

            case ESwitchToolState.EquipingTool:
                {
                    m_fEquipToolTimer = 0.0f;

                    // Check previous active tool existed
                    if (m_bActiveToolId.GetPrevious() != k_bNoActiveToolId)
                    {
                        // Turn off renderer for unequiped tool - For testing only
                        foreach (Renderer cRenderer in GetTool(m_bActiveToolId.GetPrevious()).transform.GetComponentsInChildren<Renderer>())
                        {
                            cRenderer.enabled = false;
                        }
                    }

                    // Turn on renderer for equiping tool - For testing only
                    foreach (Renderer cRenderer in GetTool(m_bActiveToolId.Get()).transform.GetComponentsInChildren<Renderer>())
                    {
                        cRenderer.enabled = true;
                    }
                }
                break;

            case ESwitchToolState.INVALID:
                {
                    // Check the player was unequiping tool only
                    if (m_eSwitchToolState == ESwitchToolState.UnequipingToolOnly)
                    {
                        // Check previous active tool existed
                        if (m_bActiveToolId.GetPrevious() != k_bNoActiveToolId)
                        {
                            // Turn off renderer for unequiped tool - For testing only
                            foreach (Renderer cRenderer in GetTool(m_bActiveToolId.GetPrevious()).transform.GetComponentsInChildren<Renderer>())
                            {
                                cRenderer.enabled = false;
                            }
                        }
                    }

                    // Run owner player specific functionality
                    if (gameObject == CGamePlayers.SelfActor)
                    {
                        // Set equiped
                        ActiveTool.GetComponent<CToolInterface>().SetEquipped(true);

                        // Activate primary if the player is holding down their primary
                        if (CUserInput.IsInputDown(CUserInput.EInput.Primary))
                        {
                            ActiveTool.GetComponent<CToolInterface>().SetPrimaryActive(true);
                        }

                        // Activate secondary if the player is holding down their secondary
                        if (CUserInput.IsInputDown(CUserInput.EInput.Secondary))
                        {
                            ActiveTool.GetComponent<CToolInterface>().SetSecondaryActive(true);
                        }
                    }
                }
                break;

            default:
                Debug.LogError("Unknown switch tool state: " + _cNewState);
                break;
        }

        Debug.LogError("Switch Tool State: " + _cNewState);

        m_eSwitchToolState = _cNewState;
    }


    void OnGUI()
    {
        if (gameObject == CGamePlayers.SelfActor)
        {
            string sToolText = "";

            for (uint i = 0; i < m_bToolCapacity.Get(); ++i)
            {
                sToolText += "Slot "+ i +": ";

                if (GetToolViewId(i) != null)
                {
                    sToolText += GetTool(i).name;

                    if (ActiveSlotId == i)
                    {
                        sToolText += " - ACTIVE";
                    }
                }
                else
                {
                    sToolText += "None";
                }

                sToolText += "\n";
            }

            GUI.Box(new Rect(10, Screen.height - 90, 240, 56),
                    "[Belt]\n" +
                    sToolText);
        }
    }


    [ALocalOnly]
    void OnEventInteractionUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
    {
        // Check use input was down during this interaction
        if (_bDown)
        {
            // Check object is a tool
            if (_cActorInteractable.GetComponent<CToolInterface>() != null)
            {
                s_cSerializeStream.Write(ENetworkAction.PickupTool);
                s_cSerializeStream.Write(_cActorInteractable.GetComponent<CNetworkView>().ViewId);
            }
        }
    }


    [ALocalOnly]
    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        switch (_eInput)
        {
            case CUserInput.EInput.Primary:
                {
                    // Check has active tool
                    // Check tool is equiped (That we are not in a switch tool transition)
                    if (ActiveTool != null &&
                        ActiveTool.GetComponent<CToolInterface>().IsEquiped)
                    {
                        ActiveTool.GetComponent<CToolInterface>().SetPrimaryActive(_bDown);
                    }
                }
                break;

            case CUserInput.EInput.Secondary:
                {
                    // Check has active tool
                    // Check tool is equiped (That we are not in a switch tool transition)
                    if (ActiveTool != null &&
                        ActiveTool.GetComponent<CToolInterface>().IsEquiped)
                    {
                        ActiveTool.GetComponent<CToolInterface>().SetSecondaryActive(_bDown);
                    }
                }
                break;

            case CUserInput.EInput.Tool_Reload:
                {
                    if (_bDown &&
                        ActiveTool != null)
                    {
                        s_cSerializeStream.Write(ENetworkAction.ReloadTool);
                        s_cSerializeStream.Write(ActiveSlotId);
                    }
                }
                break;

            case CUserInput.EInput.Tool_Drop:
                {
                    if (_bDown &&
                        ActiveTool != null)
                    {
                        s_cSerializeStream.Write(ENetworkAction.DropTool);
                        s_cSerializeStream.Write(ActiveSlotId);
                    }
                }
                break;

            case CUserInput.EInput.Tool_EquipToolSlot1:
                if (_bDown) SwitchTool(0);
                break;

            case CUserInput.EInput.Tool_EquipToolSlot2:
                if (_bDown) SwitchTool(1);
                break;

            case CUserInput.EInput.Tool_EquipToolSlot3:
                if (_bDown) SwitchTool(2);
                break;

            case CUserInput.EInput.Tool_EquipToolSlot4:
                if (_bDown) SwitchTool(3);
                break;

            default:
                Debug.LogError("Unknown error");
                break;
        }
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        // Picking up and dropping tool
        for (byte i = 0; i < k_bMaxNumTools; ++i)
        {
            if (m_acToolsViewId[i] == _cSyncedVar)
            {
                HandleVarSyncToolsViewId(i);
            }
        }

        // Changing tool
        if (_cSyncedVar == m_bActiveToolId)
        {
            HandleVarSyncActiveToolId();
        }
    }


    void HandleVarSyncToolsViewId(byte _bSlotId)
    {
        CNetworkVar<CNetworkViewId> cToolViewId = m_acToolsViewId[_bSlotId];

        // Check tool was removed from slot
        if (cToolViewId.Get() == null)
        {
            // Notify observers
            if (EventToolDropped != null) EventToolDropped(cToolViewId.GetPrevious().GameObject);
        }

        // Check tool was added to slot
        else
        {
            // Notify observers
            if (EventToolPickedup != null) EventToolPickedup(cToolViewId.GetPrevious().GameObject);
        }
    }


    void HandleVarSyncActiveToolId()
    {
        // Check new active tool id is valid
        if (m_bActiveToolId.Get() != k_bNoActiveToolId)
        {
            // Check that player is switching from another tool
            if (m_bActiveToolId.GetPrevious() != k_bNoActiveToolId)
            {
                SetSwitchingToolState(ESwitchToolState.UnequipingTool);
            }

            // Player has either picked up a new tool while not holding a tool
            // Or player is switching to a tool from not having a active tool id set
            else
            {
                SetSwitchingToolState(ESwitchToolState.EquipingTool);
            }
        }
        else
        {
            SetSwitchingToolState(ESwitchToolState.UnequipingToolOnly);
        }


        /*
        if (m_bActiveToolId.GetPrevious() == k_bNoActiveToolId ||
            GetTool(m_bActiveToolId.GetPrevious()) == null)
        {
            

            // Set state to equiping tool
            SetSwitchingToolState(ESwitchToolState.EquipingTool);
        }

        // Player 
        else
        {
            // Owner functionality
            if (gameObject == CGamePlayers.SelfActor)
            {
                // Unequip tool
                GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().SetEquipped(false);

                // Turn off primary
                if (GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().IsPrimaryActive)
                {
                    GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().SetPrimaryActive(false);
                }

                // Turn off secondary
                if (GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().IsSeconaryActive)
                {
                    GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().SetSecondaryActive(false);
                }
            }
        }

        // Check previous active tool is 


        // 
        if (m_eSwitchToolState == ESwitchToolState.EquipingTool)
        {
            m_eSwitchToolState = ESwitchToolState.UnequipingTool;
            
            // Start unequip timer where the equip timer left off
            m_fUnequipToolTimer = m_fEquipToolTimer;

            m_fEquipToolTimer = 0.0f;
        }
        else if (m_eSwitchToolState == ESwitchToolState.UnequipingTool)
        {
            // Do nothing
        }
        else
        {
            m_eSwitchToolState = ESwitchToolState.UnequipingTool;

            m_fUnequipToolTimer = 0.0f;
            m_fEquipToolTimer = 0.0f;
        }
            */

        //Debug.LogError("Set start state to unequiping tool");

        if (EventEquipedToolChanged != null) EventEquipedToolChanged(ActiveTool);
    }


// Member Fields


    const byte k_bMaxNumTools = 4;
    const byte k_bNoActiveToolId = k_bMaxNumTools;


	CNetworkVar<CNetworkViewId>[] m_acToolsViewId = null;
    CNetworkVar<EState> m_eState      = null;
	CNetworkVar<byte> m_bToolCapacity = null;
	CNetworkVar<byte> m_bActiveToolId = null;


    Vector3 m_vToolEquipedPosition;
    Vector3 m_vToolUnequipedPosition;


    [AServerOnly]
    ulong m_ulOwnerPlayerId = 0;


    ESwitchToolState m_eSwitchToolState = ESwitchToolState.INVALID;


    float m_fUnequipToolTimer = 0.0f;
    float m_fUnequipToolDuration = 0.5f;
    float m_fEquipToolTimer = 0.0f;
    float m_fEquipToolDuration = 0.5f;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
