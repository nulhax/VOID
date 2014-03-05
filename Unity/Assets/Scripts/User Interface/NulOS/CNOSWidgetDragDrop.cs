using UnityEngine;
using System.Collections;

public class CNOSWidgetDragDrop : UIDragDropItem 
{
	protected override void OnDragDropRelease(GameObject surface)
	{
		UIDragDropContainer container = surface ? NGUITools.FindInParents<UIDragDropContainer>(surface) : null;
			
		if(container != null)
		{
			CNOSWidget widget = CUtility.FindInParents<CNOSWidget>(gameObject);
			widget.ShowMainWidget();
		}

		base.OnDragDropRelease(surface);
	}
}
