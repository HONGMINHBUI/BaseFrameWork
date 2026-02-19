using System;

namespace OCC.UI.TestingFramework.WebDriverExtensions.Report
{
	internal class TestEntry
	{
		public string Action { get; set; }
		public string Parameter { get; set; }
		public string Details { get; set; }
		public bool Succeeded { get; set; }
		public DateTime DateTime { get; set; }
	}
}