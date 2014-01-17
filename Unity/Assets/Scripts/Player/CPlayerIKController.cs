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

/* Implementation */

public class CPlayerIKController : MonoBehaviour {

	//Member Types

	//Member Delegates & Events
	
	// Member Properties
	public Vector3 RightHandIKTarget
	{
		set { m_RightHandTarget = value; m_fRightHandWeightTarget = 1; }
		get { return (m_RightHandTarget); }
	}

	//Member variables
	Animator m_ThirdPersonAnim;

	float m_fRightHandIKWeight;
	float m_fRightHandWeightTarget;
	public Vector3 m_RightHandTarget;

	float m_fLerpTime = 0.5f;
	float m_fLerpTimer = 0.0f;

	//Member Methods

	// Use this for initialization
	void Start () 
	{
		m_ThirdPersonAnim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		IKTest();
		LerpToTarget();
	}

	void IKTest()
	{
		int iIgnoreLayer = 1 << 13;
		iIgnoreLayer = ~ iIgnoreLayer;
		
		Vector3 rayCastPos = transform.position;
		rayCastPos.y += 1.0f;
		
		if(Input.GetMouseButtonDown(0))
		{
			//Raycast for right hand
			RaycastHit RightHandHit;
			
			CPlayerHead cPlayerHeadMotor = CGamePlayers.SelfActor.GetComponent<CPlayerHead>();
			Vector3 vOrigin = cPlayerHeadMotor.ActorHead.transform.position;
			Vector3 vDirection = cPlayerHeadMotor.ActorHead.transform.forward;
			
			if(Physics.Raycast(vOrigin, vDirection, out RightHandHit,  2.0f, iIgnoreLayer))
			{
				m_RightHandTarget = RightHandHit.point;
				m_fRightHandWeightTarget = 1;
			}
			
			Debug.DrawLine(vOrigin, vDirection, Color.magenta, 0.1f);
		}
	}



	void LerpToTarget()
	{
		if(m_fRightHandIKWeight != m_fRightHandWeightTarget)
		{
			m_fLerpTimer += Time.deltaTime;

			float LerpFactor = m_fLerpTimer / m_fLerpTime;
			m_fRightHandIKWeight = Mathf.Lerp(m_fRightHandIKWeight, m_fRightHandWeightTarget, LerpFactor);
		}
		else
		{
			m_fLerpTimer = 0.0f;
			m_fRightHandWeightTarget = 0.0f;
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

				Quaternion handRotation = Quaternion.LookRotation(m_RightHandTarget - transform.position);
				m_ThirdPersonAnim.SetIKPosition(AvatarIKGoal.RightHand, m_RightHandTarget);
				m_ThirdPersonAnim.SetIKRotation(AvatarIKGoal.RightHand, handRotation);
			}			
		}
	}
}
