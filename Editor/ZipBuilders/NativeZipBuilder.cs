using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;
using static BuildZipper.Editor.BuildPostProcessor;

namespace BuildZipper.Editor
{
	public class NativeZipBuilder : ZipBuilder
	{
		public override void CreateZip(string sourcePath, string[] filesToInclude, string outputZipPath, BuildReport report)
		{
			VerboseLog("Checking if the 'zip' package is installed");

			if (!CheckCommandAvailableErrorEquals("which", "zip", string.Empty))
			{
				throw new Exception("Zip package not installed. Please make sure you install zip before using this code");
			}
			VerboseLog("Zip package installed");

			string buildFolder = sourcePath;

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

			string filesArgs = string.Join(" ", filesToInclude.Select(f => f.Replace(" ", @"\ ")));
			var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "bash",
					Arguments = $"-c \"cd {buildFolder} && zip -{compressionLevel} -r {outputZipPath} {filesArgs}\"",
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
		}
	}
}