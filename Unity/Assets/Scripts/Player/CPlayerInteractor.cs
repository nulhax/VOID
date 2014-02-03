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


	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
		// Empty
	}


	public void Start()
	{
		if (gameObject == CGamePlayers.SelfActor)
		{
            CUserInput.SubscribeInputChange(CUserInput.EInput.Primary, OnInputPrimaryChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Secondary, OnInputSecondaryChange);
            CUserInput.SubscribeInputChange(CUserInput.EInput.Use, OnInputUseChange);
		}
	}


	void OnDestroy()
	{
        CUserInput.UnsubscribeInputChange(CUserInput.EInput.Primary, OnInputPrimaryChange);
        CUserInput.UnsubscribeInputChange(CUserInput.EInput.Secondary, OnInputSecondaryChange);
        CUserInput.UnsubscribeInputChange(CUserInput.EInput.Use, OnInputUseChange);
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
			Camera headCamera = CGameCameras.PlayersHeadCamera;
			Ray ray = new Ray(headCamera.transform.position, headCamera.transform.forward);
			RaycastHit hit = new RaycastHit();
			float distance = 5.0f;
			GameObject hitActorInteractable = null;
			
			// Check if the player cast a ray on the screen
			if(Physics.Raycast(ray, out hit, distance))
			{
				// Get the game object which owns this mesh
				GameObject hitObject = hit.collider.gameObject;
				
				// Check the parents until we find the one that has CActorInteractable on it
				bool found = true;
				while(hitObject.GetComponent<CActorInteractable>() == null)
				{
					if(hitObject.transform.parent != null)
					{
						hitObject = hitObject.transform.parent.gameObject;
					}
					else
					{
						found = false;
						break;
					}
				}

				if(found)
					hitActorInteractable = hitObject;					
			}

			// If this is a valid interactable actor
			if(hitActorInteractable != null)
			{
				// Get the network view id of the intractable object
				CNetworkView networkView = hitActorInteractable.GetComponent<CNetworkView>();

				if(networkView != null)
				{
					// Fire the interactable event for the actor that was interacted with
					hitActorInteractable.GetComponent<CActorInteractable>().OnInteractionEvent(_eIneractionType, gameObject, hit);
				}
				else
				{
					Debug.LogError("Something has gone wrong here... There was no CNetworkView component on CActorInteractable!");
				}
				
				if (EventInteraction != null)
				{
					// Fire the interaction event for the player interactor
					EventInteraction(_eIneractionType, hitActorInteractable, hit);
				}

				Debug.DrawRay(ray.origin, ray.direction * distance, Color.green, 1.0f);
			}
			else
			{
				if (EventNoInteraction != null)
				{
					// Fire the no interaction event for the player interactor
					EventNoInteraction(_eIneractionType, hit);
				}

				Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1.0f);
			}
		}
	}


	[AClientOnly]
    void OnInputPrimaryChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
	{
        if (_ulPlayerId == 0)
        {
            CheckInteraction((_bDown) ? EInteractionType.PrimaryStart : EInteractionType.PrimaryEnd);
        }
	}


	[AClientOnly]
    void OnInputSecondaryChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
	{
        if (_ulPlayerId == 0)
        {
            CheckInteraction((_bDown) ? EInteractionType.SecondaryStart : EInteractionType.SecondaryEnd);
        }
	}


	[AClientOnly]
    void OnInputUseChange(CUserInput.EInput _eInput, ulong _ulPlayerId, bool _bDown)
	{
        if (_ulPlayerId == 0 &&
            _bDown)
		{
            //Debug.LogError("Hit twice" + gameObject.name);
			CheckInteraction(EInteractionType.Use);
		}
	}


// Member Fields


}
