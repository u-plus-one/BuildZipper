using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildZipper.Editor
{
	public enum CompressionMethod
	{
		WSL = 0,
		ZipManipulation = 1
	}

	public enum CompressionLevel
	{
		None = 0,
		Fastest = 1,
		Optimal = 2
	}

	public enum OriginalBuildOption
	{
		KeepOriginal = 0,
		Delete = 1,
		KeepEmptyDirectory = 2
	}

    public class BuildSettings : ScriptableObject
    {
		[Tooltip("Which method to use for creating the build zip file. (Windows Editor only)")]
		public CompressionMethod zipCreationMethod = CompressionMethod.WSL;
		[Tooltip("The compression level to apply when generating the zip file.")]
		public CompressionLevel zipCompressionLevel = CompressionLevel.Optimal;
		[Tooltip("Determines what happens to the original build directory that was used to create the zip file.")]
		public OriginalBuildOption originalBuildOption = OriginalBuildOption.KeepOriginal;
		[Tooltip("The time (in seconds) until the wsl process times out.")]
		public int wslProcessTimeout = 60;
		[Tooltip("If checked, prints additional debugging information about the build process.")]
		public bool verboseLogging = false;

		public static BuildSettings Instance
		{
			get
			{
				if(instance == null) Initialize();
				return instance;
			}
		}

		private static BuildSettings instance;

	    private static string ProjectAssetPath => Path.Combine("ProjectSettings", "BuildZipperSettings.asset");

	    private static void Initialize()
	    {
#if UNITY_EDITOR
		    if(!File.Exists(ProjectAssetPath))
		    {
			    CreateNewSettings();
			    Save();
		    }
		    else
		    {
			    Load();
		    }
#endif
	    }

		private static void CreateNewSettings()
		{
			instance = CreateInstance<BuildSettings>();
		}

		public static void Save()
		{
			var json = EditorJsonUtility.ToJson(Instance, true);
			File.WriteAllText(ProjectAssetPath, json);
		}

		private static void Load()
		{
			var json = File.ReadAllText(ProjectAssetPath);
			instance = CreateInstance<BuildSettings>();
			EditorJsonUtility.FromJsonOverwrite(json, instance);
		}
    }
}