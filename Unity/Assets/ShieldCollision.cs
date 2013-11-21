//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   ShieldCollision.cs
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


public class ShieldCollision : MonoBehaviour
{

    // Member Types


    // Member Delegates & Events


    // Member Properties


    // Member Functions


    public void Start()
    {
        m_bIsDrawing = false;

        renderer.enabled = m_bIsDrawing;

        //Vector3 BoundsCenter = new Vector3(10.0f, 5.0f, 7.0f);

        //Vector3 BoundsSize = new Vector3(100.0f, 100.0f, 5.0f);

        //Bounds ShieldBounds = new Bounds(BoundsCenter, BoundsSize);

        //gameObject.GetComponent<SphereCollider>().bounds.Encapsulate(ShieldBounds);
    }


    public void OnDestroy()
    {

    }


    public void Update()
    {
        ChangeRendered();
    }


    public void ChangeRendered()
    {
        renderer.enabled = m_bIsDrawing;
    }


    public void OnTriggerEnter(Collider OtherObject)
    {
        m_bIsDrawing = true;

        OtherObject.gameObject.rigidbody.velocity *= -1.0f;
    }


    public void OnTriggerExit()
    {
        m_bIsDrawing = false;
    }


    public void OnTrigger()
    {
        //renderer.enabled = true;
    }


    // Member Fields
    bool m_bIsDrawing;

};
