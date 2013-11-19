//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CDUIConsole.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CDUIConsole : MonoBehaviour 
{
	// Retarded public fields for the editor
	public GameObject m_ScreenObject = null;
	
    // Member Fields 
	private GameObject m_DUI = null;
	
    // Member Properties
	public CDUI DUI 
	{ 
		get 
		{ 
			return(m_DUI.GetComponent<CDUI>()); 
		} 
	}
	
	public GameObject DUIGameObject 
	{ 
		get 
		{ 
			return(m_DUI); 
		} 
	}
	
	// Member Methods
    public void Initialise(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions)
    {
		// Check the screen is assigned in the editor
		if(m_ScreenObject == null)
		{
			Debug.LogError("CDUIConsole Initialise failed. ScreenObj hasn't been assigned!");
		}
		
		// Create the DUI object
		CreateDUI(_Quality, _Layout, _Dimensions);
		
		// Initialise the duiInteraction script
		GetComponent<CDUIInteraction>().Initialise();
    }
	
    private void CreateDUI(EQuality _Quality, ELayoutStyle _Layout, Vector2 _Dimensions)
	{
		// Create the DUI game object
        m_DUI = new GameObject();
        m_DUI.name = name + "_DUI";
        m_DUI.layer = LayerMask.NameToLayer("DUI");
		m_DUI.transform.rotation = Quaternion.Euler(0.0f, transform.eulerAngles.y, 0.0f);
        m_DUI.transform.position = transform.position + m_DUI.transform.rotation * new Vector3(0.0f, 0.0f, -1.0f);
		
        // Add the DUI component
        CDUI dui = m_DUI.AddComponent<CDUI>();

        // Initialise the DUI Component
        dui.Initialise(_Quality, _Layout, _Dimensions, gameObject);

        // Attach the render texture material
        dui.AttatchRenderTexture(m_ScreenObject.renderer.material);
	}
}
