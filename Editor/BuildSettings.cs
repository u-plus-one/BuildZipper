using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildZipper.Editor
{
	public enum CompressionMethod
	{
		ZipManipulation = 0,
		WSL = 1
	}

	public enum CompressionLevel
	{
		None = 0,
		Fastest = 1,
		Optimal = 2
	}

	public enum SourceBuildAction
	{
		KeepSource = 0,
		DeleteSource = 1,
		LeaveEmptyDirectory = 2
	}

    public class BuildSettings : ScriptableObject
    {
	    [System.Serializable]
	    public class BuildOptions
	    {
		    public bool createZip;
		    public SourceBuildAction sourceBuildAction = SourceBuildAction.KeepSource;
	    }
	    
	    [System.Serializable]
	    public class PerPlatformBuildOptions
	    {
		    public BuildOptions windows = new BuildOptions();
		    public BuildOptions macOS = new BuildOptions();
		    public BuildOptions linux = new BuildOptions();
		    
		    [Space]
		    public BuildOptions android = new BuildOptions();
		    public BuildOptions iOS = new BuildOptions();
		    
		    [Space]
		    public BuildOptions webGL = new BuildOptions();
		    
		    [Space]
		    public BuildOptions other = new BuildOptions();
		    
		    public IEnumerable<BuildOptions> AllOptions
		    {
			    get
			    {
				    yield return windows;
				    yield return macOS;
				    yield return linux;
				    yield return android;
				    yield return iOS;
				    yield return webGL;
				    yield return other;
			    }
		    }
		    
		    public BuildOptions Get(BuildTarget platform)
		    {
			    switch(platform)
			    {
				    case BuildTarget.StandaloneWindows:
				    case BuildTarget.StandaloneWindows64:
					    return windows;
				    case BuildTarget.StandaloneOSX:
					    return macOS;
				    case BuildTarget.StandaloneLinux64:
				    case BuildTarget.EmbeddedLinux:
					    return linux;
				    case BuildTarget.Android:
					    return android;
				    case BuildTarget.iOS:
					    return iOS;
				    case BuildTarget.WebGL:
					    return webGL;
				    default:
					    return other;
			    }
		    }
	    }
	    
	    [Tooltip("Platform-specific settings for the build zipper.")]
	    public PerPlatformBuildOptions perPlatformOptions = new PerPlatformBuildOptions();
		[Tooltip("Which method to use for creating the build zip file. (Windows Editor only)")]
		public CompressionMethod zipCreationMethod = CompressionMethod.ZipManipulation;
		[Tooltip("The compression level to apply when generating the zip file.")]
		public CompressionLevel zipCompressionLevel = CompressionLevel.Optimal;
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