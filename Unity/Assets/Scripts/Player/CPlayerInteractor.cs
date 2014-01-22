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
		
		Hover,
		PrimaryStart,
		PrimaryEnd,
		SecondaryStart,
		SecondaryEnd,
		Use,
		
		MAX
	}


// Member Delegates & Events


    public delegate void HandleInteraction(EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit);
    public event HandleInteraction EventInteraction;

	public delegate void HandleNoInteraction(EInteractionType _eType, RaycastHit _cRayHit);
	public event HandleNoInteraction EventNoInteraction;
	

// Member Properties

	
// Member Methods


	public override void InstanceNetworkVars()
    {
		// Empty
	}


	public void Start()
	{
		if (gameObject == CGamePlayers.SelfActor)
		{
			CUserInput.EventPrimary += OnInputPrimaryChange;
			CUserInput.EventSecondary += OnInputSecondaryChange;
			CUserInput.EventUse += OnInputUseChange;
		}
	}


	void OnDestroy()
	{
		CUserInput.EventPrimary -= OnInputPrimaryChange;
		CUserInput.EventSecondary -= OnInputSecondaryChange;
		CUserInput.EventUse -= OnInputUseChange;
	}


	public void Update()
	{
		// Check interaction for objects when nothing is clicked
		if (CGamePlayers.SelfActor == gameObject)
		{
			CheckInteraction(EInteractionType.Hover);
		}
	}

	
	[AClientOnly]
	private void CheckInteraction(EInteractionType _eIneractionType)
	{
		if (CGamePlayers.SelfActor != null)
		{
			// Find the origin, direction, distance of the players interaction cursor
			CPlayerHead cPlayerHeadMotor = CGamePlayers.SelfActor.GetComponent<CPlayerHead>();
			Vector3 vOrigin = cPlayerHeadMotor.ActorHead.transform.position;
			Vector3 vDirection = cPlayerHeadMotor.ActorHead.transform.forward;
			float fDistance = 5.0f;
			RaycastHit cRayHit = new RaycastHit();
			GameObject hitActorInteractable = null;
			
			// Check if the player cast a ray on the screen
			if(CheckInteractableObjectRaycast(vOrigin, vDirection, fDistance, out cRayHit))
			{
				// Get the game object which owns this mesh
				GameObject hitObject = cRayHit.collider.gameObject;
				
				// Check the parents until we find the one that has CActorInteractable on it
				while(hitObject.GetComponent<CActorInteractable>() == null)
				{
					if (hitObject.transform.parent != null)
					{
						hitObject = hitObject.transform.parent.gameObject;
					}
					else
					{
						Debug.LogError("PlayerInteractor hit game object without any CActorInteractable attached!");
						break;
					}
				}


				hitActorInteractable = hitObject;			

				CGamePlayers.SelfActor.GetComponent<CPlayerIKController>().RightHandIKTarget = cRayHit.point;	
			}

			// If this is a valid interactable actor
			if (hitActorInteractable != null)
			{
				// Get the network view id of the intractable object
				CNetworkView networkView = hitActorInteractable.GetComponent<CNetworkView>();

				if(networkView != null)
				{
					// Fire the interactable event for the actor that was interacted with
					hitActorInteractable.GetComponent<CActorInteractable>().OnInteractionEvent(_eIneractionType, gameObject, cRayHit);
				}
				else
				{
					Debug.LogError("Something has gone wrong here... There was no CNetworkView component on CActorInteractable!");
				}
				
				if (EventInteraction != null)
				{
					// Fire the interaction event for the player interactor
					EventInteraction(_eIneractionType, hitActorInteractable, cRayHit);
				}
			}
			else
			{
				if (EventNoInteraction != null)
				{
					// Fire the no interaction event for the player interactor
					EventNoInteraction(_eIneractionType, cRayHit);
				}
			}
		}
	}
	

	private static bool CheckInteractableObjectRaycast(Vector3 _origin, Vector3 _direction, float _fDistance, out RaycastHit _rh)
    {
		Ray ray = new Ray(_origin, _direction);
		
		if (Physics.Raycast(ray, out _rh, _fDistance, 1 << LayerMask.NameToLayer("InteractableObject")))
		{
			Debug.DrawRay(_origin, _direction * _fDistance, Color.green, 1.0f);
			return(true);
		}
		
		Debug.DrawRay(_origin, _direction * _fDistance, Color.red, 1.0f);
		
		return(false); 
    }


	[AClientOnly]
	void OnInputPrimaryChange(bool _bDown)
	{
		CheckInteraction((_bDown) ? EInteractionType.PrimaryStart : EInteractionType.PrimaryEnd);
	}


	[AClientOnly]
	void OnInputSecondaryChange(bool _bDown)
	{
		CheckInteraction((_bDown) ? EInteractionType.SecondaryStart : EInteractionType.SecondaryEnd);
	}


	[AClientOnly]
	void OnInputUseChange(bool _bDown)
	{
		if (_bDown)
		{
            //Debug.LogError("Hit twice" + gameObject.name);
			CheckInteraction(EInteractionType.Use);
		}
	}


// Member Fields


}
