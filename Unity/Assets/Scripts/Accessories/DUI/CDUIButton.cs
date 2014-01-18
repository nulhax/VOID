//  Auckland
//  New Zealand
//
//  (c) 2013 VOID
//
//  File Name   :   CActorMotor.cs
//  Description :   --------------------------
//
//  Author      :  Programming Team
//  Mail        :  contanct@spaceintransit.co.nz
//


// Namespaces
using UnityEngine;
using System.Collections;


/* Implementation */


public class CDUIButton : CDUIElement 
{
    // Member Delegates
    public delegate void PressHandler(CDUIButton _sender);
    
	// Member events
	public event PressHandler PressDown;
	public event PressHandler PressUp;
	public event PressHandler PressHold;

    // Member Fields
	private GameObject m_TextField = null;
	
	static private string s_PressDownSoundFile = "Audio/DUI/ButtonPress";
	static private AudioClip s_PressDownSound = null;
	
    // Member Properties
    public string Text
    {
        get
        {
            return m_TextField.GetComponent<TextMesh>().text;
        }
        set
        {
            m_TextField.GetComponent<TextMesh>().text = value;
			RecalculateDimensions();
        }
    }

    // Member Methods
	public void Awake()
	{
		ElementType = CDUIElement.EElementType.Button;
		
		if(s_PressDownSound == null)
		{
			s_PressDownSound = (AudioClip)Resources.Load(s_PressDownSoundFile, typeof(AudioClip));
		}
	}
	
    public void Initialise(string _text)
    {
        InitialiseText(_text);
        InitialiseBackground();
    }

    private void InitialiseText(string _text)
    {
        // Create the text field object
        m_TextField = new GameObject(name + "_Text");
        m_TextField.transform.parent = transform;
        m_TextField.transform.localPosition = Vector3.zero;
        m_TextField.transform.localRotation = Quaternion.identity;
        m_TextField.layer = gameObject.layer;

        // Add the mesh renderer
        MeshRenderer mr = m_TextField.AddComponent<MeshRenderer>();
        mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        // Add the text mesh
        TextMesh textMesh = m_TextField.AddComponent<TextMesh>();
        textMesh.fontSize = 96;
		textMesh.characterSize = 0.00625f;
        textMesh.color = Color.white;
        textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.offsetZ = -0.01f;
		textMesh.fontStyle = FontStyle.Bold;
		Text = _text;
    }
	
	private void InitialiseBackground()
    {
        // Create the mesh
        Mesh backMesh = CreatePlaneMesh(m_Dimensions);

        // Create the material
        Material backMat = new Material(Shader.Find("Transparent/VertexLit"));
        backMat.name = gameObject.name + "_mat";
        backMat.color = Color.clear;

        // Add the mesh filter
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = backMesh;

        // Add the mesh renderer
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = backMat;

        // Add the mesh collider
        MeshCollider mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = backMesh;
        mc.isTrigger = true;
    }
	
	private void RecalculateDimensions()
	{
		m_Dimensions = m_TextField.GetComponent<MeshRenderer>().bounds.size;
	}
	
	// Event handler methods
    public void OnPressDown()
    {
        if (PressDown != null)
        {
            PressDown(this);
        }
		
		AudioSource conosleAudio = transform.parent.parent.GetComponent<CDUI>().Console.GetComponent<AudioSource>();
		conosleAudio.clip = s_PressDownSound;
		
		AudioSystem.Play(conosleAudio, 1.0f, 1.0f, false, 0.0f, AudioSystem.SoundType.SOUND_EFFECTS, false);
    }
	
	public void OnPressUp()
    {
        if (PressUp != null)
        {
            PressUp(this);
        }
    }
	
	public void OnPressHold()
    {
        if (PressHold != null)
        {
            PressHold(this);
        }
    }

        //// Set the position (optional)
    //if (_xButton.Attributes["pos"] != null)
    //{
    //    Vector3 pos = DUIView.StringToVector3(_xButton.Attributes["pos"].Value);
    //    transform.localPosition = new Vector3(pos.x * _viewWidth - (_viewWidth * 0.5f), pos.y * _viewHeight - (_viewHeight * 0.5f));
    //}

    //// Set the text (optional)
    //if (_xButton.Attributes["text"] != null)
    //    GetComponentInChildren<TextMesh>().text = _xButton.Attributes["text"].Value;

    //// Find the events handle
    //foreach (XmlNode xEvent in _xButton.SelectNodes("event"))
    //{
    //    string eventName = string.Empty;

    //    if (xEvent.Attributes["name"] != null)
    //        eventName = xEvent.Attributes["name"].Value;
    //    else
    //    {
    //        Debug.LogError(string.Format("DUI: XML Button event attribute [name] not found!"));
    //        Debug.Break();
    //    }

    //    // Find the event
    //    EventInfo ei = typeof(DUIButton).GetEvent(eventName);
    //    if (ei == null)
    //    {
    //        Debug.LogError(string.Format("DUIButton: Event [{0}] not found within this component!", eventName), gameObject);
    //        Debug.Break();
    //    }

    //    // Find the actions to register to events
    //    foreach (XmlNode xAction in xEvent.SelectNodes("action"))
    //    {
    //        string targetName = string.Empty;
    //        string componentName = string.Empty;
    //        string actionName = string.Empty;

    //        if (xAction.Attributes["target"] != null)
    //            targetName = xAction.Attributes["target"].Value;

    //        if (xAction.Attributes["component"] != null)
    //            componentName = xAction.Attributes["component"].Value;

    //        if (xAction.Attributes["method"] != null)
    //            actionName = xAction.Attributes["method"].Value;

    //        // Find the game object target
    //        GameObject targetGo = null;
    //        switch (targetName)
    //        {
    //            case "{DUI}":
    //                targetGo = transform.parent.parent.gameObject;
    //                break;

    //            default:
    //                targetGo = GameObject.Find(targetName);
    //                break;
    //        }
    //        if (targetGo == null)
    //        {
    //            Debug.LogError(string.Format("DUI: Target [{0}] not found!", targetName));
    //            Debug.Break();
    //        }

    //        // Find the component
    //        Component component = targetGo.GetComponent(componentName);
    //        if (component == null)
    //        {
    //            Debug.LogError(string.Format("DUIButton: Component [{0}] not found within target [{1}]!", componentName, targetName), targetGo);
    //            Debug.Break();
    //        }

    //        // Find the method
    //        MethodInfo mi = System.Type.GetType(componentName).GetMethod(actionName);
    //        if (mi == null)
    //        {
    //            Debug.LogError(string.Format("DUIButton: Action [{0}] not found within component [{1}], within target [{2}]! Perhaps it is not set to public", actionName, componentName, targetName), targetGo);
    //            Debug.Break();
    //        }

    //        // Register the action on the target
    //        ei.AddEventHandler(this, System.Delegate.CreateDelegate(typeof(System.Action), component, mi));
    //    }
    //}
}
