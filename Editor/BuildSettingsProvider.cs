using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BuildZipper.Editor
{
	public static class BuildSettingsProvider
	{
		private static SettingsProvider provider;
		private static GUIContent createZipDesc = new GUIContent("Create Zip", "Whether to create a zip file for this target platform.");
		private static GUIContent sourceBuildActionHeader = new GUIContent("Source Build Action", "Determines what happens to the original build directory that was used to create the zip file.");

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
			var options = so.FindProperty(nameof(BuildSettings.perPlatformOptions));
			GUILayout.BeginVertical(GUI.skin.box);
			var lastLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 100;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Target Platform", EditorStyles.boldLabel, GUILayout.Width(lastLabelWidth));
			GUILayout.Label(createZipDesc, EditorStyles.boldLabel, GUILayout.Width(70));
			GUILayout.Label(sourceBuildActionHeader, EditorStyles.boldLabel, GUILayout.Width(150));
			GUILayout.EndHorizontal();
			foreach (SerializedProperty option in GetDirectChildren(options))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(option.displayName, GUILayout.Width(lastLabelWidth));
				var createZipProp = option.FindPropertyRelative(nameof(BuildSettings.BuildOptions.createZip));
				var sourceBuildActionProp = option.FindPropertyRelative(nameof(BuildSettings.BuildOptions.sourceBuildAction));
				EditorGUILayout.PropertyField(createZipProp, GUIContent.none, GUILayout.Width(70));
				GUI.enabled = createZipProp.boolValue;
				EditorGUILayout.PropertyField(sourceBuildActionProp, GUIContent.none, GUILayout.Width(150));
				GUI.enabled = true;
				GUILayout.EndHorizontal();
			}
			EditorGUIUtility.labelWidth = lastLabelWidth;
			GUILayout.EndVertical();
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.zipCreationMethod)));
			EditorGUILayout.PropertyField(so.FindProperty(nameof(BuildSettings.zipCompressionLevel)));
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
		
		private static IEnumerable<SerializedProperty> GetDirectChildren(SerializedProperty parent) {
			int dots = parent.propertyPath.Count(c => c == '.');
			foreach (SerializedProperty inner in parent) {
				bool isDirectChild = inner.propertyPath.Count(c => c == '.') == dots + 1;
				if (isDirectChild)
					yield return inner;
			}
		}
	} 
}
