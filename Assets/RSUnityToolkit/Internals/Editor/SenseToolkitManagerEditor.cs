/*******************************************************************************

INTEL CORPORATION PROPRIETARY INFORMATION
This software is supplied under the terms of a license agreement or nondisclosure
agreement with Intel Corporation and may not be copied or disclosed except in
accordance with the terms of that agreement
Copyright(c) 2012-2014 Intel Corporation. All Rights Reserved.

*******************************************************************************/
using UnityEngine;
using System.Collections; 
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using RSUnityToolkit;

[CanEditMultipleObjects]
[CustomEditor(typeof(SenseToolkitManager), true)]
public class SenseToolkitManagerEditor: Editor
{
	private bool _speechFoldoutOpen;
	
    #region Unity's override methods
 
	// This function is called every editor gui update. In here we are diong our magic to show all to the user in a nice way.
	public override void OnInspectorGUI()
    { 	
		//SenseToolkitManager SenseToolkitManager = (SenseToolkitManager)target;
		
		DrawDefaultInspector();
				
		_speechFoldoutOpen = GUIUtils.Foldout(_speechFoldoutOpen, new GUIContent("Speech Settings"));

		if ( _speechFoldoutOpen )
		{
			EditorGUI.indentLevel++;
			
			SenseToolkitManager.SpeechManager.ActiveSource = EditorGUILayout.Popup("Available Sources: ", SenseToolkitManager.SpeechManager.ActiveSource, SenseToolkitManager.SpeechManager.AvailableSources);
			
			SenseToolkitManager.SpeechManager.ActiveModule = EditorGUILayout.Popup("Available Modules: ", SenseToolkitManager.SpeechManager.ActiveModule, SenseToolkitManager.SpeechManager.AvailableModules);
			
			SenseToolkitManager.SpeechManager.ActiveLanguage = EditorGUILayout.Popup("Available Languages: ", SenseToolkitManager.SpeechManager.ActiveLanguage, SenseToolkitManager.SpeechManager.AvailableLanguages);

			EditorGUI.indentLevel--;
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty(serializedObject.targetObject);
		}
		
	}
	
	#endregion
	
}
 
