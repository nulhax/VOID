using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Reflection;

public class CDUISubView : CDUIView
{
    // Member Fields

    // Member Properties
	public CDUIButton m_NavButton { get; set; }

    // Member Methods
    private void Update()
    {
        DebugRenderRects();
    }

    public void Initialise(Vector2 _Dimensions)
    {
//        // Get the normalised ratio of the subview and the subviewarea
//        Vector2 subviewRatio = StringToVector2(windowNode.Attributes["xyratio"].Value);
//        Vector2 subViewAreaRatio = _subViewAreaDimensions;
//        subviewRatio /= subviewRatio.x > subviewRatio.y ? subviewRatio.x : subviewRatio.y;
//        subViewAreaRatio /= subViewAreaRatio.x > subViewAreaRatio.y ? subViewAreaRatio.x : subViewAreaRatio.y;
//
//        Vector2 dimensions = Vector2.zero;
//        if (subViewAreaRatio.x == subviewRatio.y || subViewAreaRatio.y == subviewRatio.x)
//        {
//            dimensions.x = subViewAreaRatio.y * subviewRatio.x * _subViewAreaDimensions.x;
//            dimensions.y = subViewAreaRatio.x * subviewRatio.y * _subViewAreaDimensions.y;
//        }
//        else if (subViewAreaRatio.x == subviewRatio.x || subViewAreaRatio.y == subviewRatio.y)
//        {
//            if (subViewAreaRatio.y > subviewRatio.y)
//            {
//                dimensions.x = subViewAreaRatio.x / subviewRatio.x * _subViewAreaDimensions.x;
//                dimensions.y = subviewRatio.y / subViewAreaRatio.y * _subViewAreaDimensions.y;
//            }
//            else
//            {
//                dimensions.x = subViewAreaRatio.y / subviewRatio.y * _subViewAreaDimensions.x;
//                dimensions.y = subViewAreaRatio.x / subviewRatio.x * _subViewAreaDimensions.y;
//            }
//        }

        m_Dimensions = _Dimensions;

        // Create the navigation button
        InitialiseNavButton();
    }

    private void InitialiseNavButton()
    {
        // Add the button
        m_NavButton = AddButton("Test");
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.green, 0.005f);
    }
}
