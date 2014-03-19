//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CDuiFacilityDoorBehaviour.cs
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


public class CDuiDoorControlBehaviour : MonoBehaviour
{

// Member Types


    public enum EButton
    {
        INVLAID,

        OpenDoor,
        CloseDoor,

        MAX
    };


// Member Delegates & Events


    public delegate void NotifyButtonPressed(EButton _eButton);

    public event NotifyButtonPressed EventClickOpenDoor;
    public event NotifyButtonPressed EventClickCloseDoor;


// Member Properties


// Member Methods


    public void OnClickOpen()
    {
        if (CNetwork.IsServer)
        {
            if (EventClickOpenDoor != null) EventClickOpenDoor(EButton.OpenDoor);
        }
    }


    public void OnClickClose()
    {
        if (CNetwork.IsServer)
        {
            if (EventClickCloseDoor != null) EventClickCloseDoor(EButton.CloseDoor);
        }
    }


    void Start()
    {
    }


    void OnDestroy()
    {
    }


    void Update()
    {
    }


// Member Fields


};
