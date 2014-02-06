//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerIKController
//  Description :   Controls player IK, allowing a rigged character to place body parts at specified locations
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* Implementation */

public class CPlayerIKController : CNetworkMonoBehaviour
{
	
	//Member Types
	public enum ENetworkAction : byte
	{
		UpdateTarget,
	}
	
	//Member Delegates & Events
	
	
	// Member Properties
	
	public Vector3 RightHandIKTarget
	{
        set { m_RightHandTarget = value; m_fRightHandLerpTimer = 0; m_fRightHandIKWeight = 1; Debug.Log("New target: " + m_RightHandTarget.ToString());}			 
		get { return (m_RightHandTarget); }
	}

    public Vector3 LeftHandIKTarget
    {
        set { m_LeftHandTarget = value; m_fLeftHandLerpTimer = 0; }
        get { return (m_LeftHandTarget); }
    }   
	
	public float RightHandIKWeight
	{
		set { m_fRightHandIKWeight = value;  }
        get { return (m_fRightHandIKWeight); }
	}
	
	//Member variables
	Animator m_ThirdPersonAnim;
    CNetworkVar<Vector3>    m_RightHandNetworkedTarget;
    CNetworkVar<Vector3>    m_LeftHandNetworkedTarget;

	//Right hand
	float 					m_fRightHandIKWeight;
	Vector3					m_RightHandPos;
    Vector3					m_RightHandTarget;
	const float 			m_kfRightHandLerpTime = 0.75f;
	float 					m_fRightHandLerpTimer = 0.0f;
 
	//Left hand
	float 					m_fLeftHandIKWeight;
	Vector3					m_LeftHandTarget;	
	float 					m_fLeftHandLerpTime = 0.1f;
	float 					m_fLeftHandLerpTimer = 0.0f;		

	//Member Methods
	
	// Use this for initialization
	void Start () 
	{	
		m_ThirdPersonAnim = GetComponent<Animator>();          
   	} 

	public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
	{
		m_RightHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);
		m_LeftHandNetworkedTarget = _cRegistrar.CreateNetworkVar<UnityEngine.Vector3>(OnNetworkVarSync);     
	}

	void OnNetworkVarSync(INetworkVar _cSyncedNetworkVar)
	{ 
		if(_cSyncedNetworkVar == m_RightHandNetworkedTarget)
        {
            RightHandIKTarget = m_RightHandNetworkedTarget.Get();           
        }
        if(_cSyncedNetworkVar == m_LeftHandNetworkedTarget)
        {
            LeftHandIKTarget = m_LeftHandNetworkedTarget.Get();          
        }
	} 
	
	public static void SerializeIKTarget(CNetworkStream _cStream)
	{
		GameObject cSelfActor = CGamePlayers.SelfActor;
		
		if (cSelfActor != null)
		{
			CPlayerIKController cSelfIKController = cSelfActor.GetComponent<CPlayerIKController>();
			
			// Write rotation states
			_cStream.Write((byte)ENetworkAction.UpdateTarget);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.x);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.y);
			_cStream.Write((float)cSelfIKController.m_RightHandTarget.z);	      
		}
	}
	
	public static void UnserializeIKTarget(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
	{
		GameObject cPlayerActor = CGamePlayers.FindPlayerActor(_cNetworkPlayer.PlayerId);
		
		if (cPlayerActor != null)
		{			
			CPlayerIKController cPlayerIKController = cPlayerActor.GetComponent<CPlayerIKController>();
			
			ENetworkAction eNetworkAction = (ENetworkAction)_cStream.ReadByte();
			
			switch (eNetworkAction)
			{
			case ENetworkAction.UpdateTarget:
			{
				cPlayerIKController.m_RightHandTarget.x = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.y = _cStream.ReadFloat();
				cPlayerIKController.m_RightHandTarget.z = _cStream.ReadFloat();	
				
				cPlayerIKController.m_RightHandNetworkedTarget.Set(cPlayerIKController.m_RightHandTarget);              
			}
				break;
				
			default:
				Debug.LogError(string.Format("Unknown network action ({0})", (byte)eNetworkAction));
				break;
			}
		}		
	}  
	
	// Update is called once per frame
	void Update ()
	{ 			
       	LerpRightHand();             				
	}  

    void LerpRightHand()
    {
        if(m_fRightHandLerpTimer < m_kfRightHandLerpTime)
        {
            m_fRightHandLerpTimer += Time.deltaTime;   
			
			Vector3 pos = m_ThirdPersonAnim.GetIKPosition(AvatarIKGoal.RightHand);			
            float LerpFactor = m_fRightHandLerpTimer / m_kfRightHandLerpTime;
			
            m_RightHandPos = Vector3.Lerp(pos, m_RightHandTarget, LerpFactor);
        }        
    }
	
	void OnAnimatorIK()
	{
		if(m_ThirdPersonAnim) 
		{		
			//Right hand IK						
			//set the position and the rotation of the right hand where the external object is
			if(m_RightHandTarget != null)
			{
                m_ThirdPersonAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
                m_ThirdPersonAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, m_fRightHandIKWeight);
													
				m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandPos);
				//m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
			}			
		}
	}  
}
