using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class DUISubView : DUIView
{
    private XmlNode m_uiXmlNode;

    public Vector2 m_dimensions     { get; set; }

    public Rect m_tabAreaRect       { get; set; }
    public Rect m_widgetAreaRect    { get; set; }

    private void Update()
    {
        DebugRenderRects();
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Test for rendering title, nav and content areas
        Vector3 start = Vector3.zero;
        Vector3 end = Vector3.zero;

        start = new Vector3(m_tabAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_tabAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_tabAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_tabAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_tabAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_tabAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_tabAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_tabAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_tabAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);



        start = new Vector3(m_widgetAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_widgetAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_widgetAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_widgetAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_widgetAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_widgetAreaRect.x * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);

        start = new Vector3(m_widgetAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.yMax * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;
        end = new Vector3(m_widgetAreaRect.xMax * m_dimensions.x - (m_dimensions.x * 0.5f), m_widgetAreaRect.y * m_dimensions.y - (m_dimensions.y * 0.5f)) + transform.position;

        Debug.DrawLine(start, end);
    }
}
