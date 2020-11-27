using System;

namespace MN.L10n.BuildTasks
{
    class L10nBuildCache
    {
        public DateTimeOffset BuildStarted { get; set; }
        public DateTimeOffset? BuildFinished { get; set; }
        public long FoundPhrases { get; set; }
        public long ScannedFiles { get; set; }
    }
}
