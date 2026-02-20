using System;
using OCC.UI.TestingFramework.Utility;
using OCC.UI.TestingFramework.WebDriverExtensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	public enum SelectByEnum
	{
		Id,
		TestId,
		Name,
		CSS,
		XPath
	}

	/// <summary>
	/// Selects the option from the dropdown by it's text
	/// </summary>
	[Serializable]
	public abstract class PageElementBase
	{
		public string InputSelector { get; set; }

		public SelectByEnum SelectBy { get; set; }

		public PageObjectBase OwnerPage { get; set; }

		public SafeWebDriver Driver { get { return OwnerPage.Driver; } }

		public PageElementBase(PageObjectBase owner, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
		{
			InputSelector = inputSelector;
			SelectBy = selectBy;
			OwnerPage = owner;
		}

		public bool IsVisible()
		{
			try
			{
				var el = OwnerPage.Driver.FindElement(GetBy());
				return el != null;
			}
			catch
			{
				return false;
			}

		}

		public bool IsReadOnly()
		{
			try
			{
				var el = OwnerPage.Driver.FindElement(GetBy());
				return el.IsReadOnly();
			}
			catch (Exception) { }

			return false;
		}

		public By GetBy()
		{
			switch (SelectBy)
			{
				case SelectByEnum.Id:
					return By.Id(InputSelector.Replace("#", ""));
				case SelectByEnum.TestId:
					return By.CssSelector($"[data-uitesting-id=\"{InputSelector}\"]");
				case SelectByEnum.Name:
					return By.ClassName(InputSelector);
				case SelectByEnum.CSS:
					return By.CssSelector(InputSelector);
				case SelectByEnum.XPath:
					return By.XPath(InputSelector);
				default:
					throw new NotImplementedException("Get by not specified");
			};
		}

		private string GetIdByLocationName(string prefix, string name)
		{
			var inputName = GetNameByLocationName(prefix, name);
			var inputId = inputName.Replace('.', '_').Replace('[', '_').Replace(']', '_');
			return inputId;
		}

		private string GetNameByLocationName(string prefix, string name)
		{
			var inputName = prefix + "." + name;
			return inputName;
		}

		internal string GetCssSelector(PageObjectBase page, string locationName)
		{
			if (!string.IsNullOrWhiteSpace(InputSelector)) return InputSelector;
			if (SelectBy == SelectByEnum.Id)
			{
				return "#" + GetIdByLocationName(page.GetFullyQualifiedViewIdPrefix(), locationName);
			}
			return "name=" + GetNameByLocationName(page.GetFullyQualifiedViewIdPrefix(), locationName);

		}

		internal string GetXPath(PageObjectBase page, string locationName)
		{
			string elementSelector = XpathHelper.ToXPathSelector(GetCssSelector(page, locationName));
			var pageXpath = page.GetFullyQualifiedXpathPrefix();
			return pageXpath + "//" + elementSelector;
		}

		public void DoubleClick(IWebElement element)
		{
			var driver = (RemoteWebDriver)OwnerPage.Driver.DecoratedWebDriver;
			Actions Action = new Actions(driver);
			Action.DoubleClick(element).Perform();
		}

		public void HoverOver(IWebElement element)
		{
			var driver = (RemoteWebDriver)OwnerPage.Driver.DecoratedWebDriver;
			Actions Action = new Actions(driver);
			Action.MoveToElement(element).Perform();
		}

		public TPage CreatePage<TPage>() where TPage : PageObjectBase, new()
		{
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = Driver;
			return page;
		}

		public void WaitForLoadingScreenToClose()
		{
			Driver.Wait.Until(condition =>
			{
				try
				{
					var loadingScreen = Driver.DecoratedWebDriver.FindElement(By.CssSelector(".loader__banner"));
					if (loadingScreen.Displayed)
					{
						return false;
					}
					return true;
				}
				catch (StaleElementReferenceException)
				{
					return true;
				}

				catch (NoSuchElementException)
				{
					return true;
				}
			});
		}
	}
}