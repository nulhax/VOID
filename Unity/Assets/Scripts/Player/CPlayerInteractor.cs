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
using System.Linq;


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
		UseStart,
        UseEnd,
		
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
			float distance = 5.0f;
			GameObject hitActorInteractable = null;
			RaycastHit actualHit = new RaycastHit();

			// Do the raycast against all objects in path
			RaycastHit[] hits = Physics.RaycastAll(ray, distance).OrderBy(h => h.distance).ToArray();

			// Check each one for an interactable objectg
			foreach(RaycastHit hit in hits)
			{
				// Get the game object which owns this mesh
				GameObject hitObject = hit.collider.gameObject;

				// Check the object itself for the interactable script
				CActorInteractable hitInteractable = hitObject.GetComponent<CActorInteractable>();

				// Check the parents until we find the one that has CActorInteractable on it
				if(hitInteractable == null)
					hitInteractable = CUtility.FindInParents<CActorInteractable>(hitObject);

				// If found an interactable break out
				if(hitInteractable != null)
				{
					actualHit = hit;
					hitActorInteractable = hitInteractable.gameObject;	
					break;
				}
			}

			// If this is a valid interactable actor
			if(hitActorInteractable != null)
			{
				// Get the network view id of the intractable object
				CNetworkView networkView = hitActorInteractable.GetComponent<CNetworkView>();

				if(networkView != null)
				{
					// Fire the interactable event for the actor that was interacted with
					hitActorInteractable.GetComponent<CActorInteractable>().OnInteractionEvent(_eIneractionType, gameObject, actualHit);
				}
				else
				{
					Debug.LogError("Something has gone wrong here... There was no CNetworkView component on CActorInteractable!");
				}
				
				if (EventInteraction != null)
				{
					// Fire the interaction event for the player interactor
					EventInteraction(_eIneractionType, hitActorInteractable, actualHit);
				}

				Debug.DrawRay(ray.origin, ray.direction * distance, Color.green, 1.0f);
			}
			else
			{
				if (EventNoInteraction != null)
				{
					// Fire the no interaction event for the player interactor
					EventNoInteraction(_eIneractionType, actualHit);
				}

				Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1.0f);
			}
		}
	}


	[AClientOnly]
    void OnInputPrimaryChange(CUserInput.EInput _eInput, bool _bDown)
	{
        CheckInteraction((_bDown) ? EInteractionType.PrimaryStart : EInteractionType.PrimaryEnd);
	}


	[AClientOnly]
    void OnInputSecondaryChange(CUserInput.EInput _eInput, bool _bDown)
	{
        CheckInteraction((_bDown) ? EInteractionType.SecondaryStart : EInteractionType.SecondaryEnd);
	}


	[AClientOnly]
    void OnInputUseChange(CUserInput.EInput _eInput, bool _bDown)
	{
        if (_bDown)
		{
            //Debug.LogError("Hit twice" + gameObject.name);
			CheckInteraction(EInteractionType.UseStart);
		}
	}


// Member Fields


}
