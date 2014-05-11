//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CLASSNAME.cs
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


[RequireComponent(typeof(CShieldGeneratorInterface))]
public class CShieldGeneratorSmallBehaviour : MonoBehaviour
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
        m_cTransInnerRotor.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);
        m_cTransMiddleRotor.transform.Rotate(Vector3.up, -90.0f * Time.deltaTime);
        m_cTransOuterRotor.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);
	}


// Member Fields


    public Transform m_cTransInnerRotor = null;
    public Transform m_cTransMiddleRotor = null;
    public Transform m_cTransOuterRotor = null;


};
