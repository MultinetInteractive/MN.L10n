using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MN.L10n.NullProviders;

namespace MN.L10n.Test
{
	[TestClass]
	public class L10nTests
	{
		public static L10n Instance;
		public void InitInstance()
		{
			if(Instance == null)
				Instance = L10n.CreateInstance(new NullLanguageProvider(), new NullDataProvider());
		}

		[TestMethod]
		public void TestLanguages()
		{
			InitInstance();
			Assert.AreEqual(1, Instance.Languages.Count);
		}
	}
}
