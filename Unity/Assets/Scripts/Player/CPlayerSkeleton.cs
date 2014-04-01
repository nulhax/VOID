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

	public Avatar PlayerAvatar
	{
		get { return(m_headlessAvatar);}
	}
	
	//Member variables

	public GameObject 	m_skeletonRoot;
	public GameObject 	m_ragdollHead;
	public GameObject 	m_playerHead;
	public Avatar		m_headlessAvatar;
}
