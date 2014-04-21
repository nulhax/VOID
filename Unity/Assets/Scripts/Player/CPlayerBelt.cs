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
		SwitchTool,
		ReloadTool,
        DropTool,

        MAX
    }


    public enum ESwitchToolState
    {
        INVALID,

        UnequippingTool,
        EquippingTool,

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


    public byte ActiveToolId
    {
        get { return (m_bActiveToolId.Get()); }
    }


    public byte PreviousActiveToolId
    {
        get { return (m_bActiveToolId.GetPrevious()); }
    }


	public GameObject ActiveTool
	{
		get 
		{
			if (ActiveToolId == k_bInvalidToolId)
			{
                return (null);
			}

            return (GetTool(ActiveToolId));
		}
	}


    public GameObject PreviousActiveTool
    {
        get
        {
            if (PreviousActiveToolId == k_bInvalidToolId ||
                GetTool(PreviousActiveToolId) == null)
            {
                return (null);
            }

            return (GetTool(PreviousActiveToolId));
        }
    }


	public byte ToolCapacity
	{
		get { return (m_bToolCapacity.Get()); }
	}
			
	
// Member Functions


    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        _cRegistrar.RegisterRpc(this, "RemoteNotifySwitchingTool");

		m_acToolsViewId = new CNetworkVar<TNetworkViewId>[k_bMaxNumTools];

		for (uint i = 0; i < k_bMaxNumTools; ++i)
		{
			m_acToolsViewId[i] = _cRegistrar.CreateReliableNetworkVar<TNetworkViewId>(OnNetworkVarSync);
		}

		m_bToolCapacity = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, 3);
        m_bActiveToolId = _cRegistrar.CreateReliableNetworkVar<byte>(OnNetworkVarSync, k_bInvalidToolId);
    }


	[AServerOnly]
	public void PickupTool(GameObject _cTool)
	{
        // Check object is a tool
        if (_cTool.GetComponent<CToolInterface>() == null)
        {
            Debug.LogError(string.Format("Cannot pick up tool. Object does not have a CToolInterface component. GameObject({0})", _cTool));
        }
		else
		{
			// Find free slot
			for (byte i = 0; i < ToolCapacity; ++i)
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

                        // Set tool to owned
                        cToolInterface.SetOwner(m_ulOwnerPlayerId);

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
        if (_bSlotId != k_bMaxNumTools &&
            (_bSlotId < 0 ||
             _bSlotId >= k_bMaxNumTools))
        {
            Debug.LogError(string.Format("Invalid tool slot id. SlotId({0})", _bSlotId));
        }
        else
		{
            //Debug.LogError("Chaning tool slot id to: " + _bSlotId);

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
        bool bNewToolFound = false;

		// Check tool id is valid and exists
        if (_bSlotId != k_bInvalidToolId &&
            GetToolViewId(_bSlotId) != null)
        {
            //Debug.LogError("Dropping tool: " + _bSlotId);

            // Notify tool about being dropped
            GetTool(_bSlotId).GetComponent<CToolInterface>().SetDropped();

            // Remove tool from slot
            m_acToolsViewId[_bSlotId].Set(null);

            // Change tool to next available tool
            for (int i = (int)k_bMaxNumTools - 1; i >= 0; --i)
            {
                if (m_acToolsViewId[i].Get() != null)
                {
                    ChangeTool((byte)i);
                    bNewToolFound = true;
                    break;
                }
            }
        }

        if (!bNewToolFound)
        {
            ChangeTool(k_bInvalidToolId);
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


	public TNetworkViewId GetToolViewId(uint _bSlotId)
	{
        if (_bSlotId == k_bInvalidToolId)
        {
            return (null);
        }

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
                        TNetworkViewId cToolViewId = _cStream.Read<TNetworkViewId>();

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

                case ENetworkAction.SwitchTool:
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
			m_vInitialToolEquipedPosition = m_vToolEquipedPosition;

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

		if (ActiveTool != null) 
		{
			//Get variables from current tool's orientation script
			CToolOrientation cToolOrientation = ActiveTool.GetComponent<CToolOrientation>();
			m_vToolEquipedPosition = cToolOrientation.Position;
			m_vInitialToolEquipedPosition = m_vToolEquipedPosition;

			m_fLateralDeviation = cToolOrientation.LateralDeviation;
			m_fVerticalDeviation = cToolOrientation.VerticalDeviation;

			UpdateVerticalToolPositioning ();
			UpdateLateralToolPositioning ();
		}
    }


    void UpdateToolSwitching()
    {
        // Check we are currently unequping a tool
        if (m_eSwitchToolState == ESwitchToolState.UnequippingTool)
        {
            // Increment timer
            m_fUnequipToolTimer += Time.deltaTime;

            // Update position lerp - For testing only
            PreviousActiveTool.transform.localPosition = Vector3.Lerp(m_vToolEquipedPosition, m_vToolUnequipedPosition, m_fUnequipToolTimer / m_fUnequipToolDuration);

            // Check uneuiping has finished
            if (m_fUnequipToolTimer >= m_fUnequipToolDuration)
            {
                // Check current tool is valid
                if (ActiveToolId != k_bInvalidToolId)
                {
                    // Change state to equping tool
                    SetSwitchingToolState(ESwitchToolState.EquippingTool);
                }
                else
                {
                    // Change state to equping tool
                    SetSwitchingToolState(ESwitchToolState.INVALID);
                }
            }
        }

        // Check we are currently equiping a tool
        else if (m_eSwitchToolState == ESwitchToolState.EquippingTool)
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

	void UpdateVerticalToolPositioning()
	{
		float cameraPitch = CGameCameras.MainCamera.transform.rotation.eulerAngles.x;
		if(cameraPitch > 180 && cameraPitch < 360)
		{
			cameraPitch -= 360;
		}
		float minRotation = -m_fVerticalRotationThreshold;
		float maxRotation = m_fVerticalRotationThreshold;

		float scale = maxRotation - minRotation;
		cameraPitch += scale / 2;
		float lerpFactor = cameraPitch / (scale);

		Vector3 maxPositionY = m_vInitialToolEquipedPosition;
		maxPositionY.y -= m_fVerticalDeviation;

		Vector3 minPositionY = m_vInitialToolEquipedPosition;
		minPositionY.y += m_fVerticalDeviation;

		Vector3 newOffset =  Vector3.Lerp(minPositionY, maxPositionY, lerpFactor);
		m_vToolEquipedPosition.y = newOffset.y;
		ActiveTool.transform.localPosition = m_vToolEquipedPosition;
	}

	void UpdateLateralToolPositioning()
	{
		float cameraYaw = CGameCameras.MainCamera.transform.rotation.eulerAngles.y;
		float characterYaw = transform.rotation.eulerAngles.y;

		float offSet = cameraYaw - characterYaw;
		if(offSet > 180 && offSet < 360)
		{
			offSet -= 360;
		}
		else if(offSet < -180)
		{
			offSet += 360;
		}

		float minRotation = -m_fLateralRotationThreshold;
		float maxRotation = m_fLateralRotationThreshold;

		float scale = maxRotation - minRotation;
		offSet += scale / 2;
		float lerpFactor = offSet / (scale);

		Vector3 maxPositionX = m_vInitialToolEquipedPosition;
		maxPositionX.x += m_fLateralDeviation;
		
		Vector3 minPositionX = m_vInitialToolEquipedPosition;
		minPositionX.x -= m_fLateralDeviation;

		Vector3 newOffset =  Vector3.Lerp(minPositionX, maxPositionX, lerpFactor);
		m_vToolEquipedPosition.x = newOffset.x;
		ActiveTool.transform.localPosition = m_vToolEquipedPosition;
	}


    [ALocalOnly]
    void SwitchTool(byte _bSlotId)
    {
        // Check tool exists in that slot
        if (GetTool(_bSlotId) != null)
        {
            // Set unequiped
            ActiveTool.GetComponent<CToolInterface>().SetEquipped(false);

            s_cSerializeStream.Write(ENetworkAction.SwitchTool);
            s_cSerializeStream.Write(_bSlotId);
        }
    }


    void SetSwitchingToolState(ESwitchToolState _cNewState)
    {
        switch (_cNewState)
        {
            case ESwitchToolState.UnequippingTool:
                {
                    bool bStartFresh = true;

                    // Check current state is equipping tool
                    if (m_eSwitchToolState == ESwitchToolState.EquippingTool)
                    {
                        // Get current ratio into equpping tool
                        float fEquipTimerRatio = m_fEquipToolTimer / m_fEquipToolDuration;

                        // Check equip did not finish
                        if (fEquipTimerRatio < 1.0f)
                        {
                            // Set unequip timer relative to equip timer
                            m_fUnequipToolTimer = m_fUnequipToolDuration - (m_fUnequipToolTimer * fEquipTimerRatio);

                            // Stop starting fresh
                            bStartFresh = false;
                        }
                    }

                    // Start fresh
                    if (bStartFresh)
                    {
                        m_fUnequipToolTimer = 0.0f;

                        //Debug.LogError("Starting unequip fresh");
                    }

                    m_bUnequipingToolId = PreviousActiveToolId;
                }
                break;

            case ESwitchToolState.EquippingTool:
                {
                    m_bUnequipingToolId = k_bInvalidToolId;

                    // Turn on renderer for equiping tool - For testing only
                    GetTool(m_bActiveToolId.Get()).GetComponent<CToolInterface>().SetVisible(true);

                    // Check previous active tool valid and still is held by me
                    if (PreviousActiveToolId != k_bInvalidToolId &&
                        PreviousActiveTool   != null)
                    {
                        // Turn off renderer for unequiped tool - For testing only
                        GetTool(m_bActiveToolId.GetPrevious()).GetComponent<CToolInterface>().SetVisible(false);
                    }

                    bool bStartFresh = true;

                    // Check current state is unsequipping tool
                    if (m_eSwitchToolState == ESwitchToolState.UnequippingTool)
                    {
                        // Get current ratio into unequpping tool
                        float fUnequipTimerRatio = m_fUnequipToolTimer / m_fUnequipToolDuration;

                        // Check unequip did not finish
                        if (fUnequipTimerRatio < 1.0f)
                        {
                            // Set equip timer relative to unequip timer
                            m_fEquipToolTimer = m_fEquipToolDuration - (m_fEquipToolTimer * fUnequipTimerRatio);

                            // Stop starting fresh
                            bStartFresh = false;
                        }

                    }

                    // Start fresh
                    if (bStartFresh)
                    {
                        m_fEquipToolTimer = 0.0f;

                        //Debug.LogError("Starting equip fresh");
                    }
                }
                break;

            case ESwitchToolState.INVALID:
                {
                    m_bUnequipingToolId = k_bInvalidToolId;

                    // Run owner player specific functionality
                    if (gameObject == CGamePlayers.SelfActor &&
                        ActiveTool != null)
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

        //Debug.LogError("Switch Tool State: " + _cNewState);

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

                    if (ActiveToolId == i)
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
                        s_cSerializeStream.Write(ActiveToolId);
                    }
                }
                break;

            case CUserInput.EInput.Tool_Drop:
                {
                    if (_bDown &&
                        ActiveTool != null)
                    {
                        s_cSerializeStream.Write(ENetworkAction.DropTool);
                        s_cSerializeStream.Write(ActiveToolId);
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
        CNetworkVar<TNetworkViewId> cToolViewId = m_acToolsViewId[_bSlotId];

        // Check tool was removed from slot
        if (cToolViewId.Get() == null)
        {
            cToolViewId.GetPrevious().GameObject.GetComponent<CToolInterface>().SetVisible(true);
            cToolViewId.GetPrevious().GameObject.transform.parent = null;

            // Notify observers
            if (EventToolDropped != null) EventToolDropped(cToolViewId.GetPrevious().GameObject);
        }

        // Check tool was added to slot
        else
        {
            // Hide newly pickedup tool
            cToolViewId.Get().GameObject.GetComponent<CToolInterface>().SetVisible(false);

            // Position tool at unequiped position (To prevent glitching when equiping)
            cToolViewId.Get().GameObject.transform.parent = GetComponent<CPlayerInterface>().Model.transform;
            cToolViewId.Get().GameObject.transform.localPosition = m_vToolUnequipedPosition;

            // Notify observers
            if (EventToolPickedup != null) EventToolPickedup(cToolViewId.GetPrevious().GameObject);
        }
    }


    void HandleVarSyncActiveToolId()
    {
        // Check new active tool id is invalid
        if (ActiveToolId == k_bInvalidToolId)
        {
            // Check previous active tool was valid and the tool is still being held
            if (PreviousActiveToolId != k_bInvalidToolId &&
                PreviousActiveTool   != null)
            {
                // Player is unequiping tool to nothing
                SetSwitchingToolState(ESwitchToolState.UnequippingTool);
            }
            else
            {
                // Turn off tool switching
                SetSwitchingToolState(ESwitchToolState.INVALID);
            }
        }

        // New active tool is valid
        else
        {
            // Check that player is switching from another tool
            // And that other tool is still held
            if (PreviousActiveToolId != k_bInvalidToolId &&
                PreviousActiveTool   != null)
            {
                // Check player is already unequiping a weapon
                // Check the tool they want to equip to the same tool that is being unequiped
                if (m_eSwitchToolState  == ESwitchToolState.UnequippingTool &&
                    m_bUnequipingToolId == ActiveToolId)
                {
                    SetSwitchingToolState(ESwitchToolState.EquippingTool);
                }
                else
                {
                    SetSwitchingToolState(ESwitchToolState.UnequippingTool);
                }
            }

            // Player has either picked up a new tool while not holding a tool
            // Or player is switching to a tool that was dropped
            else
            {
                SetSwitchingToolState(ESwitchToolState.EquippingTool);
            }
        }

        // Notify observers
        if (EventEquipedToolChanged != null) EventEquipedToolChanged(ActiveTool);
    }


// Member Fields


    const byte k_bMaxNumTools = 4;
    const byte k_bInvalidToolId = k_bMaxNumTools;


	CNetworkVar<TNetworkViewId>[] m_acToolsViewId = null;
	CNetworkVar<byte> m_bToolCapacity = null;
	CNetworkVar<byte> m_bActiveToolId = null;


	Vector3 m_vInitialToolEquipedPosition;
    Vector3 m_vToolEquipedPosition;
    Vector3 m_vToolUnequipedPosition;

	float m_fLateralDeviation = 0.3f;
	float m_fVerticalDeviation = 0.4f;

	float m_fLateralRotationThreshold = 60.0f;
	float m_fVerticalRotationThreshold = 45.0f;

    [AServerOnly]
    ulong m_ulOwnerPlayerId = 0;


    ESwitchToolState m_eSwitchToolState = ESwitchToolState.INVALID;


    float m_fUnequipToolTimer = 0.0f;
    float m_fUnequipToolDuration = 0.5f;
    float m_fEquipToolTimer = 0.0f;
    float m_fEquipToolDuration = 0.5f;


    byte m_bUnequipingToolId = k_bInvalidToolId;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();

};
