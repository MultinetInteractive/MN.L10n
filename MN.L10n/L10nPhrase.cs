using System;
using System.Collections.Generic;

namespace MN.L10n
{
	public class L10nPhrase
	{
		public string Comment { get; set; }
		public DateTime Created { get; set; } = DateTime.Now;
		public long Usages { get; set; } = 1;
		public List<string> Sources { get; set; } = new List<string>();
	}
}