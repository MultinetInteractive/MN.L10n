using System.Collections.Generic;

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

        /// <summary>
        /// Prevents the build task from running on build
        /// </summary>
        public bool PreventBuildTask { get; set; }
        /// <summary>
        /// Outputs a verbose log for L10n
        /// </summary>
        public bool ShowDetailedLog { get; set; }
        /// <summary>
        /// Downloads translation from defined language sources while building
        /// </summary>
        public bool DownloadTranslationFromSourcesOnBuild { get; set; }
        /// <summary>
        /// Defines which language identifier is the default one (Source language)
        /// </summary>
        public string SourceLanguage { get; set; }
        /// <summary>
        /// Use this to override what valid file extensions you want to use L10n for
        /// Default: ".aspx", ".ascx", ".js", ".jsx", ".cs", ".cshtml", ".ts", ".tsx", ".master", ".ashx", ".php"
        /// </summary>
        public List<string> OverrideValidExtensions { get; set; } = new List<string>();
    }
}
