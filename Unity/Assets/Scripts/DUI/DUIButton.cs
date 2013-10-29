using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

public class DUIButton : DUIElement 
{
    // Member Delegates
    public delegate void PressHandler(DUIButton _sender);
    
	// Member events
	public event PressHandler PressDown;
	public event PressHandler PressUp;
	public event PressHandler PressHold;

    // Member Fields
    private TextMesh m_textMesh;


    // Member Properties
    public string m_text
    {
        get
        {
            return m_textMesh.text;
        }
        set
        {
            m_textMesh.text = value;
        }
    }

    // Member Methods
    public void Initialise(string _text)
    {
        InitialiseText(_text);
        InitialiseBackground();
    }

    private void InitialiseText(string _text)
    {
        // Create the text object
        GameObject text = new GameObject(name + "_text");
        text.transform.parent = transform;
        text.transform.localPosition = Vector3.zero;
        text.transform.localRotation = Quaternion.identity;
        text.layer = gameObject.layer;

        // Add the mesh renderer
        MeshRenderer mr = text.AddComponent<MeshRenderer>();
        mr.material = (Material)Resources.Load("Fonts/Arial", typeof(Material));

        // Add the text mesh
        m_textMesh = text.AddComponent<TextMesh>();
        m_textMesh.fontSize = 24;
		m_textMesh.characterSize = 0.025f;
        m_textMesh.color = Color.white;
        m_textMesh.font = (Font)Resources.Load("Fonts/Arial", typeof(Font));
        m_textMesh.anchor = TextAnchor.MiddleCenter;
        m_textMesh.offsetZ = -0.01f;
        m_textMesh.text = _text;
		m_textMesh.fontStyle = FontStyle.Bold;
		
		// Get the dimensions for the text
		m_dimensions = new Vector2(new Vector2(mr.bounds.size.x, mr.bounds.size.z).magnitude, mr.bounds.size.y);
    }
	
	private void InitialiseBackground()
    {
        // Create the background
        GameObject background = new GameObject(name + "_background");
        background.transform.parent = transform;
        background.transform.localPosition = Vector3.zero;
        background.transform.localRotation = Quaternion.identity;
        background.layer = gameObject.layer;

        // Create the mesh
        Mesh backMesh = CreateButtonMesh(m_dimensions);

        // Create the material
        Material backMat = new Material(Shader.Find("Transparent/Diffuse"));
        //m_ButtonBackMat.SetTexture("_MainTex", m_ButtonTexture);
        backMat.name = background.name + "_mat";
        backMat.color = Color.black;

        // Add the mesh filter
        MeshFilter mf = background.AddComponent<MeshFilter>();
        mf.mesh = backMesh;

        // Add the mesh renderer
        MeshRenderer mr = background.AddComponent<MeshRenderer>();
        mr.material = backMat;

        // Add the mesh collider
        MeshCollider mc = background.AddComponent<MeshCollider>();
        mc.sharedMesh = backMesh;
        mc.isTrigger = true;
    }

    private Mesh CreateButtonMesh(Vector2 _dimensions)
    {
        Mesh buttonBackMesh = new Mesh();
        buttonBackMesh.name = name + "_mesh";
        buttonBackMesh.Clear();

        int numVertices = 4;
        int numTriangles = 6;

        Vector3[] vertices = new Vector3[numVertices];
        Vector2[] uvs = new Vector2[numVertices];
        int[] triangles = new int[numTriangles];

        vertices[0] = new Vector3(-(_dimensions.x * 0.5f), -(_dimensions.y * 0.5f));
        vertices[1] = new Vector3((_dimensions.x * 0.5f), -(_dimensions.y * 0.5f));
        vertices[2] = new Vector3(-(_dimensions.x * 0.5f), (_dimensions.y * 0.5f));
        vertices[3] = new Vector3((_dimensions.x * 0.5f), (_dimensions.y * 0.5f));

        uvs[0] = new Vector2(0.0f, 0.0f);
        uvs[1] = new Vector2(1.0f, 0.0f);
        uvs[2] = new Vector2(0.0f, 1.0f);
        uvs[3] = new Vector2(1.0f, 1.0f);

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;

        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;

        buttonBackMesh.vertices = vertices;
        buttonBackMesh.uv = uvs;
        buttonBackMesh.triangles = triangles;
        buttonBackMesh.RecalculateNormals();
        buttonBackMesh.RecalculateBounds();

        return (buttonBackMesh);
    }
	
	// Event handler methods
    public void OnPressDown()
    {
        if (PressDown != null)
        {
            PressDown(this);
        }
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
