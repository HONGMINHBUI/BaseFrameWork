using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public static class IWebElementExtensions
	{
		public static IWebElement FindElementOrDefault(this IWebElement element, By by, IWebElement defaultElement = null)
		{
			try
			{
				return element.FindElement(by);
			}

			catch (NoSuchElementException)
			{
				return defaultElement;
			}
		}
	}
}
