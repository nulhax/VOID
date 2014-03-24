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
		//gameObject.GetComponent<CPlayerInteractor>().EventTargetChange += OnPlayerInteraction;
		//gameObject.GetComponent<CPlayerBelt>().EventToolPickedup += OnToolChange;		
        //gameObject.GetComponent<CPlayerBelt>().EventToolDropped += OnToolDrop;   
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
            gameObject.GetComponent<CThirdPersonAnimController>().RaiseArm();
            // Commented out by Nathan to avoid extraneous debug information.
            // Feel free to uncomment for debugging purposes when required.
            //Debug.Log("Tool changed to" + m_HeldTool.gameObject.name);
		}
		else
		{
            m_HeldTool = null;
            gameObject.GetComponent<CThirdPersonAnimController>().LowerArm();
            // Commented out by Nathan to avoid extraneous debug information.
            // Feel free to uncomment for debugging purposes when required.
            //Debug.Log("Tool changed to" + m_HeldTool.gameObject.name);
		}
	}
    void OnToolDrop(CNetworkViewId _cViewId)
    {
        m_HeldTool = null;
        gameObject.GetComponent<CThirdPersonAnimController>().LowerArm();  
    }
	
    /*
	public void OnPlayerInteraction(CPlayerInteractor.EInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
    {
        switch (_eType)
        {
            case CPlayerInteractor.EInteractionType.PrimaryStart:
            {
                // Commented out by Nathan to avoid extraneous debug information.
                // Feel free to uncomment for debugging purposes when required.
				//Debug.Log("Interactable used: " + _cInteractableObject.name);
			
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
                if(m_HeldTool != null &&  m_bRepairing)
				{
	                switch(m_HeldTool.ToolType)           
					{
						case CToolInterface.EType.Ratchet:
						{
							m_HeldTool.GetComponent<CRatchetBehaviour>().EndRepairs();
                            m_bRepairing = false;
							break;
						}
                        case CToolInterface.EType.CircuitryKit:
                        {
                            m_HeldTool.GetComponent<CCircuitryKitBehaviour>().EndRepairs();
                            m_bRepairing = false;
                            break;
                        }
                        case CToolInterface.EType.Calibrator:
                        {
                            m_HeldTool.GetComponent<CCalibratorBehaviour>().EndRepairs();
                            m_bRepairing = false;
                            break;
                        }
                        case CToolInterface.EType.Fluidizer:
                        {
                            m_HeldTool.GetComponent<CFluidToolBehaviour>().EndRepairs();
                            m_bRepairing = false;
                            break;
                        }
					}
				}
                break;
            }
        }
    }
     * */
	
   
    /*
	public void HandleToolUse(CPlayerInteractor.EInputInteractionType _eType, GameObject _cInteractableObject, RaycastHit _cRayHit)
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
                        m_bRepairing = true;
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
                        m_bRepairing = false;
					}
					break;
				}				
				case CComponentInterface.EType.CalibratorComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.Calibrator)
					{
                        m_HeldTool.GetComponent<CCalibratorBehaviour>().BeginRepair(_cInteractableObject);
                        m_bRepairing = true;
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
                        m_bRepairing = false;
					}
					break;
				}
				case CComponentInterface.EType.CircuitryComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.CircuitryKit)
					{
                        m_HeldTool.GetComponent<CCircuitryKitBehaviour>().BeginRepair(_cInteractableObject);	
                        m_bRepairing = true;
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
                        m_bRepairing = false;
					}
					break;
				}
				case CComponentInterface.EType.FluidComp:
				{		
					if(m_HeldTool.ToolType == CToolInterface.EType.Fluidizer)
					{
                        m_HeldTool.GetComponent<CFluidToolBehaviour>().BeginRepair(_cInteractableObject);  
                        m_bRepairing = true;
					}
					else
					{
						Debug.Log("Player not holding correct tool for interaction");
                        m_bRepairing = false;
					}
					break;
				}					
			}
		}
	}	
     * */

	
	CToolInterface			m_HeldTool;	
    bool                    m_bRepairing = false;
}
