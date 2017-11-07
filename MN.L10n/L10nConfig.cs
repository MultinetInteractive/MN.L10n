using System.Collections.Generic;

namespace MN.L10n
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
        public bool PreventBuildTask { get; set; }
    }
}
