//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerRepairBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CPlayerRepairBehaviour : MonoBehaviour 
{
	
	// Member Types


	// Member Delegates & Events
	
	
	// Member Properties
	
	
	// Member Functions

	
	/* Implementation */
	

	// Use this for initialization
	void Start () 
	{
		gameObject.GetComponent<CPlayerInteractor>().EventInteraction += OnPlayerInteraction;
		gameObject.GetComponent<CPlayerBelt>().EventToolPickedup += OnToolChange;		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
	
	void OnToolChange(CNetworkViewId _cViewId)
	{
		if(_cViewId.GameObject != null)
		{
			m_HeldTool = _cViewId.GameObject.GetComponent<CToolInterface>();
		}
		else
		{
			m_HeldTool = null;
		}
	}
	
	public void OnPlayerInteraction(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
    {
        switch (_eType)
        {
            case CPlayerInteractor.EInteractionType.PrimaryStart:
            {
				Debug.Log("Interactable used: " + _cInteractableObject.name);
			
				if(m_HeldTool != null)
				{
					HandleToolUse(_eType, _cInteractableObject, _cRayHit);
				}
				else
				{
					Debug.Log("Player not holding a tool. No interaction possible");					
				}
			
				break;
            }

            case CPlayerInteractor.EInteractionType.PrimaryEnd:
            {
				if(m_HeldTool != null)
				{
	                switch(m_HeldTool.ToolType)           
					{
						case CToolInterface.EType.Ratchet:
						{
							m_HeldTool.GetComponent<CRatchetBehaviour>().EndRepairs();
							break;
						}
					}
				}
                break;
            }
        }
    }
	
	public void HandleToolUse(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
	{
		//Check that player is targetting a component
		if(_cInteractableObject.GetComponent<CComponentInterface>()	!= null)
		{
			CComponentInterface compInterface = _cInteractableObject.GetComponent<CComponentInterface>();
			
			//Make sure player is holding the right tool to repair with
			switch(compInterface.ComponentType)
			{
				case CComponentInterface.EType.MechanicalComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.Ratchet)
					{
						m_HeldTool.GetComponent<CRatchetBehaviour>().BeginRepair(_cInteractableObject);
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
					}
					break;
				}				
				case CComponentInterface.EType.CalibratorComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.Calibrator)
					{
	        							
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
					}
					break;
				}
				case CComponentInterface.EType.CircuitryComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.CircuitryKit)
					{
	        						
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
					}
					break;
				}
				case CComponentInterface.EType.FluidComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.Fluidizer)
					{
	        					
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
					}
					break;
				}					
			}
		}
	}	
	
	CToolInterface			m_HeldTool;	
}
