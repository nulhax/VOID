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
			CUserInput.EventPrimary += new CUserInput.NotifyKeyChange(OnInputPrimaryChange);
			CUserInput.EventSecondary += new CUserInput.NotifyKeyChange(OnInputSecondaryChange);
			CUserInput.EventUse += new CUserInput.NotifyKeyChange(OnInputUseChange);
		}
	}
	

	public void Update()
	{
		// Empty
	}

	
	[AClientOnly]
	private void CheckInteraction(EInteractionType _eIneractionType)
	{
		// Find the origin, direction, distance of the players interaction cursor
		CPlayerHead cPlayerHeadMotor = CGamePlayers.SelfActor.GetComponent<CPlayerHead>();
		Vector3 vOrigin = cPlayerHeadMotor.ActorHead.transform.position;
		Vector3 vDirection = cPlayerHeadMotor.ActorHead.transform.forward;
		float fDistance = 5.0f;
		RaycastHit cRayHit = new RaycastHit();
		GameObject cHitInteractableObject = null;

		
		// Check if the player cast a ray on the screen
		if(CheckInteractableObjectRaycast(vOrigin, vDirection, fDistance, out cRayHit))
		{
			// Get the game object which owns this mesh
			GameObject cHitObject = cRayHit.collider.gameObject;
			
			// Check the parents until we find the one that is not of layer InteractableObject
			int IOLayer = LayerMask.NameToLayer("InteractableObject");
			for(int i = 0; i < 10; ++i)
			{
				// Break endless loop
				if(i == 9)
				{
					Debug.LogError("Couldn't find the intractable objects parent!");
				}

                if (cHitObject.transform.parent != null &&
                    cHitObject.transform.parent.gameObject.layer == IOLayer)
				{
					cHitObject = cHitObject.transform.parent.gameObject;
				}
				else
				{
					break;
				}
			}
			
			// Get the intractable Object script from the object
			CActorInteractable cInteractableObjectComponent = cHitObject.GetComponent<CActorInteractable>();

			// If this is a valid IO
			if (cInteractableObjectComponent != null)
			{
				cHitInteractableObject = cHitObject;
			}
		}


		// If this is a valid IO
		if (cHitInteractableObject != null)
		{
			// Get the network view id of the intractable object
			CNetworkView cNetworkView = cHitInteractableObject.GetComponent<CNetworkView>();

			if (cNetworkView == null)
			{
				Debug.LogError("CheckInteractionObjects. Something has gone wrong here... There was no CNetworkView component!");
			}
			else
			{
				cHitInteractableObject.GetComponent<CActorInteractable>().OnInteractionEvent(_eIneractionType, gameObject, cRayHit);
			}
			
			if (EventInteraction != null)
			{
				EventInteraction(_eIneractionType, cHitInteractableObject, cRayHit);
			}
		}
		else
		{
			if (EventNoInteraction != null)
			{
				EventNoInteraction(_eIneractionType, cRayHit);
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
			CheckInteraction(EInteractionType.Use);
		}
	}


// Member Fields


}
