//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CPlayerInteractor : CNetworkMonoBehaviour
{
	// Member Types
	public enum EInteractionEvent
	{
		INVALID = -1,
		
		Nothing,
		LeftClick,
		RightClick,
		Action1,
		Action2,
		
		MAX
	}
	
	
	// Member Fields
	private EInteractionEvent m_CurrentInteraction = EInteractionEvent.Nothing;
	
	private ushort m_InteractableObjViewID = 0;
	
    static KeyCode m_eAction1Key = KeyCode.E;
    static KeyCode m_eAction2Key = KeyCode.F;
	
	// Member Properties
	
	// Member Methods
	public override void InstanceNetworkVars()
    {
		
	}
	
	public void Update()
	{
		// Return if this is not their player
		if(gameObject != CGame.PlayerActor)
			return;
		
		// Check if there is any interaction event active
		CheckInteractionEvents();
		
		// If the interaction event is not nothing check for interaction objects
		if(m_CurrentInteraction != EInteractionEvent.Nothing)
		{
			CheckInteractionObjects();
		}
	}
	
	private void CheckInteractionEvents()
	{
		// Find out if any of the interaction events are active
		if(Input.GetMouseButtonDown(0))
		{
			m_CurrentInteraction = EInteractionEvent.LeftClick;
		}
		else if(Input.GetMouseButtonDown(1))
		{
			m_CurrentInteraction = EInteractionEvent.RightClick;
		}
		else if(Input.GetKeyDown(m_eAction1Key))
		{
			m_CurrentInteraction = EInteractionEvent.Action1;
		}
		else if(Input.GetKeyDown(m_eAction2Key))
		{
			m_CurrentInteraction = EInteractionEvent.Action2;
		}
		else
		{
			return;
		}
	}
	
	private void CheckInteractionObjects()
	{
		// Find the origin, direction, distance of the players interaction cursor
		CPlayerHeadMotor playerHeadMotor = CGame.PlayerActor.GetComponent<CPlayerHeadMotor>();
		Vector3 orig = playerHeadMotor.ActorHead.transform.position;
		Vector3 direction = playerHeadMotor.ActorHead.transform.forward;
		float distance = 5.0f;
		RaycastHit hit = new RaycastHit();
		
		// Check if the player cast a ray on the screen
		if(CheckInteractableObjectRaycast(orig, direction, distance, out hit))
		{
			// Get the game object which owns this mesh
			GameObject IOHit = hit.collider.gameObject;
			
			// Check the parents untill we find the one that is not of layer InteractableObject
			int IOLayer = LayerMask.NameToLayer("InteractableObject");
			while(IOHit.transform.parent.gameObject.layer == IOLayer)
			{
				IOHit = IOHit.transform.parent.gameObject;
			}
			
			// Get the Interactable Object script from the object
			CInteractableObject IO = IOHit.GetComponent<CInteractableObject>();
			
			// If this is a valid IO
			if(IO != null)
			{
				// Get the network view id of the interactable object
				CNetworkView IONetworkView = IOHit.GetComponent<CNetworkView>();
				
				if(IONetworkView != null)
				{
					m_InteractableObjViewID = IONetworkView.ViewId;
				}
				else
				{
					Debug.LogError("CheckInteractionObjects. Something has gone wrong here... There was no CNetworkView component!");
				}
			}
			else
			{
				Debug.LogError("CheckInteractionObjects. Something has gone wrong here... There was no CInteractableObject component!");
			}
		}
	}
	
	private bool CheckInteractableObjectRaycast(Vector3 _origin, Vector3 _direction, float _fDistance, out RaycastHit _rh)
    {
		Ray ray = new Ray(_origin, _direction);
		
		if (Physics.Raycast(ray, out _rh, _fDistance, 1 << LayerMask.NameToLayer("InteractableObject")))
		{
			return(true);
		}
		
		return(false); 
    }
	
	public static void SerializePlayerState(CNetworkStream _cStream)
    {
		if(CGame.PlayerActorViewId != 0)
		{
			CPlayerInteractor actorInteractor = CGame.PlayerActor.GetComponent<CPlayerInteractor>();
			
			if(actorInteractor.m_CurrentInteraction != EInteractionEvent.Nothing && actorInteractor.m_InteractableObjViewID != 0)
			{
				_cStream.Write((int)actorInteractor.m_CurrentInteraction);
				_cStream.Write(actorInteractor.m_InteractableObjViewID);
			}
			
			// Reset the states
			actorInteractor.m_CurrentInteraction = EInteractionEvent.Nothing;
			actorInteractor.m_InteractableObjViewID = 0;
		}
    }

	public static void UnserializePlayerState(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
		GameObject playerActor = CGame.FindPlayerActor(_cNetworkPlayer.PlayerId);
		CPlayerInteractor actorInteractor = playerActor.GetComponent<CPlayerInteractor>();
		
		EInteractionEvent interactionEvent = (EInteractionEvent)_cStream.ReadInt();
		ushort interactableObjectViewID = _cStream.ReadUShort();
		
		// Get the interactable object
		GameObject obj = CNetwork.Factory.FindObject(interactableObjectViewID);
		
		// If the event is registered then let all of the clients know
		CInteractableObject interactableObject = obj.GetComponent<CInteractableObject>();
		if(interactableObject.IsInteractionEventRegistered(interactionEvent))
		{
			// Invoke the rpc to activate the event across all clients
			interactableObject.InvokeRpcAll("OnInteractionEvent", interactionEvent, playerActor.GetComponent<CNetworkView>().ViewId);
		}
    }
}
