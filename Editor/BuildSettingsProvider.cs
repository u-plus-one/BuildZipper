using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildZipper.Editor
{
	public static class BuildSettingsProvider
	{
		private static SettingsProvider provider;

		[SettingsProvider]
		internal static SettingsProvider Register()
		{
			provider = new SettingsProvider("Project/Build Zipper Settings", SettingsScope.Project)
			{
				guiHandler = OnGUI,
				deactivateHandler = OnClose
			};
			return provider;
		}

		private static void OnGUI(string search)
		{
			var width = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 200;
			var so = new SerializedObject(BuildSettings.Instance);
			so.Update();
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.zipCreationMethod)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.zipCompressionLevel)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.originalBuildOption)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.wslProcessTimeout)), new GUIContent("WSL Process Timeout"));
			GUILayout.Space(20);
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.verboseLogging)));
			if(so.ApplyModifiedProperties())
			{
				BuildSettings.Save();
			}
			EditorGUIUtility.labelWidth = width;
		}

		private static void OnClose()
		{
			BuildSettings.Save();
		}
	} 
}
