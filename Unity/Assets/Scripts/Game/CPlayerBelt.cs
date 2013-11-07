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

//The player belt holds tools, and interacts with Tools on behalf of the Player.
//This Script contains:
//		an array which contains two to three tools.
//		an Id for the currently held/active tool
//		
//This Script Can:
//		Pick Up Tools
//		drop tools
//		get tool info
//		


public class CPlayerBelt : CNetworkMonoBehaviour
{

// Member Types
    const uint k_uiMaxTools = 3;

    public enum ENetworkAction
    {
        PickupTool,
        DropTool,
        ChangeTool,
        UseTool,
        ReloadTool
    }

// Member Delegates & Events
	
	
// Member Properties
	
	
// Member Functions

    public override void InstanceNetworkVars()
    {
    }


	public void Start()
	{
        m_uiActiveToolId = 0;

        CNetwork.Server.EventPlayerConnect += new CNetworkServer.NotifyPlayerConnect(NotifyPlayerConnect);
	}


	public void OnDestroy()
	{
	}


	public void Update()
	{

	}


    public void NotifyPlayerConnect(CNetworkPlayer _cPlayer)
    {

    }
	

    [ANetworkRpc]
	public void PickUpTool(byte _bToolSlotId, ushort _usToolViewId, ushort _usPlayerActorViewId)
	{
        GameObject Tool = CNetwork.Factory.FindObject(_usToolViewId);

        m_cTools[_bToolSlotId] = Tool;
        m_cTools[_bToolSlotId].GetComponent<CToolInterface>().SetPickedUp();

        Tool.transform.parent = gameObject.GetComponent<CPlayerHeadMotor>().ActorHead.transform;//gameObject.GetComponent<>().transform;
	}

    [ANetworkRpc]
    public void DropTool(byte _bToolId)
	{
        //if there is a tool in the slot, drop it, activating physics
        if (m_cTools[_bToolId] != null)
        {
            m_cTools[_bToolId].transform.parent = null;
            m_cTools[_bToolId].GetComponent<CToolInterface>().SetDropped();
            m_cTools[_bToolId] = null;
        }
	}
	

    [ANetworkRpc]
	public void ChangeTool(byte _uiToolId)
	{
        Vector3 ToolOffset = new Vector3(1, -1, 0);
        m_cTools[_uiToolId].transform.rotation = gameObject.GetComponent<CPlayerHeadMotor>().ActorHead.transform.rotation;
        m_cTools[_uiToolId].transform.position = gameObject.GetComponent<CPlayerHeadMotor>().ActorHead.transform.position + (transform.forward);
        m_cTools[_uiToolId].transform.localPosition = ToolOffset;

        m_uiActiveToolId = _uiToolId;
    }


    [ANetworkRpc]
    public void SetToolCapacity(uint _uiNewCapacity)
    {
        for (uint i = _uiNewCapacity; i < m_uiToolCapacity; i++)
        {
            DropTool((byte)i);
        }
        m_uiToolCapacity = _uiNewCapacity;
    }

    [ANetworkRpc]
    public void UseTool(byte _bToolId)
    {
        //if (m_cTools[_bToolId].GetComponent<CToolInterface>())

        m_cTools[_bToolId].GetComponent<CToolInterface>().SetPrimaryActive();

        //m_cTools[_bToolId].GetComponent<CToolInterface>().SetPrimaryActive(true);
    }

    public static void SerializeBeltState(CNetworkStream _cStream)
    {
        if (Input.GetKeyDown("f"))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit) && (hit.transform.gameObject.GetComponent<CToolInterface>() != null))
            {
                _cStream.Write((byte)ENetworkAction.PickupTool);
                _cStream.Write(hit.transform.gameObject.GetComponent<CNetworkView>().ViewId);
            }
        }
        else if(Input.GetKeyDown("g"))
        {
            _cStream.Write((byte)ENetworkAction.DropTool);
            _cStream.Write((uint)CGame.PlayerActor.GetComponent<CPlayerBelt>().m_uiActiveToolId);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            _cStream.Write((byte)ENetworkAction.UseTool);
            _cStream.Write((uint)CGame.PlayerActor.GetComponent<CPlayerBelt>().m_uiActiveToolId);
        }
    }


    public static void UnserializeBeltState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        ENetworkAction ToolAction = (ENetworkAction)_cStream.ReadByte();

        if (ToolAction == ENetworkAction.PickupTool)
        {
            ushort ToolViewId = _cStream.ReadUShort();

            GameObject Tool = CNetwork.Factory.FindObject(ToolViewId);

            if (Tool.GetComponent<CToolInterface>() != null)
            {
                if (Tool.GetComponent<CToolInterface>().IsHeldByPlayer == false)
                {
                    GameObject PlayerActor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);

                    CPlayerBelt PlayerAcotrsBelt = PlayerActor.GetComponent<CPlayerBelt>();

                    for (uint i = 0; i < PlayerAcotrsBelt.m_uiToolCapacity; i++)
                    {
                        if (PlayerAcotrsBelt.m_cTools[i] == null)
                        {
                            PlayerAcotrsBelt.InvokeRpcAll("PickUpTool", (byte)i, ToolViewId, PlayerActor.GetComponent<CNetworkView>().ViewId);
                            PlayerAcotrsBelt.InvokeRpcAll("ChangeTool", (byte)i);

                            break;
                        }
                    }
                }
            }
        }

        else if (ToolAction == ENetworkAction.DropTool)
        {
            ushort ToolId = _cStream.ReadUShort();

            GameObject PlayerActor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);

            PlayerActor.GetComponent<CPlayerBelt>().InvokeRpcAll("DropTool", (byte)ToolId);
            //PlayerAcotrsBelt.InvokeRpcAll("PickUpTool", (byte)i, ToolViewId, PlayerActor.GetComponent<CNetworkView>().ViewId);
        }
        else if (ToolAction == ENetworkAction.ChangeTool)
        {

        }
        else if (ToolAction == ENetworkAction.UseTool)
        {
            ushort ToolId = _cStream.ReadUShort();

            GameObject PlayerActor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);

            PlayerActor.GetComponent<CPlayerBelt>().InvokeRpcAll("UseTool", (byte)ToolId);
        }
        else if (ToolAction == ENetworkAction.ReloadTool)
        {

        }
        else
        {
            Debug.LogError("This should NOT BE HAPPENING");
        }
    }


    // Member Fields
    CNetworkVar<ushort> m_cToolsViewId = new CNetworkVar<ushort>[k_uiMaxTools];
    GameObject[] m_cTools = new GameObject[k_uiMaxTools];

    uint m_uiToolCapacity = 2;

	public uint m_uiActiveToolId;
};
