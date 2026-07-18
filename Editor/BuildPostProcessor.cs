using System;
using System.IO;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using System.IO.Compression;
using ZipCompressionLevel = System.IO.Compression.CompressionLevel;
using System.Diagnostics;

namespace BuildZipper.Editor
{
	public class BuildPostProcessor : IPostprocessBuildWithReport
	{
		public int callbackOrder => int.MaxValue;

		public void OnPostprocessBuild(BuildReport report)
		{
			if(BuildSettings.Instance.perPlatformOptions.Get(report.summary.platform).createZip)
			{
				string sourceDir = report.summary.outputPath;
				string zipFileName;
				if(Directory.Exists(sourceDir)) zipFileName = sourceDir + ".zip";
				else zipFileName = Directory.GetParent(sourceDir) + ".zip";

				//Delete existing zip if present
				if(File.Exists(zipFileName))
				{
					File.Delete(zipFileName);
				}

#if UNITY_EDITOR_WIN
				//In case of windows, use either windows subsystem for linux (WSL) or manual zip manipulation
				ZipBuilder builder;
				if(BuildSettings.Instance.zipCreationMethod == CompressionMethod.WSL)
				{
					builder = new WSLZipBuilder(report);
				}
				else if(BuildSettings.Instance.zipCreationMethod == CompressionMethod.ZipManipulation)
				{
					builder = new ManualZipBuilder(report);
				}
				else
				{
					throw new System.NotImplementedException();
				}
				VerboseLog($"Using {builder.GetType().Name} to create Zip ...");
				builder.CreateZip(sourceDir, zipFileName);
#else
				//In case of MacOS / Linux, just create a normal zip without modifying it
				//TODO: Test MacOS/Linux zipping code
				//CreateZip(sourceDir, zipFileName);

				VerboseLog("Checking if the 'zip' package is installed");
				
				if (!CheckCommandAvailableErrorEquals("which", "zip", string.Empty))
				{
					throw new Exception("Zip package not installed. Please make sure you install zip before using this code");
				}
				VerboseLog("Zip package installed");

                string buildName = Path.GetFileName(sourceDir);
                string buildFolder = Directory.GetParent(sourceDir).FullName;

                float compressionLevel = 6;

                switch (BuildSettings.Instance.zipCompressionLevel)
                {
                    case CompressionLevel.None:
                        compressionLevel = 0;
                        break;

                    case CompressionLevel.Fastest:
                        compressionLevel = 1;
                        break;

                    case CompressionLevel.Optimal:
                        compressionLevel = 9;
                        break;
                }

                var process = new Process()
				{
					StartInfo = new ProcessStartInfo()
					{
						FileName = "bash",
						Arguments = $"-c \"cd {buildFolder} && zip -{compressionLevel} -r {buildName}.zip {buildName}\"",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true,
					},
					EnableRaisingEvents = true,
				};
				process.OutputDataReceived += Process_OutputDataReceived;
				process.ErrorDataReceived += Process_ErrorDataReceived;
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				process.WaitForExit(BuildSettings.Instance.wslProcessTimeout * 1000);
				if (!process.HasExited)
				{
					process.Kill();
				}
#endif

				if (File.Exists(zipFileName))
				{
					VerboseLog($"Build zip created successfully at {zipFileName}");
				}
				else
				{
					UnityEngine.Debug.LogError("Build zip creation failed.");
				}

				//Perform actions on the build directory based on project setting
				CleanSourceDirectory(report.summary.platform, sourceDir);
			}
		}

		private void CleanSourceDirectory(BuildTarget target, string sourceDir)
		{
			var sourceBuildAction = BuildSettings.Instance.perPlatformOptions.Get(target).sourceBuildAction;
			if(sourceBuildAction == SourceBuildAction.LeaveEmptyDirectory)
			{
				VerboseLog("Clearing original build directory ...");
				//Delete contents of the build directory but keep the directory itself
				foreach(var dir in Directory.GetDirectories(sourceDir))
				{
					Directory.Delete(dir, true);
				}
				foreach(var file in Directory.GetFiles(sourceDir))
				{
					File.Delete(file);
				}
			}
			else if(sourceBuildAction == SourceBuildAction.DeleteSource)
			{
				VerboseLog("Deleting original build directory ...");
				Directory.Delete(sourceDir, true);
			}
		}

		private void CreateZip(string folder, string zip)
		{
			ZipCompressionLevel compressionLevel;
			if(BuildSettings.Instance.zipCompressionLevel == CompressionLevel.Optimal) compressionLevel = ZipCompressionLevel.Optimal;
			else if(BuildSettings.Instance.zipCompressionLevel == CompressionLevel.Fastest) compressionLevel = ZipCompressionLevel.Fastest;
			else compressionLevel = ZipCompressionLevel.NoCompression;
			ZipFile.CreateFromDirectory(folder, zip, compressionLevel, true);
		}

		public static void VerboseLog(string message)
		{
			if(BuildSettings.Instance.verboseLogging)
			{
				UnityEngine.Debug.Log(message);
			}
		}

        public static bool CheckCommandAvailableErrorContains(string commandFileName, string commandArguments, string outputContainsError)
        {
            string output = GetCommandOutput(commandFileName, commandArguments);

            VerboseLog($"Checking output\n{output}");

            if (output.Contains(outputContainsError))
            {
                return false;
            }
            return true;
        }

        public static bool CheckCommandAvailableErrorEquals(string commandFileName, string commandArguments, string outputEqualsError)
        {
            string output = GetCommandOutput(commandFileName, commandArguments);

            VerboseLog($"Checking output\n{output}");

            if (output.Equals(outputEqualsError))
            {
                return false;
            }
            return true;
        }

        public static string GetCommandOutput(string commandFileName, string commandArguments)
        {
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = commandFileName,
                    Arguments = commandArguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            return output.Replace("\0", string.Empty);
        }

        public static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                VerboseLog(e.Data);
        }

        public static void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                UnityEngine.Debug.LogError(e.Data);
        }
    }
}
