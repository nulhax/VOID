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

[RequireComponent(typeof(CDUIInteraction))]
public class CDUIConsole : MonoBehaviour 
{
	// Retarded public fields for the editor
	public GameObject m_ScreenObject = null;
	
    // Member Fields 
	private GameObject m_DUI = null;
	
	static float s_UIOffset = 0.0f;
	
    // Member Properties
	public CDUI DUI 
	{ 
		get { return(m_DUI.GetComponent<CDUI>()); } 
	}
	
	public GameObject DUIGameObject 
	{ 
		get { return(m_DUI); } 
	}
	
	// Member Methods
	public void Start()
	{
		// Create the DUI object
		CreateDUI();

		// Attach the render texture material
        DUI.AttatchRenderTexture(m_ScreenObject.renderer.material);
	}
	
    private void CreateDUI()
	{
		// Create the DUI game object
        m_DUI = new GameObject();
        m_DUI.name = name + "_DUI";
        m_DUI.layer = LayerMask.NameToLayer("DUI");
		m_DUI.transform.rotation = Quaternion.identity;
        m_DUI.transform.position = new Vector3(0.0f, 0.0f, s_UIOffset);
		
        // Add the DUI component
        CDUI dui = m_DUI.AddComponent<CDUI>();
		dui.Console = gameObject;
		
		// Increment the offset
		s_UIOffset += 2.0f;
	}
	
	private void OnDestroy()
	{
		Destroy(m_DUI);
	}
}
