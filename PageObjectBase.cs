using OCC.UI.TestingFramework.Utility;
using OCC.UI.TestingFramework.Utility.Reflection;
using OCC.UI.TestingFramework.WebDriverExtensions;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	/// <summary>
	/// The base class for all pages
	/// </summary>
	public class PageObjectBase
	{
		#region Public properties

		public SafeWebDriver Driver { get; set; }
		public string PageTitle { get { return Driver.Title; } }
		public PageObjectBase ParentPage { get; set; }
		
		#endregion
		#region Private fields
		private PageSettings pageSettings;
		#endregion
		#region Constructor

		public PageObjectBase()
		{
			pageSettings = GetType().GetCustomAttributes(true).OfType<PageSettings>().FirstOrDefault();
			if (pageSettings == null)
				pageSettings = new PageSettings("");
		}

		public PageObjectBase(PageObjectBase parent)
		{
			ParentPage = parent;
		}

		#endregion
		#region Id/Xpath generation helper methods

		/// TODO: refactor to recursive function
		public string GetFullyQualifiedPageObjectPrefix()
		{
			return GetPrefix(p => p.GetPageObjectPrefix());
		}

		//TODO: refactor to recursive function
		public string GetFullyQualifiedViewIdPrefix()
		{
			return GetPrefix(p => p.GetViewIdPrefix());
		}

		private string GetPrefix(Func<PageObjectBase,string> action)
		{
			var path = new List<string>();
			PageObjectBase actPage = this;
			while (actPage != null)
			{
				var prefix = action(actPage);
				if (!string.IsNullOrWhiteSpace(prefix))
					path.Add(prefix);
				actPage = actPage.ParentPage;
			}
			path.Reverse();
			var result = string.Join(".", path);
			return result.TrimStart('.');
		}

		/// <summary>
		/// Returns prefix for the Xpath selector path
		/// </summary>
		/// <returns></returns>
		public virtual string GetFullyQualifiedXpathPrefix()
		{
			var path = new List<string>();
			PageObjectBase actPage = this;
			while (actPage != null)
			{
				var containerSelector = actPage.GetContainerSelector();
				if (!string.IsNullOrWhiteSpace(containerSelector))
				{
					var xpathPart = XpathHelper.ToXPathSelector(containerSelector);
					path.Add(xpathPart);
				}
				actPage = actPage.ParentPage;
			}
			path.Reverse();
			if(path.Count>0)
				return ".//" + string.Join("//", path);
			return ".";
		}

		public bool HideViewIdPrefix { get; set; }

		public virtual string GetViewIdPrefix()
		{
			if (HideViewIdPrefix)
				return "";

			pageSettings = GetType().GetCustomAttributes(true).OfType<PageSettings>().FirstOrDefault();
			return pageSettings!=null? pageSettings.ViewPrefix:"";
		}

		public virtual string GetPageObjectPrefix()
		{
			pageSettings = GetType().GetCustomAttributes(true).OfType<PageSettings>().FirstOrDefault();
			return pageSettings!=null? pageSettings.PageObjectPrefix:"";
		}

		public virtual string GetContainerSelector()
		{
			return pageSettings!=null? pageSettings.ContainerSelector :"";
		}

		/// <summary>Clicks the button whose HTML value property matches the provided value.</summary>
		/// <param name="valueAttributeValue"></param>
		/// <returns></returns>
		public ReturnPageType ClickButton<ReturnPageType>(string valueAttributeValue) where ReturnPageType : PageObjectBase, new()
		{
			return NavigateTo<ReturnPageType>(By.XPath(string.Format("/descendant::input[attribute::value=\"{0}\"]", valueAttributeValue)));
		}

		#endregion

		public RDPMessageDialogPage RDPMessageDialogPage
		{
			get { return new RDPMessageDialogPage(Driver); }
		}

		public TPage NavigateTo<TPage>(By by) where TPage : PageObjectBase, new()
		{
			Driver.FindElement(by).Click();
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = Driver;
			return page;
		}

		public TPage NavigateTo<TPage>(IWebElement element) where TPage : PageObjectBase, new()
		{
			WaitForLoadingScreenToClose();
			element.Click();
			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = Driver;
			return page;
		}

		/// <summary>
		/// Use in case when multiple links like :"Edit", "View" etc are placed in table.
		/// </summary>
		/// <param name="linkText">Text of the link to search e.g "Edit", "View" etc</param>
		/// <param name="index">0 based index of link to click.</param>
		/// <returns>New Page object of type TPage</returns>
		public TPage NavigateToPageByClickOneOfManyLinks<TPage>(string linkText, int index) where TPage : PageObjectBase, new()
		{
			return NavigateToPageByClickOneOfManyItems<TPage>("a", linkText, index);
		}

		public TPage NavigateToPageByClickOneOfManyButtons<TPage>(string linkText, int index) where TPage : PageObjectBase, new()
		{
			return NavigateToPageByClickOneOfManyItems<TPage>("input.button", linkText, index);
		}

		private TPage NavigateToPageByClickOneOfManyItems<TPage>(string selector, string itemText, int index) where TPage : PageObjectBase, new()
		{
			var list = Driver.FindElements(By.CssSelector(selector));
			List<IWebElement> links = new List<IWebElement>();

			links = list.Where(x => ( x.GetAttribute("Value") ?? x.Text).Trim().ToLower() == itemText.Trim().ToLower()).ToList();

			if (links.Count > index)
				links[index].Click();
			else
				throw new Exception($"Invalid number of elements. Items founded:{links.Count} Expected index {index}");

			var page = Activator.CreateInstance<TPage>();
			page.Driver = Driver;
			return page;
		}

		/// <summary>
		/// Controcc Web Specific added by MDT -
		/// Used to go through all li items in a ul parent (e.g. the File menu) and choose the element that matches the input string. 
		/// </summary>
		/// <param name="unorderedList">The ul parent item to be iterated across.</param>
		/// <param name="itemToFind">The text of the li item that needs to be returned.</param>
		/// <returns>The Web Element to be interacted with.</returns>
		public IWebElement UnorderedListIterator(By unorderedList, string itemToFind)
		{
			IWebElement unorderedListElements = Driver.FindElement(unorderedList);
			IList<IWebElement> listOfElements = unorderedListElements.FindElements(By.TagName("li"));
			foreach (IWebElement listItem in listOfElements)
			{
				if (listItem.Text.Equals(itemToFind))
				{
					return listItem;
				}
			}
			throw new Exception("Element was not found in list");
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



	/// <summary>
	/// Contains the main settings for a PageObject
	/// </summary>
	public class PageSettings : Attribute
	{
		/// <summary>
		/// This property is used to relate the page object to a DOM subtree in the view.
		/// This affects how the test helper locates the element.
		/// </summary>
		public string ContainerSelector{ get; set; }
		/// <summary>
		/// If the input fields referenced by a property of the page object, are actually properties of an other property,
		/// we can specify this to be trimmed, from the generated C# code.
		/// This affects the code generation only.
		/// 
		/// Exapmle:
		/// 
		/// ViewModel.DomainModel.Property
		/// PageObject.Property
		/// 
		/// Will generate:
		/// ExecuteCommand(c => c.SetValue(f => f.Property, "test"));
		/// Instead of:
		/// ExecuteCommand(c => c.DomainModel.SetValue(f => f.Property, "test"));
		/// </summary>
		public string ViewPrefix { get; set; }

		/// <summary>
		/// The structure of the nested page object does not affect the generated path.
		/// This is used to prefix the nested class's properties with.
		/// Example
		/// We have two properties in the view:
		/// -ViewModel.Property1
		/// -ViewModel.Property2
		/// 
		/// But we want to restructure them to be in two child classes
		/// PageObject.TabPage1.Property1
		/// PageObject.TabPage2.Property2
		/// We have to prefix them with the appropiate property names
		/// TODO: this is got to be moved to the TabPageInitializerAspect, if we want to enable reusability of the tab pages across page objects
		/// </summary>
		public string PageObjectPrefix { get; set; }
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="containerSelector">
		/// This property is used to relate the page object to a DOM subtree in the view.
		/// This affects how the test helper locates the element.
		/// </param>
		/// <param name="viewModelTypeName">
		/// This is the related ViewModel's type full name. It helps the code generator to find the appropiate page object for the view.
		/// </param>
		/// <param name="viewPrefix">
		/// If the input fields referenced by a property of the page object, are actually properties of an other property,
		/// we can specify this to be trimmed, from the generated C# code.
		/// </param>
		/// <param name="pageObjectPrefix">
		/// The structure of the nested page object does not affect the generated path.
		/// This is used to prefix the nested class's properties with 
		/// This affects the code generation only.
		/// </param>
		public PageSettings(string containerSelector,string viewPrefix="",string pageObjectPrefix="")
		{
			ContainerSelector = containerSelector;
			ViewPrefix = viewPrefix;
			PageObjectPrefix = pageObjectPrefix;
		}
	}

	/// <summary>
	/// A few helper methods for pages
	/// </summary>
	public static class PageExtensions
	{

		public static TPage SetValue<TPage, TProperty>(this TPage page, Expression<Func<TPage, TProperty>> exp, string value) where TPage : PageObjectBase
		{
			var visitor = new MemberAccessVisitor(exp);
			visitor.SetValue(page,value);
			return page;
		}

		public static TPage SetDriver<TPage>(this TPage page, SafeWebDriver d)where TPage : PageObjectBase
		{
			page.Driver = d;
			return page;
		}

		public static TPage DumpPage<TPage>(this TPage page,string dumpName,string description="") where TPage : PageObjectBase
		{
			page.Driver.DumpPage(dumpName,description);
			return page;
		}
	}

	public class RDPMessageDialogPage
	{
		private SafeWebDriver _driver;

		public static By RdpMessageBoxFormCancelSelector = By.Id("rdpMessageBoxFormCancel");
		public static By RdpMessageBoxFormOKSelector = By.ClassName("rdpMessageBoxFormOK");
		public const string RdpMessageBoxFormOKCssSelector = ".button rdpMessageBoxFormOK";

		public RDPMessageDialogPage(SafeWebDriver driver)
		{
			_driver = driver;
		}

		/// <summary>
		/// Click the Cancel button
		/// </summary>
		public void ClickCancelButton()
		{
			_driver.FindElement(RdpMessageBoxFormCancelSelector).Click();
		}

		/// <summary>
		/// Click the Ok, (button with css class rdpMessageBoxFormOK)
		/// </summary>
		public void ClickOkButton()
		{
			_driver.FindElement(RdpMessageBoxFormOKSelector).Click();
		}
	}
}