//  Auckland
//  New Zealand
//
//  (c) 2013
//
//  File Name   :   ShipHazardMalfunction.cs
//  Description :   --------------------------
//
//  Author  	:  
//  Mail    	:  @hotmail.com
//


// Namespaces
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


static public class ShipHazardMalfunction
{
    // Member Functions
    static public void Trigger()
    {
        // Create an array of module interfaces
        CModuleInterface[] ArrayModules = CGameShips.Ship.GetComponentsInChildren<CModuleInterface>();

        // Create a list of functional components
        List<CComponentInterface> ListFunctionalComponents = new List<CComponentInterface>();

        // For each module on the ship
        foreach (CModuleInterface ModInt in ArrayModules)
        {
            // Create a local variable for holding the current component interfaces
            CComponentInterface[] LocalCompInterface = ModInt.GetComponentsInChildren<CComponentInterface>();

            // For each component in the module
            foreach (CComponentInterface CompInt in LocalCompInterface)
            {
                // If the component is functional
                if (CompInt.IsFunctional)
                {
                    // Add the component to the list
                    ListFunctionalComponents.Add(CompInt);
                }
            }
        }

        // If there is a functional component
        if (ListFunctionalComponents.Count != 0)
        {
            // Randomly determine a functional component to malfunction
            int iRandomComponent = (int)(Random.value * 100.0f) % ListFunctionalComponents.Count;

            // Trigger a malfunction on the selected component
            ListFunctionalComponents[iRandomComponent].TriggerMalfunction();
        }
    }
}