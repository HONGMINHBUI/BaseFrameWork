using OpenQA.Selenium;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	/// <summary>
	/// Represents a tab page on the page which is really is a logical section of the same page,
	/// but it is moved to a separate logical page object.
	/// </summary>
	public abstract class TabPage : PageObjectBase
	{
		public string NavigationLinkHref { get; set; }

		public PageObjectBase Activate()
		{
			return Activate<PageObjectBase>();
		}

		public T Activate<T>() where T : PageObjectBase
		{
			Driver.FindElement(By.XPath(string.Format("//*[@href='{0}']", NavigationLinkHref))).Click();
			return ParentPage as T;
		}
	}


}