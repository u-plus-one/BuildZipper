using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildZipper.Editor
{
	public abstract class ZipBuilder
	{
		public abstract void CreateZip(string sourcePath, string[] filesToInclude, string outputZipPath, BuildReport report);
	} 
}
