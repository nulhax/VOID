//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   CRoomAtmosphere.cs
//  Description :   Atmosphere information for rooms
//
//  Author  	: 
//  Mail    	: 
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
	public delegate void HandleFacilityAtmosphereEvent(CFacilityAtmosphere _Self);

	public event HandleFacilityAtmosphereEvent EventDecompressionStarted;
	public event HandleFacilityAtmosphereEvent EventDecompressionFinished;


	// Member Fields	
	private List<GameObject> m_Consumers = new List<GameObject>();

	private CNetworkVar<float> m_Volume = null;						// Unit: m3 
	private CNetworkVar<float> m_Pressure = null;					// Unit: Pa (Pascals)
	private CNetworkVar<float> m_Temperature = null;				// Unit: K (Kelvin)
	private CNetworkVar<float> m_Density = null;					// Unit: Kg/m3
	
	private const float k_SpaceTempurature = 2.7f;					// Unit: K (Kelvin)

	private const float k_IdealInteriorTemperature = 295.0f; 		// Unit: K (Kelvin)
	private const float k_IdealInteriorAirDensity = 1.1965f; 		// Unit: Kg/m3
	private const float k_IdealInteriorPressure = 101325.0f; 		// Unit: Pa (Pascals)

	private const float k_DischargeCoefficient = 0.61f;
	private const float k_UniversalGasConstant = 8.3144621f;		// Unit: J K−1 mol−1
	private const float k_SpecificGasConstant = 287.058f;			// Unit: J kg−1 K−1

	public float airDensity = 0.0f;
	public float airPressure = 0.0f;

	// Server Member Fields
	private float m_fConsumptionRate = 0.0f;
	
	private bool m_RefillingEnabled = true;
	private bool m_Depressurizing = false;


	// Member Properties
	public float Volume
	{
		get { return(m_Volume.Value); } 
	}

    public float Pressure
    {
		get { return(m_Pressure.Value); }
    }

	public float Temperature
	{
		get { return(m_Temperature.Value); }
	}

	public float Density
	{
		get { return(m_Density.Value); }
	}
	
//	public bool IsRefillingRequired
//	{
//        get { return(IsRefillingEnabled && Pressure != 1.0f); } 
//	}
//
//    public bool IsRefillingEnabled
//    {
//        get { return(!IsDepressurizing && m_RefillingEnabled); }
//    }
//
//    [AServerOnly]
//    public float ConsumptionRate
//    {
//        get { return(m_fConsumptionRate); }
//    }
//
//    [AServerOnly]
//    public bool IsDepressurizing
//    {
//        get { return(m_Depressurizing); }
//    }

	// Member Methods
    public override void RegisterNetworkComponents(CNetworkViewRegistrar _cRegistrar)
    {
		m_Volume = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_Pressure = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_Temperature = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
		m_Density = _cRegistrar.CreateReliableNetworkVar<float>(OnNetworkVarSync, 0.0f);
    }

	private void OnNetworkVarSync(INetworkVar _cSynedVar)
	{
		// Empty
	}

	private void Start()
	{
		if(!CNetwork.IsServer)
			return;
		
		// Calculate the volume of the facility
		float volumeMetresCubed = 0.0f;
		float tileSize = CGameShips.Ship.GetComponent<CShipFacilities>().m_ShipGrid.m_TileSize;
		foreach(CTileInterface tileInterface in gameObject.GetComponent<CFacilityTiles>().InteriorTiles)
			volumeMetresCubed += tileSize * tileSize * tileSize;
		
		// Set the volume
		m_Volume.Value = volumeMetresCubed;
		
		// Set the tempertaure
		m_Temperature.Value = k_IdealInteriorTemperature;
		
		// Set the air density
		m_Density.Value = k_IdealInteriorAirDensity;

		// Set the atmospheric pressure
		m_Pressure.Value = k_IdealInteriorPressure;

		// Subscribe to door events
		foreach(GameObject door in gameObject.GetComponent<CFacilityInterface>().AllDoors)
		{
			CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();
			
			//			doorInterface.EventOpenStart += OnDoorOpenStart;
			//			doorInterface.EventCloseFinish += OnDoorCloseFinish;
		}
	}

	[AServerOnly]
	public void SetPressure(float _Pressure)
	{
		m_Pressure.Value = _Pressure;
	}

	[AServerOnly]
	public void SetDensity(float _Density)
	{
		m_Density.Value = _Density;
	}

//    [AServerOnly]
//    public void SetQuanity(float _fAmount)
//    {
//        if (_fAmount < 0.0f)
//        {
//            _fAmount = 0.0f;
//        }
//		else if (_fAmount > Volume)
//        {
//			_fAmount = Volume;
//        }
//
//        m_Temperature.Set(_fAmount);
//    }
//    [AServerOnly]
//    public void SetRefillingState(bool _State)
//    {
//        m_RefillingEnabled = _State;
//    }
//
//    [AServerOnly]
//    public void SetDepressurizingState(bool _State)
//    {
//        m_Depressurizing = _State;
//
//		if (EventDecompression != null) 
//			EventDecompression(m_Depressurizing);
//    }
//
//    [AServerOnly]
//    public void RegisterAtmosphericConsumer(GameObject _Consumer)
//    {
//        if(!m_Consumers.Contains(_Consumer))
//            m_Consumers.Add(_Consumer);
//    }
//
//    [AServerOnly]
//    public void UnregisterAtmosphericConsumer(GameObject _Consumer)
//    {
//        if(m_Consumers.Contains(_Consumer))
//            m_Consumers.Remove(_Consumer);
//    }
	
	private void Update()
	{
		if(!CNetwork.IsServer)
			return;

		ProcessExteriorDoorDecompression();
		ProcessInteriorDoorDecompression();

		airDensity = Density;
		airPressure = Pressure;
	}

//    [AServerOnly]
//    void ProcessDecompression()
//    {
//        if(!IsDepressurizing)
//			return;
//
//		if(Quantity != 0.0f)
//			return;
//
//		ChangeQuantityByAmount(-k_fControlledDecompressionRate * Time.deltaTime);
//    }

//    [AServerOnly]
//	private void ProcessConsumption()
//    {
//        // Calulate the combined consumption rate within the facility
//        float ConsumptionRate = 0.0f;
//
//		foreach(GameObject consumer in m_Consumers)
//		{
//            CActorAtmosphericConsumer cActorAtmosphereConsumer = consumer.GetComponent<CActorAtmosphericConsumer>();
//
//            if(!cActorAtmosphereConsumer.IsConsumingAtmosphere)
//				continue;
//
//			ConsumptionRate += cActorAtmosphereConsumer.AtmosphericConsumptionRate;
//        }
//
//		// Save consumption rate
//		m_fConsumptionRate = ConsumptionRate;
//
//        ChangeQuantityByAmount(m_fConsumptionRate * Time.deltaTime);
//    }

	[AServerOnly]
	private void ProcessInteriorDoorDecompression()
	{
		foreach(GameObject door in gameObject.GetComponent<CFacilityInterface>().m_InteriorDoors)
		{
			CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();

			// Get the opening area
			float openingArea = doorInterface.DoorOrificeArea;

			if(!doorInterface.IsOpened || openingArea == 0.0f)
				continue;
			
			GameObject neighbourFacility = doorInterface.m_FirstConnectedFacility == gameObject ? 
				doorInterface.m_SecondConnectedFacility : doorInterface.m_FirstConnectedFacility;
			
			CFacilityAtmosphere otherFacilityAtmos = neighbourFacility.GetComponent<CFacilityAtmosphere>();

			// Compair the neighbour's pressure
			if(otherFacilityAtmos.Pressure >= Pressure)
				continue;
			
			// Calculate the pressure difference
			float pressureDifference = Pressure - otherFacilityAtmos.Pressure;

			// Calculate the volumetric flow rate (Q = C * A * abs(2 * pressureDiff / Density))
			float volumetricFlowRate = (k_DischargeCoefficient * openingArea) * Mathf.Pow(Mathf.Abs((2.0f * pressureDifference) / Density), 0.5f);
			
			// Calculate the mass flow rate (Qm = p * Q)
			float massFlowRate = Density * volumetricFlowRate;
			
			// Get the delta mass
			float deltaMass = massFlowRate * Time.deltaTime;
			
			// Calculate the new mass for air
			float newAirMass = (Volume * Density) - deltaMass;
			float otherNewAirMass = (otherFacilityAtmos.Volume * otherFacilityAtmos.Density) + deltaMass;

			// Calculate the new air density
			float newAirDensity = newAirMass / Volume;
			float otherNewAirDensity = otherNewAirMass / otherFacilityAtmos.Volume;

			// Calculate the new pressure
			float newPressure = (newAirDensity * k_SpecificGasConstant * Temperature);
			float otherNewPressure = (otherNewAirDensity * k_SpecificGasConstant * otherFacilityAtmos.Temperature);

			// Set the values for this faclity
			SetPressure(newPressure);
			SetDensity(newAirDensity);

			// Set the values for the other faclity
			otherFacilityAtmos.SetPressure(otherNewPressure);
			otherFacilityAtmos.SetDensity(otherNewAirDensity);
		}
	}

	static float timer = 0.0f;

	[AServerOnly]
	private void ProcessExteriorDoorDecompression()
	{
		foreach(GameObject door in gameObject.GetComponent<CFacilityInterface>().m_ExteriorDoors)
		{
			CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();

			// Get the opening area
			float openingArea = doorInterface.DoorOrificeArea;

			if(!doorInterface.IsOpened || openingArea == 0.0f)
				continue;

			if(Density < 0.0001f)
				continue;

			// Calculate the volumetric flow rate (Q = C * A * abs(2 * pressureDiff/Air Density))
			float volumetricFlowRate = (k_DischargeCoefficient * openingArea) * Mathf.Pow(Mathf.Abs((2.0f * Pressure) / Density), 0.5f);

			// Calculate the mass flow rate (Qm = p * Q)
			float massFlowRate = Density * volumetricFlowRate;

			// Get the delta mass
			float deltaMass = massFlowRate * Time.deltaTime;

			// Calculate the new mass for air
			float newAirMass = (Volume * Density) - deltaMass;

			// Calculate the new air density
			float newAirDensity = newAirMass / Volume;

			// Calculate the new pressure
			float newPressure = (newAirDensity * k_SpecificGasConstant * Temperature);

			// Set the values
			m_Pressure.Value = newPressure;
			m_Density.Value = newAirDensity;
		}
	}



	// Q = C A sqrt( k p P ( 2 / (k + 1) )^ (k + 1)/(k - 1) )
	// Q = C A P sqrt( (k M / Z R T) ( 2 / k + 1) ^ (k + 1)/(k - 1) )
	//
	//			Q =  mass flow rate, kg / s 
	//			C =  discharge coefficient     (dimensionless, usually about 0.72) 
	//			A =  discharge hole area, m 2 
	//			k =  cp / cv  of the gas  =  the isentropic expansion coefficient
	//			  =  (specific heat at constant pressure) / (specific heat at constant volume)
	//			ρ =  real gas density, kg / m 3 at P and T 
	//			P =  absolute source or upstream pressure, Pa 
	//			PA = absolute ambient or downstream pressure, Pa  
	//			M =  gas molecular weight
	//			R =  the Universal Gas Law Constant  =  8314.5 ( Pa · m 3) / ( kgmol · °K )
	//			T =  gas temperature, °K
	//			Z =  the gas compressibility factor at P and T     (dimensionless)					
	//
	
	//float compressibilityFactor = Pressure / (AirDensity * (k_UniversalGasConstant / currentMolarMass) * Temperature);
	//float specificHeatRatio = 1.41f;
	
	//float massFlowRate = k_DischargeCoefficient * openingArea * Pressure *
	//	Mathf.Sqrt(Mathf.Pow((2.0f * currentMolarMass) / (compressibilityFactor * k_UniversalGasConstant * Temperature) * (2.0f / specificHeatRatio + 1.0f), (specificHeatRatio + 1.0f) * (specificHeatRatio - 1.0f)));
	
	//Debug.Log("Mass Flow Rate (other): " + massFlowRate);
	
	//float newTemperature = Temperature;//(newMolarMass * Pressure) / (newAirDensity * k_UniversalGasConstant);
	
	// Apply the air density difference
	//float currentAirMass = AirDensity * Volume;
	//float newAirDensity = (currentAirMass - airMassDifference) / Volume;
	//Debug.Log("New AirDensity = " + newAirDensity);
	
	// Calculate the new pressure (P = p * R * T)
	//float newPressure = (newAirDensity * k_UniversalGasConstant * Temperature) / newMolarMass;
	//Debug.Log("New Pressure = " + newPressure);
	
	// Calculate the new temperature (T2 = P2 * T1 / P1)
	//float newTemperature = newPressure * Temperature / Pressure;
	
	// Calculate the new temperature (T = P / p * R)
	//float newTemperature = (newMolarMass * newPressure) / (newAirDensity * k_UniversalGasConstant);
	//Debug.Log("New Temperature = " + newTemperature);
	
	// Calculate the flow rate (Q = C * A * abs(2 * pressureDiff/Air Density))
	//			float openingArea = 0.1f;
	//			float flowRate = k_DischargeCoefficient * openingArea * Mathf.Pow(Mathf.Abs(2.0f * (Pressure) / AirDensity), 0.5f);
	//			float volumeTransfer = flowRate * Time.deltaTime;
	
	//			Debug.Log(volumeTransfer);
	
	//			// Calculate the new pressure with the given change of mass flow
	//			float modifiedVolume = Volume + volumeTransfer;
	//
	//			// Calculate the moles in the air (n = m / M) (m = V * p)
	//			float moles = modifiedVolume * AirDensity / k_MoleMassInAir;
	//			Debug.Log("Moles = " + moles);
	//
	//			// Calculate the new  pressure (P = n * R * T / V)
	//			float newPressure = (moles * k_SpecificGasConstant * Temperature) / modifiedVolume;
	//			Debug.Log("New Pressure = " + newPressure);
	//
	//			// Calculate the new temperature (T2 = P2 * T1 / P1)
	//			float newTemperature = newPressure * Temperature / Pressure;
	//			Debug.Log("New Temperature = " + newTemperature);
	//
	//			// Calculate the new air density (p = P / R * T)
	//			float newAirDensity = newPressure / (k_SpecificGasConstant * newTemperature);
	//			Debug.Log("New AirDensity = " + newAirDensity);






//    [AServerOnly]
//	private void CheckForDecompression()
//    {
//        bool exposiveDecompressing = false;
//
//        if(GetComponent<CFacilityHull>().IsBreached)
//        {
//            exposiveDecompressing = true;
//        }
//        else
//        {
//			foreach(GameObject door in gameObject.GetComponent<CFacilityInterface>().m_ExteriorDoors)
//			{
//				CDoorInterface doorInterface = door.GetComponent<CDoorInterface>();
//
//				// Check door is open
//				if(!doorInterface.IsOpened)
//					continue;
//
//				exposiveDecompressing = true;
//            }
//        }
//
//        if(exposiveDecompressing &&
//		   !IsExplosiveDepressurizing)
//        {
//            Debug.Log(gameObject.name + " explosive depressurizing enabled");
//        }
//
//        SetExplosiveDepressurizingState(exposiveDecompressing);
//    }

//	[AServerOnly]
//	private void OnDoorOpenStart(CDoorInterface _DoorInterface)
//	{
//		CheckForDecompression();
//	}
//
//	[AServerOnly]
//	private void OnDoorCloseFinish(CDoorInterface _DoorInterface)
//	{
//		CheckForDecompression();
//	}

//    private void OnHullEvent(CFacilityHull.EEventType _eEventType)
//    {
//        switch (_eEventType)
//        {
//            case CFacilityHull.EEventType.Breached:
//                CheckExplosiveDecompression();
//                break;
//
//            case CFacilityHull.EEventType.BreachFixed:
//                CheckExplosiveDecompression();
//                break;
//
//            default:
//                Debug.LogError("Unknown facility hull event. " + _eEventType);
//                break;
//        }
//    }
};
