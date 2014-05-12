//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CShieldHitBehaviour.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/* Implementation */


public class CShieldHitBehaviour : MonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties


// Member Methods


	void Start()
	{
	}


	void OnDestroy()
	{
	}


	void Update()
	{
        m_fAliveTimer += Time.deltaTime;

        renderer.material.SetFloat("_UIAlpha", 1.0f - (m_fAliveTimer / m_fAliveDuration));

        if (m_fAliveTimer > m_fAliveDuration)
        {
            Destroy(transform.parent.gameObject);
        }
	}


// Member Fields


    public float m_fAliveDuration = 5.0f;


    float m_fAliveTimer = 0.0f;


};
