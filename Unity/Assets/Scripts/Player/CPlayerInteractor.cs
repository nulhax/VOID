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


	public enum EInteractionType
	{
		INVALID = -1,
		
		Nothing,
		Primary,
		Secondary,
		Use,
		Action2,
		
		MAX
	}


// Member Delegates & Events


    public delegate void HandleInteraction(EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit);
    public event HandleInteraction EventInteraction;
	
	
// Member Fields


	private EInteractionType m_CurrentInteraction = EInteractionType.Nothing;
	

    static KeyCode m_eUseKey = KeyCode.E;
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
		if(m_CurrentInteraction != EInteractionType.Nothing)
		{
			CheckInteractionObjects();
		}
	}

	
	private void CheckInteractionEvents()
	{
		// Reset the interaction event
		m_CurrentInteraction = EInteractionType.Nothing;
		
		// Find out if any of the interaction events are active
		if(Input.GetMouseButtonDown(0))
		{
			m_CurrentInteraction = EInteractionType.Primary;
		}
		else if(Input.GetMouseButtonDown(1))
		{
			m_CurrentInteraction = EInteractionType.Secondary;
		}
		else if(Input.GetKeyDown(m_eUseKey))
		{
			m_CurrentInteraction = EInteractionType.Use;
		}
		else if(Input.GetKeyDown(m_eAction2Key))
		{
			m_CurrentInteraction = EInteractionType.Action2;
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
			for(int i = 0; i < 100; ++i)
			{
				if(i == 99)
				{
					Debug.LogError("CheckInteractionObjects Couldn't find the interactableobjetcs parent!");
				}
                if (IOHit.transform.parent != null &&
                    IOHit.transform.parent.gameObject.layer == IOLayer)
				{
					IOHit = IOHit.transform.parent.gameObject;
				}
				else
				{
					break;
				}
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
                    if (EventInteraction != null)
                    {
                        EventInteraction(m_CurrentInteraction, IOHit, hit);
                    }


					IO.OnInteractionEvent(m_CurrentInteraction, gameObject, hit);
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
	

	private static bool CheckInteractableObjectRaycast(Vector3 _origin, Vector3 _direction, float _fDistance, out RaycastHit _rh)
    {
		Ray ray = new Ray(_origin, _direction);
		
		if (Physics.Raycast(ray, out _rh, _fDistance, 1 << LayerMask.NameToLayer("InteractableObject")))
		{
			return(true);
		}
		
		return(false); 
    }


}
