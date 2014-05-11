//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CPlayerSkeleton
//  Description :   Holds onto variables related to the player's skeleton, which can be accessed b ut not modified
//
//  Author      :  
//  Mail        :  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;

/* Implementation */

public class CPlayerSkeleton : MonoBehaviour 
{
	//Member Types
	
	//Member Delegates & Events   
	
	//Member Properties  
	public GameObject SkeletonRoot
	{
		get { return(m_skeletonRoot);}
	}

	public GameObject RagdollHead
	{
		get { return(m_ragdollHead);}
	}

	public GameObject PlayerHead
	{
		get { return(m_playerHead);}
	}

	public GameObject PlayerRightHand
	{
		get { return(m_playerRightHand);}
	}

	public Avatar PlayerAvatar
	{
		get { return(m_headlessAvatar);}
	}

    public GameObject PlayerNaniteArmBand
    {
        get { return (m_cNaniteArmBand); }
    }

    public GameObject PlayerNaniteArmBandLaserNode
    {
        get { return (m_cNaniteArmBandLaserNode); }
    }

    //Member methods
    void Start()
    {
        m_vDefaultNaniteArmBandRotation = m_cNaniteArmBand.transform.localEulerAngles;
    }
	
	//Member variables

	public GameObject 	m_skeletonRoot;
	public GameObject 	m_ragdollHead;
	public GameObject 	m_playerHead;
    public GameObject   m_playerNeck;
	public GameObject   m_playerRightHand;
	public Avatar		m_headlessAvatar;

    public GameObject m_cNaniteArmBand;
    public GameObject m_cNaniteArmBandLaserNode;

    public Vector3 m_vDefaultNaniteArmBandRotation = new Vector3();

}
