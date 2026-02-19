namespace OCC.UI.TestingFramework.WebDriverExtensions.Report
{
	internal class PageDump
	{
		internal string Label { get; set; }
		internal string Description { get; set; }
		internal byte[] ScreenDump { get; set; }
		internal string HtmlDump { get; set; }
	}
}