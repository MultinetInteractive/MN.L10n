using System;
using System.Collections.Generic;
using System.Text;

namespace MN.L10n.BuildTasks
{
	public class L10nConfig
	{
		/// <summary>
		/// Glob-patterns för att definiera vilka kataloger/filer som L10n ska jobba med
		/// </summary>
		public List<string> IncludePatterns { get; set; } = new List<string>();
		/// <summary>
		/// Glob-patterns för att definiera vilka kataloger/filer som L10n ska ignorera
		/// </summary>
		public List<string> ExcludePatterns { get; set; } = new List<string>();
        /// <summary>
        /// Copies the L10n files to the following folders, if not specified copies to the projectFolder that has the Nuget reference to L10n
        /// </summary>
	    public List<string> CopyFilesTo { get; set; }

		public bool PreventBuildTask { get; set; }
		public bool ShowDetailedLog { get; set; }
	}
}
