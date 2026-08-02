using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildZipper.Editor
{
	public abstract class ZipBuilder
	{
		public abstract void CreateZip(string buildDirectory, string targetZip, BuildReport report, string[] rootFiles);
	} 
}
