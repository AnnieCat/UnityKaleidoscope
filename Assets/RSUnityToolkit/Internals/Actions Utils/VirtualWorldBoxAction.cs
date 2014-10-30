/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RSUnityToolkit;

/// <summary>
/// Virtual world box action draws gizmos when needed
/// </summary>
public abstract class VirtualWorldBoxAction : BaseAction {
	
	#region Public Fields
	
	/// <summary>
	/// The virtual world box center.
	/// </summary>
	[HideInInspector]
	public Vector3 VirtualWorldBoxCenter = new Vector3(0,0,50);
	
	/// <summary>
	/// The virtual world box dimensions.
	/// </summary>
	public Vector3 VirtualWorldBoxDimensions = new Vector3(100,100,100);	
	
	#endregion
	
	#region Private Fields
	
	//On Start
	new void Start()
	{
		base.Start();
		
		VirtualWorldBoxCenter = gameObject.transform.localPosition;
	}
	
	// used to draw gizmos
	private void OnDrawGizmosSelected()
	{
		if (transform.parent != null)
		{
			Gizmos.matrix = transform.parent.localToWorldMatrix;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(VirtualWorldBoxCenter, VirtualWorldBoxDimensions);
    }

	#endregion
}
 
