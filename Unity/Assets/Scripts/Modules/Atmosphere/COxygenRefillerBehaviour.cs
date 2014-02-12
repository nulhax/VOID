//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   COxygenRefillerBehaviour.cs
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


public class COxygenRefillerBehaviour : MonoBehaviour
{

    // Member Types
    public enum ENetworkAction
    {
        RefillOxygen,
    }


    // Member Delegates & Events


    // Member Fields
    static CNetworkStream s_cSerializeStream = new CNetworkStream();


    // Member Properties


    // Member Methods


    void Start()
    {
        CActorInteractable ai = GetComponent<CActorInteractable>();

        ai.EventUseStart += OnPlayerUse;
    }


    void OnDestroy()
    {

    }


    void Update()
    {

    }

    [AClientOnly]
	public static void SerializeOutbound(CNetworkStream _cStream)
    {
		_cStream.Write(s_cSerializeStream);

		s_cSerializeStream.Clear();
    }

    [AServerOnly]
    public static void UnserializeInbound(CNetworkPlayer _cNetworkPlayer, CNetworkStream _cStream)
    {
        while (_cStream.HasUnreadData)
        {
            CNetworkViewId oxygenRefillViewId = _cStream.ReadNetworkViewId();
            ENetworkAction eAction = (ENetworkAction)_cStream.ReadByte();

            GameObject cOxygenRefillerObject = oxygenRefillViewId.GameObject;

            COxygenRefillerBehaviour blah = cOxygenRefillerObject.GetComponent<COxygenRefillerBehaviour>();

            switch (eAction)
            {
                case ENetworkAction.RefillOxygen:
                    blah.HandlePlayerOxygenRefill(_cNetworkPlayer.PlayerId);
                    break;
            }
        }
    }

    [AClientOnly]
    private void OnPlayerUse(RaycastHit _RayHit, CNetworkViewId _cPlayerActorViewId)
    {
        // TODO: if broken do not call this section
        s_cSerializeStream.Write(gameObject.GetComponent<CNetworkView>().ViewId);
        s_cSerializeStream.Write((byte)ENetworkAction.RefillOxygen);
        // section not to be called end.
    }

    [AServerOnly]
    private void HandlePlayerOxygenRefill(ulong _ulPlayerId)
    {
        //being broken may be done here, seeing as it is the server.
        GameObject cPlayerActor = CGamePlayers.GetPlayerActor(_ulPlayerId);

        cPlayerActor.GetComponent<CPlayerSuit>().AddOxygen(20.0f);
    }
};
