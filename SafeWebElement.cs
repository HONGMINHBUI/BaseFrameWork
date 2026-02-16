using System;
using OCC.UI.TestingFramework.Utility.InputSimulation;
using OpenQA.Selenium;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	/// <summary>
	/// This class decorates a web element, and intercepts the method calls in oder to log them.
	/// There are some tweaks, for example the element has to be visible in order to be clickable,
	/// this class scrolls down to the element's position in order to make sure, that it's visible.
	/// It also uses a WIN32 calls to emulate sendkeys for IE
	/// </summary>
	public class SafeWebElement : IWebElement
	{
		public static bool? DetailedElementDump { get; set; }

		internal IWebElement DecoratedWebElement { get; set; }
		internal SafeWebDriver SafeWebDriver { get; set; }
		internal string ElementDump { get; set; }

		public SafeWebElement(IWebElement webElement, SafeWebDriver safeWebDriver)
		{
			safeWebDriver.Logger?.AddLine("Creating safe web element");
			DecoratedWebElement = webElement;
			SafeWebDriver = safeWebDriver;
			safeWebDriver.Logger?.AddLine(" => Safe web element dump");
			ElementDump = DecoratedWebElement.DumpWebElement();
			safeWebDriver.Logger?.AddLine(" => After safe web element dump");
		}

		/// <summary>
		/// IE and opera only clicks if the window is in focus. 
		/// The element is got to be visible so it also scrolls down if it is necessary
		/// </summary>
		public void Click()
		{
			try
			{
				var executor = SafeWebDriver.DecoratedWebDriver as IJavaScriptExecutor;
				if (executor != null) executor.ExecuteScript("window.focus()");

			}
			catch (Exception)
			{
				//excluded
			}

			try
			{
				int elementPosition = DecoratedWebElement.Location.Y;
				String js = String.Format("window.scroll(0, {0})", elementPosition);
				SafeWebDriver.ExecuteJavascript(js);

				SafeWebDriver.Wait.Until(condition =>
				{
					try
					{
						DecoratedWebElement.Click();
						return true;
					}
					catch (ElementClickInterceptedException)
					{
						return false;
					}
				});
				SafeWebDriver.TestReport.AppendEntry("Click", ElementDump, true, "");
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Click", ElementDump, false, ex.Message);
			}
		}

		/// <summary>
		/// IE sendkey is slow because it injects javascript to the code, so if it's IE we send it with WIN32 calls
		/// </summary>
		/// <param name="text"></param>
		public void SendKeys(string text)
		{
			try
			{
				if (SafeWebDriver.Browser == BrowserEnum.InternetExplorer)
				{
					VirtualKeyBoard.SimulateTextEntry(text);
				}
				else
				{
					DecoratedWebElement.SendKeys(text);
				}
				SafeWebDriver.TestReport.AppendEntry("SendKeys", text, true, ElementDump);
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("SendKeys", text, false, ex.Message);
				throw;
			}
		}

		public void Clear()
		{
			//get the focus
			DecoratedWebElement.SendKeys("");
			//DecoratedWebElement.SendKeys(Keys.Backspace);
			DecoratedWebElement.SendKeys(Keys.Control + "a" + Keys.Delete);
			DecoratedWebElement.Clear();
		}

		#region Simple delegation

		public bool Displayed
		{
			get { return DecoratedWebElement.Displayed; }
		}

		public bool Enabled
		{
			get { return DecoratedWebElement.Enabled; }
		}

		public string GetAttribute(string attributeName)
		{
			return DecoratedWebElement.GetAttribute(attributeName);
		}

		public string GetCssValue(string propertyName)
		{
			return DecoratedWebElement.GetCssValue(propertyName);
		}

		public System.Drawing.Point Location
		{
			get { return DecoratedWebElement.Location; }
		}

		public bool Selected
		{
			get { return DecoratedWebElement.Selected; }
		}

		public System.Drawing.Size Size
		{
			get { return DecoratedWebElement.Size; }
		}

		public void Submit()
		{
			DecoratedWebElement.Submit();
		}

		public string TagName
		{
			get { return DecoratedWebElement.TagName; }
		}

		public string Text
		{
			get { return DecoratedWebElement.Text; }
		}

		public IWebElement FindElement(By by)
		{
			return DecoratedWebElement.FindElement(by);
		}

		public System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> FindElements(By by)
		{
			return DecoratedWebElement.FindElements(by);
		}

		public string GetProperty(string propertyName)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}