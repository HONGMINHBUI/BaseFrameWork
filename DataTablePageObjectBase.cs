using OpenQA.Selenium;
using System;
using System.Threading;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	public class DataTablePageObjectBase<TModel> : PageObjectBase<TModel>
	{
		/// <summary>
		/// A method to search for an item into tables using the "Search" text box. 
		/// </summary>
		/// <param name="itemName">Value to enter in "Search" text box</param>
		public TPage SearchFor<TPage>(string itemName) where TPage : PageObjectBase, new()
		{
			Driver.FindElement(By.CssSelector("input[type=\"search\"]")).Clear();
			Thread.Sleep(500);
			Driver.FindElement(By.CssSelector("input[type=\"search\"]")).SendKeys(itemName);
			Thread.Sleep(1000);

			var newPage = Activator.CreateInstance<TPage>();
			newPage.Driver = Driver;
			return newPage;
		}

		/// <summary>
		/// A method to search for an item into tables using the "Search" text box. 
		/// Next navigate to particular item by clicking on the first link with text "itemName"
		/// </summary>
		/// <param name="itemName">Value to enter in "Search" text box, and link text to navigate to.</param>
		public TPage SearchForAndNavigateTo<TPage>(string itemName) where TPage : PageObjectBase, new()
		{
			Driver.FindElement(By.CssSelector("input[type=\"search\"]")).Clear();
			Thread.Sleep(500);
			Driver.FindElement(By.CssSelector("input[type=\"search\"]")).SendKeys(itemName);
			Thread.Sleep(1000);
			return NavigateTo<TPage>(By.LinkText(itemName));
		}
	}
}
