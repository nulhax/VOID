//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorCamera.cs
//  Description :   --------------------------
//
//  Author  	:  Programming Team
//  Mail    	:  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CActorCamera : MonoBehaviour
{

// Member Types


// Member Functions

    // public:


    public void Start()
    {
        GameObject.Find("Main Camera").camera.enabled = false;
        

        m_cCamera = GameObject.Instantiate(Resources.Load("Prefabs/Actor Camera", typeof(GameObject))) as GameObject;
        m_cCamera.transform.parent = gameObject.transform;
        m_cCamera.transform.position = gameObject.transform.position + new Vector3(0.0f, 1.5f, -5.0f);
    }


    public void OnDestroy()
    {
    }


    public void Update()
    {
    }


    // protected:


    // private:


// Member Variables

    // protected:


    // private:


    GameObject m_cCamera = null;


};
