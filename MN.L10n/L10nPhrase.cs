using System;

namespace MN.L10n
{
	public class L10nPhrase
	{
		public string Comment { get; set; }
		public DateTime Created { get; set; } = DateTime.Now;
		public long Usages { get; set; } = 1;
		public string LatestBuildUsage { get; set; }
	}
}