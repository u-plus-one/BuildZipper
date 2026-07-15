using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildZipper.Editor
{
	public abstract class ZipBuilder
	{
		protected readonly BuildReport buildReport;

		protected ZipBuilder(BuildReport buildReport)
		{
			this.buildReport = buildReport;
		}

		public abstract void CreateZip(string buildDirectory, string targetZip);
	} 
}
