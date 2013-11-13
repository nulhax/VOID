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


public class CDUISubView : CDUIView
{
    // Member Fields
	
    // Member Properties

    // Member Methods
    private void Update()
    {
        DebugRenderRects();
    }

    public void Initialise(Vector2 _Dimensions)
    {
        m_Dimensions = _Dimensions;
    }

    // Debug Functions
    private void DebugRenderRects()
    {
        // Render self rect
        DebugDrawRect(new Rect(0.0f, 0.0f, 1.0f, 1.0f), Color.green, 0.005f);
    }
}

/***************************** CODE THAT COULD BE USEFUOL IN THE FUTURE ****************************/

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