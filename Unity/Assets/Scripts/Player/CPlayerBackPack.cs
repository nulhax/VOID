//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerBackPack.cs
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


public class CPlayerBackPack : CNetworkMonoBehaviour
{

// Member Types


	public enum ENetworkAction
	{
		PickupModule,
		DropModule,
		InsertCell
	}


// Member Delegates & Events


// Member Properties


	public GameObject ModuleObject
	{
		get
		{
			if (CarryingModuleViewId != 0)
			{
				return (CNetwork.Factory.FindObject(CarryingModuleViewId));
			}

			return (null);
		}
	}


	public ushort CarryingModuleViewId
	{
		get { return (m_usCarryingModuleViewId.Get()); }
	}


	public bool IsCarryingModule
	{
		get { return (CarryingModuleViewId != 0); }
	}


// Member Functions


	public override void InstanceNetworkVars()
	{
		m_usCarryingModuleViewId = new CNetworkVar<ushort>(OnNetworkVarSync);
	}


    [AServerOnly]
    public void PickupModule(ulong _ulPlayerId, ushort _usModuleViewId)
    {
        if (!IsCarryingModule)
        {
            m_usCarryingModuleViewId.Set(_usModuleViewId);
            CNetwork.Factory.FindObject(_usModuleViewId).GetComponent<CModuleInterface>().Pickup(_ulPlayerId);
        }
    }


    [AServerOnly]
    public void DropModule()
    {
        if (IsCarryingModule)
        {
            CNetwork.Factory.FindObject(CarryingModuleViewId).GetComponent<CModuleInterface>().Drop();
            m_usCarryingModuleViewId.Set(0);
        }
    }


    [AServerOnly]
    public void InsertCell(ulong _ulPlayerId, ushort _usCellSlotViewId)
    {
        if (IsCarryingModule)
        {
            ushort CellToInsert = m_usCarryingModuleViewId.Get();

            DropModule();

            ushort replacementCell = CNetwork.Factory.FindObject(_usCellSlotViewId).GetComponent<CCellSlot>().Insert(CellToInsert);

            if (replacementCell != 0)
            {
                PickupModule(_ulPlayerId, replacementCell);
            }
        }
    }


	[AClientOnly]
	public void OnPickupModuleRequest(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
	{
		if (_eType == CPlayerInteractor.EInteractionType.Use &&
			_cInteractableObject.GetComponent<CModuleInterface>() != null)
		{
			// Action
			s_cSerializeStream.Write((byte)ENetworkAction.PickupModule);

			// Target tool view id
			s_cSerializeStream.Write(_cInteractableObject.GetComponent<CNetworkView>().ViewId);
		}
	}
	
	[AClientOnly]
	public void OnCellInsertRequest(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
	{
		if (_eType == CPlayerInteractor.EInteractionType.PrimaryStart &&
			_cInteractableObject.GetComponent<CCellSlot>() != null &&
			IsCarryingModule)
		{
			CModuleInterface.EType carryingCellType = CNetwork.Factory.FindObject(CarryingModuleViewId).GetComponent<CModuleInterface>().m_eType;
			CModuleInterface.EType cellSlotType = _cInteractableObject.GetComponent<CCellSlot>().m_CellSlotType;
			
			if(carryingCellType == cellSlotType)
			{
				// Function to insert the cell here
				Debug.Log("Ima sliding my " + carryingCellType.ToString() + " into a " + cellSlotType.ToString() + " slot.");
				
				// Action
				s_cSerializeStream.Write((byte)ENetworkAction.InsertCell);

				// Target tool view id
				s_cSerializeStream.Write(_cInteractableObject.GetComponent<CNetworkView>().ViewId);
			}
		}
	}


	[AClientOnly]
    public static void SerializeOutbound(CNetworkStream _cStream)
    {
		// Drop
		if (Input.GetKeyDown(s_eDropKey))
		{
			_cStream.Write((byte)ENetworkAction.DropModule);
		}

		// Write in internal stream
		if (s_cSerializeStream.Size > 0)
		{
			_cStream.Write(s_cSerializeStream);
			s_cSerializeStream.Clear();
		}
	}


	[AServerOnly]
	public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerObject = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);
		CPlayerBackPack cPlayerBackPack = cPlayerObject.GetComponent<CPlayerBackPack>();

		// Process stream data
		while (_cStream.HasUnreadData)
		{
			// Extract action
			ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

			// Handle action
			switch (eAction)
			{
				case ENetworkAction.PickupModule:
					ushort usModuleViewId = _cStream.ReadUShort();
					cPlayerBackPack.PickupModule(_cNetworkPlayer.PlayerId, usModuleViewId);
					
					break;

				case ENetworkAction.DropModule:
					cPlayerBackPack.DropModule();
					break;
				
				case ENetworkAction.InsertCell:
					ushort usCellSlotViewId = _cStream.ReadUShort();
					cPlayerBackPack.InsertCell(_cNetworkPlayer.PlayerId, usCellSlotViewId);
					break;
			}
		}
	}


    void Start()
    {
        gameObject.GetComponent<CPlayerInteractor>().EventInteraction += new CPlayerInteractor.HandleInteraction(OnPickupModuleRequest);
        gameObject.GetComponent<CPlayerInteractor>().EventInteraction += new CPlayerInteractor.HandleInteraction(OnCellInsertRequest);
        gameObject.GetComponent<CNetworkView>().EventPreDestory += new CNetworkView.NotiftyPreDestory(OnPreDestroy);
    }


    void OnPreDestroy()
    {
        if (CNetwork.IsServer)
        {
            if (IsCarryingModule)
            {
                DropModule();
            }
        }
    }


    void Update()
    {
        // Empty
    }


    void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
    {
        if (_cSyncedNetworkVar == m_usCarryingModuleViewId)
        {
            if (m_usCarryingModuleViewId.Get() == 0)
            {

            }
        }
    }


    void OnGUI()
    {
        if (gameObject == CGame.PlayerActor)
        {
            string sModuleText = "[Module] ";

            if (m_usCarryingModuleViewId.Get() != 0)
            {
                sModuleText += ModuleObject.name;
            }
            else
            {
                sModuleText += "None";
            }


            GUI.Label(new Rect(10, Screen.height - 80, 500, 50), sModuleText);
        }
    }


// Member Fields


	CNetworkVar<ushort> m_usCarryingModuleViewId = null;


	static KeyCode s_eDropKey = KeyCode.H;
	static CNetworkStream s_cSerializeStream = new CNetworkStream();


};
