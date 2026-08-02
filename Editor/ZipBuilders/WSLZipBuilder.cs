using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static BuildZipper.Editor.BuildPostProcessor;

namespace BuildZipper.Editor
{
	public class WSLZipBuilder : ZipBuilder
	{
		public WSLZipBuilder()
		{
		}

		public override void CreateZip(string sourcePath, string[] filesToInclude, string outputZipPath, BuildReport report)
		{
			#region Variables
			bool previousBuildRenamed = false;
			bool oldBuildRecycled = false;

			string buildFolder = sourcePath;
			string buildFolderWsl = SanitizeLinuxPath(buildFolder);

			string buildName = Path.GetFileName(outputZipPath);
			buildName = buildName.Replace(" ", @"\ ");
			
			string outputZipPathWsl = SanitizeLinuxPath(outputZipPath);

			string productName = Application.productName;
			#endregion

			#region WSL installed check
			VerboseLog("Checking if WSL is installed");

			if (!CheckCommandAvailableErrorContains("wsl", "--list", "--install"))
			{
				throw new Exception("WSL has no installed distributions. For more information, go to https://learn.microsoft.com/windows/wsl/install");
			}
			VerboseLog("WSL installed");
			#endregion

			#region zip installed check
			VerboseLog("Checking if the 'zip' package is installed in WSL");

			if (!CheckCommandAvailableErrorEquals("wsl", "-e which zip", string.Empty))
			{
				throw new Exception("Zip package not installed on WSL. Please make sure you install zip before using this code. To do so, run 'sudo apt install zip' in WSL");
			}
			VerboseLog("Zip package installed");
			#endregion

			#region Zipping process
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
			
			// Escape spaces for WSL
			string filesArgs = string.Join(" ", filesToInclude.Select(f => f.Replace(" ", @"\ ")));

			var process = new Process()
			{
				StartInfo = new ProcessStartInfo()
				{
					FileName = "wsl",
					Arguments = $"-e bash -c \"cd {buildFolderWsl} && zip -{compressionLevel} -r {outputZipPathWsl} {filesArgs}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				},
				EnableRaisingEvents = true,
			};
			VerboseLog($"Running WSL zip command: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
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
			#endregion
		}

		private string SanitizeLinuxPath(string path)
		{
			char driveLetter = path[0];
			var linuxPath = path;
			if (linuxPath[1].Equals(':'))
			{
				linuxPath = linuxPath.Substring(2);
				linuxPath = "/mnt/" + char.ToLower(driveLetter).ToString() + linuxPath;
			}
			linuxPath = linuxPath.Replace("\\", "/");
			linuxPath = linuxPath.Replace(" ", @"\ ");
			return linuxPath;
		}
	}
}
