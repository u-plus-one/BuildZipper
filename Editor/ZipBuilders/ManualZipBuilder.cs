using System;
using System.IO.Compression;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ZipCompressionLevel = System.IO.Compression.CompressionLevel;

namespace BuildZipper.Editor
{
	public class ManualZipBuilder : ZipBuilder
	{
		//							         _______rwxrwxrwx________________
		const uint UNIX_FLAGS_EXECUTABLE = 0b10000001111111110000000000000000;
		const uint UNIX_FLAGS_DEFAULT =    0b10000001101101100000000000000000;
		//						             \------/\------/\------/\------/

		public ManualZipBuilder(BuildReport report) : base(report) 
		{

		}

		public override void CreateZip(string rootPath, string targetZip)
		{
			var buildName = Path.GetFileName(rootPath);
			var executableFilePath = $"{buildName}/Contents/MacOS/{PlayerSettings.productName}";

			//Compress executable into a zip file
			ZipCompressionLevel compressionLevel = ZipCompressionLevel.Fastest;

			switch (BuildSettings.Instance.zipCompressionLevel)
            {
                case CompressionLevel.None:
                    compressionLevel = ZipCompressionLevel.NoCompression;
                    break;

                case CompressionLevel.Fastest:
                    compressionLevel = ZipCompressionLevel.Fastest;
                    break;

                case CompressionLevel.Optimal:
                    compressionLevel = ZipCompressionLevel.Optimal;
                    break;
            }

			if (Directory.Exists(rootPath))
			{
				ZipFile.CreateFromDirectory(rootPath, targetZip, compressionLevel, true);
			}
			else
			{
				//Add the .exe and the _Data directory to the zip
				using (var zip = ZipFile.Open(targetZip, ZipArchiveMode.Create))
				{
					zip.CreateEntryFromFile(rootPath, Path.GetFileName(rootPath), compressionLevel);
					var dataDir = rootPath.Replace(".exe", "_Data");
					if (Directory.Exists(dataDir))
					{
						foreach (var file in Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories))
						{
							var relativePath = Path.GetRelativePath(Directory.GetParent(rootPath).FullName, file);
							zip.CreateEntryFromFile(file, relativePath, compressionLevel);
						}
					}
				}
			}

			bool requiresExecutableFlag = buildReport.summary.platform == BuildTarget.StandaloneOSX;
			int entryCount;
			//Modify zip to set the executable attributes
			using (var zip = ZipFile.Open(targetZip, ZipArchiveMode.Update))
			{
				entryCount = zip.Entries.Count;
				if (requiresExecutableFlag)
				{
					//Set standard unix flags for all files and executable flags for the executable
					foreach (var entry in zip.Entries)
					{
						bool executable = entry.FullName == executableFilePath;
						SetUnixFlags(entry, executable ? UNIX_FLAGS_EXECUTABLE : UNIX_FLAGS_DEFAULT);
					}
				}
			}

			//Manual trickery to pretend that the zip was created on unix
			SetHostOS(rootPath, entryCount);

			//Self test
			if (requiresExecutableFlag)
			{
				CheckExecutableFlags(rootPath, executableFilePath);
			}
		}

		private static void SetHostOS(string rootPath, int entryCount)
		{
			const byte P = (byte)'P';
			const byte K = (byte)'K';
			//Minimum version needed to extract: 0x14 (20) = 2.0
			const byte V = 0x14;
			var bytes = File.ReadAllBytes(rootPath + ".zip");
			int modifiedEntries = 0;
			for(int i = 0; i < bytes.Length; i++)
			{
				if(i + 6 >= bytes.Length) break;
				if(bytes[i] == P && bytes[i + 1] == K && bytes[i + 2] == 0x01 && bytes[i + 3] == 0x02 && bytes[i + 4] == V)
				{
					bytes[i + 5] = 0x03;
					modifiedEntries++;
					//Approximate minimum central directory entry length
					i += 48;
				}
			}
			if(entryCount != modifiedEntries)
			{
				Debug.LogError($"Number of modified entries does not match actual entry count (expected: {entryCount}, modified: {modifiedEntries})");
			}
			else
			{
				Debug.Log($"Zip Host OS changed successfully ({modifiedEntries} entries modified)");
			}
			File.WriteAllBytes(rootPath + ".zip", bytes);
		}

		private static void SetUnixFlags(ZipArchiveEntry entry, uint flags)
		{
			unchecked
			{
				int i = (int)flags;
				entry.ExternalAttributes |= i;
			}
		}

		private static void CheckExecutableFlags(string rootPath, string executableFilePath)
		{
			using var zip = ZipFile.OpenRead(rootPath + ".zip");
			var attributes = zip.GetEntry(executableFilePath).ExternalAttributes;
			bool test;
			unchecked
			{
				test = ((uint)attributes & UNIX_FLAGS_EXECUTABLE) == UNIX_FLAGS_EXECUTABLE;
			}
			string base2 = Convert.ToString(zip.GetEntry(executableFilePath).ExternalAttributes, 2).PadLeft(32, '0');
			if(!test) Debug.LogError("Unix perms test failed: " + base2);
		}
	} 
}
