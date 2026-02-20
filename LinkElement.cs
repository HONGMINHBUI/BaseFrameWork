using OpenQA.Selenium;
using System;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	[Serializable]
	public class LinkElement : PageElementBase
	{
		private readonly string linkText;

		public LinkElement(PageObjectBase page, string text)
			: base(page, default, default)
		{
			linkText = text;
		}

		public void Click()
		{
			OwnerPage.Driver.Logger?.AddLine($"{nameof(LinkElement)} [{linkText}] clicked");
			var link = OwnerPage.Driver.FindElement(By.LinkText(linkText));
			var parent = link.FindElement(By.XPath(".."));
			string value = parent.GetAttribute("class");
			if (value.Contains("disabled"))
			{
				throw new ElementNotInteractableException($"The \"{linkText}\" link is disabled");
			}
			else
			{
				link.Click();
			}
		}

		public TPage Click<TPage>() where TPage : PageObjectBase, new()
		{
			Click();

			OwnerPage.Driver.Logger?.AddLine(String.Format("{0} [{1}] activating page", "LinkElement", linkText));
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}
	}
}
