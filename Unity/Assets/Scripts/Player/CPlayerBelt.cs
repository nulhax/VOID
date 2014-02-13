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


    const uint k_uiMaxNumTools = 4;


    public enum ENetworkAction
    {
		ActivateToolPrimary,
		DeactivateToolPrimary,
		ActivateToolSecondary,
		DeactivateToolSeconary,
		PickupTool,
		UseTool,
		ChangeTool,
		ReloadActiveTool,
        DropActiveTool
    }


// Member Delegates & Events


	public delegate void EquipTool(GameObject _cEquippedTool);
	public event EquipTool EventEquipTool;


    [AClientOnly]
    public delegate void HandleToolChanged(CNetworkViewId _cViewId);
    public event HandleToolChanged EventToolChanged;


    [AClientOnly]
    public delegate void HandleToolDropped(CNetworkViewId _cViewId);
    public event HandleToolDropped EventToolDropped;


    [AClientOnly]
    public delegate void HandleToolPickedup(CNetworkViewId _cViewId);
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
		m_acToolsViewId = new CNetworkVar<CNetworkViewId>[k_uiMaxNumTools];

		for (uint i = 0; i < k_uiMaxNumTools; ++i)
		{
			m_acToolsViewId[i] = _cRegistrar.CreateNetworkVar<CNetworkViewId>(OnNetworkVarSync);
		}

		m_bToolCapacity = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync, 2);
		m_bActiveToolId = _cRegistrar.CreateNetworkVar<byte>(OnNetworkVarSync);
    }


	[AServerOnly]
	public void PickupTool(GameObject _cInteractableObject)
	{
		// Check object exists
		if (_cInteractableObject != null)
		{
			// Find free slot
			for (uint i = 0; i < ToolCapacity; ++i)
			{
				if (GetToolViewId(i) == null)
				{
					// Retrieve tool interface script
					CToolInterface cToolInterface = _cInteractableObject.GetComponent<CToolInterface>();
					CNetworkView cToolNetworkView = _cInteractableObject.GetComponent<CNetworkView>();

					// Check script found
					if (cToolInterface == null)
					{
						Debug.LogError(string.Format("Target tool does not have the CToolInterface component attached! ObjectName({0})", _cInteractableObject.name));
					}
                    else if (cToolInterface.IsHeld)
                    {
                        break;
                    }
					else
					{
						m_acToolsViewId[i].Set(cToolNetworkView.ViewId);
                        cToolInterface.NotifyPickedUp(m_ulOwnerPlayerId);
						ChangeTool((byte)i);
                        // Commented out by Nathan to avoid extraneous debug information.
                        // Feel free to uncomment for debugging purposes when required.
						//Debug.Log(string.Format("Picked up tool. PlayerId({0}) ToolObjectName({1}) SlotId({2})", _ulPlayerId, _cInteractableObject.name, i));
					}
					break;
				}
			}
		}
	}


	[AServerOnly]
	public void ChangeTool(byte _bSlotId)
	{
		// Check tool exists
		if (_bSlotId >= 0 &&
            _bSlotId < k_uiMaxNumTools &&
            GetToolViewId(_bSlotId) != null)
		{
			m_bActiveToolId.Set(_bSlotId);

            // Commented out by Nathan to avoid extraneous debug information.
            // Feel free to uncomment for debugging purposes when required.
            //Debug.Log(string.Format("Changing tool to SlotId({0})", _bSlotId));
		}
	}


	[AServerOnly]
	public void ReloadTool(byte _bSlotId)
	{
		// Check tool exists
		if (GetToolViewId(_bSlotId) != null)
		{
			GetTool(_bSlotId).GetComponent<CToolInterface>().Reload();
			Debug.Log("Reloading active tool");
		}
	}


	[AServerOnly]
	public void DropTool(byte _bSlotId)
	{
		// Check tool exists
		if (GetToolViewId(_bSlotId) != null)
		{
			GetTool(_bSlotId).GetComponent<CToolInterface>().NotifyDropped();
            m_acToolsViewId[_bSlotId].Set(null);

            // Change tool to next available tool
            for (int i = (int)k_uiMaxNumTools - 1; i >= 0; --i)
            {
                if (m_acToolsViewId[i].Get() != null)
                {
                    ChangeTool((byte)i);
                    break;
                }
            }

            //Debug.Log("Dropping active tool");
		}
	}


	public GameObject GetTool(uint _bSlotId)
	{
		return (CNetwork.Factory.FindObject(GetToolViewId(_bSlotId)));
	}


	public CNetworkViewId GetToolViewId(uint _bSlotId)
	{
		return (m_acToolsViewId[_bSlotId].Get());
	}


    [AClientOnly]
    public static void SerializeBeltState(CNetworkStream _cStream)
    {
        // Write in internal stream
        _cStream.Write(s_cSerializeStream);
        s_cSerializeStream.Clear();
    }


    [AServerOnly]
    public static void UnserializeBeltState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();
            CPlayerBelt cPlayerBelt = CGamePlayers.GetPlayerActor(_cNetworkPlayer.PlayerId).GetComponent<CPlayerBelt>();

            switch (eAction)
            {
                case ENetworkAction.PickupTool:
                    {
                        CNetworkViewId cToolViewId = _cStream.ReadNetworkViewId();
                        cPlayerBelt.PickupTool(cToolViewId.GameObject);
                    }
                    break;
            }
        }
    }


    void Start()
    {
        m_ulOwnerPlayerId = CGamePlayers.GetPlayerActorsPlayerId(ThisNetworkView.ViewId);

        if (CNetwork.IsServer)
        {
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_Reload, OnEventClientInput);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_Drop, OnEventClientInput);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot1, OnEventClientInput);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot2, OnEventClientInput);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot3, OnEventClientInput);
            CUserInput.SubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot4, OnEventClientInput);
        }

        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerInteractor>().EventUse += OnEventInteractionUse;

            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
        }

        gameObject.GetComponent<CNetworkView>().EventPreDestory += new CNetworkView.NotiftyPreDestory(OnPreDestroy);
    }


	void OnDestroy()
	{
        if (CNetwork.IsServer)
        {
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_Reload, OnEventClientInput);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_Drop, OnEventClientInput);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot1, OnEventClientInput);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot2, OnEventClientInput);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot3, OnEventClientInput);
            CUserInput.UnsubscribeClientInputChange(CUserInput.EInput.Tool_SelectSlot4, OnEventClientInput);
        }

        if (gameObject == CGamePlayers.SelfActor)
        {
            gameObject.GetComponent<CPlayerInteractor>().EventUse -= OnEventInteractionUse;

            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnEventInput);
            CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnEventInput);
        }

        gameObject.GetComponent<CNetworkView>().EventPreDestory -= OnPreDestroy;
	}


    void Update()
    {
        // Empty
    }


    void OnNetworkVarSync(INetworkVar _cSyncedVar)
    {
        for (uint i = 0; i < k_uiMaxNumTools; ++i)
        {
            if (m_acToolsViewId[i] == _cSyncedVar)
            {
                CNetworkViewId cToolNetworkViewId = m_acToolsViewId[i].Get();

                if (cToolNetworkViewId == null)
                {
                    if (EventToolDropped != null) EventToolDropped(m_acToolsViewId[i].GetPrevious());
                }
                else
                {
                    if (EventToolPickedup != null) EventToolPickedup(cToolNetworkViewId);
                }
            }
        }

        // Changing tool
        if (_cSyncedVar == m_bActiveToolId)
        {
            if (EventToolChanged != null) EventToolChanged(m_acToolsViewId[m_bActiveToolId.Get()].Get());
        }
    }


    void OnPreDestroy()
    {
        if (CNetwork.IsServer)
        {
            for (uint i = 0; i < k_uiMaxNumTools; ++i)
            {
                if (m_acToolsViewId[i].Get() != null)
                {
                    GetTool(i).GetComponent<CToolInterface>().NotifyDropped();
                }
            }
        }
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


    void OnEventInteractionUse(CPlayerInteractor.EInputInteractionType _eInteractionType, GameObject _cActorInteractable, RaycastHit _cRaycastHit, bool _bDown)
    {
        if (_bDown)
        {
            if (_cActorInteractable.GetComponent<CToolInterface>() != null)
            {
                s_cSerializeStream.Write((byte)ENetworkAction.PickupTool);
                s_cSerializeStream.Write(_cActorInteractable.GetComponent<CNetworkView>().ViewId);
            }
        }
    }


    [AClientOnly]
    void OnEventInput(CUserInput.EInput _eInput, bool _bDown)
    {
        switch (_eInput)
        {
            case CUserInput.EInput.Primary:
                {
                    if (ActiveTool != null &&
                        ActiveTool.GetComponent<CToolInterface>() != null)
                    {
                        ActiveTool.GetComponent<CToolInterface>().SetPrimaryActive(_bDown);
                    }
                }
                break;

            case CUserInput.EInput.Secondary:
                {
                    if (ActiveTool != null &&
                        ActiveTool.GetComponent<CToolInterface>() != null)
                    {
                        ActiveTool.GetComponent<CToolInterface>().SetSecondaryActive(_bDown);
                    }
                }
                break;

            default:
                Debug.LogError("Unknown error");
                break;
        }
    }


    [AClientOnly]
    void OnEventClientInput(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
    {
        if (_ulPlayerId == m_ulOwnerPlayerId &&
            _bDown)
        {
            switch (_eInput)
            {
                case CUserInput.EInput.Tool_SelectSlot1:
                    ChangeTool(0);
                    if(EventEquipTool != null) EventEquipTool(GetTool(0));
                    break;

                case CUserInput.EInput.Tool_SelectSlot2:
                    ChangeTool(1);
                    if(EventEquipTool != null) EventEquipTool(GetTool(1));
                    break;

                case CUserInput.EInput.Tool_SelectSlot3:
                    ChangeTool(2);
                    if(EventEquipTool != null) EventEquipTool(GetTool(2));
                    break;

                case CUserInput.EInput.Tool_SelectSlot4:
                    ChangeTool(3);
                    if(EventEquipTool != null) EventEquipTool(GetTool(3));
                    break;

                case CUserInput.EInput.Tool_Reload:
                    ReloadTool(ActiveSlotId);
                    break;

                case CUserInput.EInput.Tool_Drop:
                    DropTool(ActiveSlotId);
                    break;
            }
        }
    }


// Member Fields


	CNetworkVar<CNetworkViewId>[] m_acToolsViewId = null;
	CNetworkVar<byte> m_bToolCapacity = null;
	CNetworkVar<byte> m_bActiveToolId = null;


	static CNetworkStream s_cSerializeStream = new CNetworkStream();


// Server Member Fields


    ulong m_ulOwnerPlayerId = 0;


};
