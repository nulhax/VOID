//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	:  Nathan Boon
//  Mail    	:  Nathan.BooN@gmail.com
//

// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


/* Implementation */


public class CFacilityAtmosphere : CNetworkMonoBehaviour
{

// Member Types


// Member Delegates & Events


// Member Properties
	

    public float AtmosphereQuantity
    {
        get { return (m_fQuantity.Get()); }
    }


    public float AtmosphereQuantityPercentage
    {
		get { return ((AtmosphereQuantity / m_fVolume) * 100.0f); }
    }


	public float AtmosphereVolume
	{
		get { return (m_fVolume); } 
	}


	public float AtmosphereConsumeRate
	{
        get { return (m_fConsumptionRate); }
	}


	public bool IsAtmosphereRefillingRequired
	{
        get { return (IsAtmosphereRefillingEnabled &&
                      AtmosphereQuantityPercentage != 1.0f); } 
	}


    public bool IsAtmosphereRefillingEnabled
    {
        get { return (m_bRefillingEnabled); }
    }


// Member Methods
	

    public override void InstanceNetworkVars(CNetworkViewRegistrar _cRegistrar)
    {
        m_fQuantity = _cRegistrar.CreateNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }


    public void IncrementQuantity(float _fQuantity)
    {
        float fNewQuanity = m_fQuantity.Get() + _fQuantity;

        if (fNewQuanity < 0.0f)
        {
            fNewQuanity = 0.0f;
        }
        else if (fNewQuanity > m_fVolume)
        {
            fNewQuanity = m_fVolume;
        }

        m_fQuantity.Set(fNewQuanity);
    }


    [AServerOnly]
    public void RegisterAtmosphericConsumer(GameObject _cConsumer)
    {
        if (!m_aConsumers.Contains(_cConsumer))
        {
            m_aConsumers.Add(_cConsumer);
        }
    }


    [AServerOnly]
    public void UnregisterAtmosphericConsumer(GameObject _cConsumer)
    {
        if (m_aConsumers.Contains(_cConsumer))
        {
            m_aConsumers.Remove(_cConsumer);
        }
    }


    void Start()
    {
        if (CNetwork.IsServer)
        {
            // Debug: Atmosphere starts at half the total volume
            m_fQuantity.Set(AtmosphereVolume / 2);

            GetComponent<CFacilityHull>().EventBreached += () =>
            {
                m_bExplosiveDepressurizing = true;
            };

            GetComponent<CFacilityHull>().EventBreachFixed += () =>
            {
                m_bExplosiveDepressurizing = false;
            };
        }
    }

	
	void Update()
	{
		if(CNetwork.IsServer)
		{
			// Remove consumers that are now null
			m_aConsumers.RemoveAll((item) => item == null);

			UpdateConsumptionRate();

            // Turn on warning alarms
            if (!m_bAlarmsActive &&
                AtmosphereQuantityPercentage < 25.0f)
            {
                gameObject.GetComponent<CFacilityInterface>().FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning).ForEach((_cAlarmObject) =>
                {
                    _cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(true);
                });
            }
            
            // Turn off alarms
            else if (m_bAlarmsActive &&
                     AtmosphereQuantityPercentage > 25.0f)
            {
                gameObject.GetComponent<CFacilityInterface>().FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning).ForEach((_cAlarmObject) =>
                {
                    _cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(false);
                });

                m_bAlarmsActive = false;
            }

            // Compute explosive decompression
            if (m_bExplosiveDepressurizing)
            {
                if (m_fQuantity.Get() != 0.0f)
                {
                    IncrementQuantity(-m_fVolume * k_fExplosiveDecompressionRatio * Time.deltaTime);
                }
            }

            // Loop through expansion ports
            foreach (GameObject cExpansionPortObject in GetComponent<CFacilityExpansion>().ExpansionPorts)
            {
                CExpansionPortBehaviour cExpansionPortBehaviour = cExpansionPortObject.GetComponent<CExpansionPortBehaviour>();

                // Check door is open on this expansion port
                if (cExpansionPortBehaviour.DoorBehaviour.GetComponent<CDoorBehaviour>().IsOpened)
                {
                    GameObject cAttachedFacilityObject = cExpansionPortBehaviour.AttachedFacilityObject;

                    // Check there is not a neighbouring facility
                    if (cAttachedFacilityObject == null)
                    {
                        /* The open door leads to outside space. Decompress */
                        gameObject.GetComponent<CFacilityInterface>().FindAccessoriesByType(CAccessoryInterface.EType.Alarm_Warning).ForEach((_cAlarmObject) =>
                        {
                            _cAlarmObject.GetComponent<CAlarmBehaviour>().SetAlarmActive(true);
                        });

                        Debug.LogError("Ahhhh the door to the outside is open. BREACHHHHEEDDD");
                    }
                    else
                    {
                        if (cAttachedFacilityObject.GetComponent<CFacilityAtmosphere>().AtmosphereQuantityPercentage < AtmosphereQuantityPercentage)
                        {




                            Debug.LogError("Ahhhh the atmosphere in the other facility is lower then mine" + cAttachedFacilityObject.GetComponent<CFacilityAtmosphere>().AtmosphereQuantityPercentage + ":" + AtmosphereQuantityPercentage);
                        }
                    }
                }
            }
		}
	}


	[AServerOnly]
	void UpdateConsumptionRate()
	{
		// Calulate the combined consumption rate within the facility
		float fConsumptionRate = 0.0f;

		m_aConsumers.ForEach((GameObject _cConsumer) =>
		{
            CActorAtmosphericConsumer cActorAtmosphereConsumer = _cConsumer.GetComponent<CActorAtmosphericConsumer>();

            if (cActorAtmosphereConsumer.IsConsumingAtmosphere)
            {
                fConsumptionRate += cActorAtmosphereConsumer.AtmosphericConsumptionRate;
            }
		});

		// Set the consumption rate
		m_fConsumptionRate = fConsumptionRate;
	}


    void OnNetworkVarSync(INetworkVar _cSynedVar)
    {
        // Empty
    }


// Member Fields


    public float m_fVolume = 1000.0f;

    List<GameObject> m_aConsumers = new List<GameObject>();

    CNetworkVar<float> m_fQuantity = null;

    bool m_bAlarmsActive = false;


    const float k_fExplosiveDecompressionRatio = 0.1f;


// Server Member Fields


    float m_fConsumptionRate = 0.0f;

    bool m_bRefillingEnabled = true;
    bool m_bDepressurizing = false;
    bool m_bExplosiveDepressurizing = false;


};
