using OCC.UI.TestingFramework.Interface;
using OCC.UI.TestingFramework.WebDriverExtensions.Report;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	/// <summary>
	/// A decorator class which abstracts the differences between the different webdriver implementations
	/// </summary>
	public class SafeWebDriver : IWebDriver
	{

		public IWebDriver DecoratedWebDriver { get; set; }
		public bool IsReused { get; set; }

		public bool IsOpen
		{
			//There is no explicit way to check does driver is still open. So we try to perform an action and check does error occur.
			get
			{
				try
				{
					var a = this.Title;
				}
				catch (Exception)
				{
					return false;
				}
				return true;
			}
		}
		public int PollingInterval { get; set; }
		public int Timeout { get; set; }

		public Action<SafeWebDriver> WaitAction { get; set; }

		public IFileLogger Logger { get; set; }
		internal TestReport TestReport { get; set; }
		internal IWebDriver PreviousPopup { get; set; }

		public BrowserEnum Browser { get; private set; }

		public string BaseURL { get; private set; }

		public WebDriverWait Wait { get; set; } //Added by MDT

		public SafeWebDriver(BrowserEnum browser, RemoteWebDriver d, string baseURL = null)
		{
			this.Browser = browser;
			DecoratedWebDriver = d;
			PollingInterval = 100;
			Timeout = 2000;
			Wait = new WebDriverWait(d, new TimeSpan(0, 0, 90)); //Added by MDT
			TestReport = new TestReport();
			//d.Manage().Window.Maximize();
			d.Manage().Window.Size = new Size(1920, 1080);
			this.BaseURL = baseURL;
			this.Navigate().GoToUrl(BaseURL);
			d.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0.5);

			//IE Skip certificate error
			if (Title.StartsWith("Certificate Error:"))
				ExecuteJavascript("javascript:document.getElementById('overridelink').click()");

		}

		#region Internal methods

		public void TestFinished(bool succeeded, string path, Exception ex = null, List<ILogEntry> errorLog = null)
		{
			if (!succeeded)
			{
				var errorDump = CreatePageDump();
				if (errorDump != null)
					TestReport.AddErrorDump(errorDump);
				if (ex != null) TestReport.AppendEntry("Error", ex.GetType().Name, false, ex.Message);
				if (errorLog != null)
				{
					foreach (var logEntry in errorLog)
					{
						TestReport.AddLogEntry(logEntry);
						TestReport.AppendEntry("Error", ex.GetType().Name, false, logEntry.GetErrorMessage());
					}
				}
			}
			//only save the report if the test failed
			if (!succeeded)
			{
				TestReport.TestFinished(succeeded);

				if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
					TestReport.SaveReport(path);
			}
		}

		#endregion

		#region Dump methods

		private PageDump CreatePageDump()
		{
			var s = Task<PageDump>.Factory.StartNew(
				() =>
				{
					try
					{
						var scrFile = ((ITakesScreenshot)DecoratedWebDriver).GetScreenshot();
						return new PageDump()
						{
							ScreenDump = scrFile.AsByteArray,
							HtmlDump = DecoratedWebDriver.PageSource

						};
					}
					catch (Exception)
					{
						return null;
					}
				}
				);
			//TODO:continuewith el megoldani
			s.Wait(20000);
			return s.Result;
		}

		public void DumpElement(IWebElement element)
		{
			TestReport.AppendEntry("DumpElement", element.DumpWebElement(), true, "");
		}


		public void DumpElements(IEnumerable<IWebElement> elements)
		{
			TestReport.AppendEntry("DumpElements", "Count:" + elements.Count(), true,
				string.Join(Environment.NewLine, elements.Select(p => p.DumpWebElement())));
		}

		public void DumpPage(string label, string description)
		{
			var pageDump = CreatePageDump();
			if (pageDump != null)
			{
				pageDump.Description = description;
				pageDump.Label = label;
				TestReport.AddPageDump(pageDump);
				TestReport.AppendEntry("DumpPage", label, true, description);
			}
		}

		#endregion

		#region Navigation methods

		public string Url
		{
			get { return DecoratedWebDriver.Url; }
			set
			{
				try
				{
					DecoratedWebDriver.Url = value;
					TestReport.AppendEntry("NavigateToURL", value, true, "");
				}
				catch (Exception ex)
				{
					TestReport.AppendEntry("NavigateToURL", value, false, ex.Message);
					throw;
				}
			}
		}

		/// <summary>
		/// Egy megnyílt tabra váltja a drivert. Ha van title megadva, akkor azt keresi meg, ha nem akkor a legutoljára megnyíltat
		/// </summary>
		public void SwitchToPopup(string popupTitle = "", bool ignoreCase = true, bool contains = true, bool maximize = false)
		{
			var popupFound = false;

			var windowTitle = string.Format("FROM:{0} |", Title);

			if (string.IsNullOrEmpty(popupTitle))
			{
				PreviousPopup = SwitchTo().Window(WindowHandles[WindowHandles.Count - 1]);
				popupFound = true;
				if (maximize)
					Manage().Window.Maximize();
			}
			else
			{

				foreach (var windowHandle in WindowHandles)
				{
					PreviousPopup = SwitchTo().Window(windowHandle);

					var actTitle = ignoreCase ? PreviousPopup.Title.ToLower() : PreviousPopup.Title;
					var titleToFind = ignoreCase ? popupTitle.ToLower() : popupTitle;

					if (contains)
					{
						if (actTitle.Contains(titleToFind))
						{
							if (maximize)
								Manage().Window.Maximize();
							popupFound = true;
							break;
						}
					}
					else
					{
						if (actTitle == titleToFind)
						{
							if (maximize)
								Manage().Window.Maximize();
							popupFound = true;
							break;
						}
					}
				}
			}
			var details = string.Format("filter:{0} ignoreCase:{1}, contains:{2}, maximize:{3}", popupTitle, ignoreCase, contains,
				maximize);

			windowTitle += string.Format("TO:{0} |", popupFound ? Title : "NO POPUP FOUND");

			if (popupFound)
				TestReport.AppendEntry("SwitchToPopup", windowTitle, true, details);
			else
			{
				TestReport.AppendEntry("SwitchToPopup", windowTitle, false, details);
				throw new Exception("POPUP NOT FOUND");
			}
		}

		public ITargetLocator SwitchTo()
		{
			return new SafeTargetLocator(DecoratedWebDriver.SwitchTo(), this);
		}

		#endregion

		#region Delegating methods

		public void Close()
		{
			DecoratedWebDriver.Close();
		}

		public void Dispose()
		{
			DecoratedWebDriver.Dispose();
		}

		public string CurrentWindowHandle
		{
			get { return DecoratedWebDriver.CurrentWindowHandle; }
		}

		public IOptions Manage()
		{
			return DecoratedWebDriver.Manage();
		}

		public INavigation Navigate()
		{
			return DecoratedWebDriver.Navigate();
		}

		public string PageSource
		{
			get { return DecoratedWebDriver.PageSource; }
		}

		public void Quit()
		{
			DecoratedWebDriver.Quit();
		}

		public string Title
		{
			get { return DecoratedWebDriver.Title; }
		}

		public System.Collections.ObjectModel.ReadOnlyCollection<string> WindowHandles
		{
			get { return DecoratedWebDriver.WindowHandles; }
		}

		#endregion

		#region Finding elements

		public IWebElement FindElement(By by)
		{
			if (WaitAction != null)
			{
				WaitAction(this);
			}

			Logger?.AddLine(String.Format("FindElement by {0}", by.ToString()));

			var description = GetDescription(by);
			var sw = Stopwatch.StartNew();

			Logger?.AddLine(" => create stopwatch");

			while (sw.ElapsedMilliseconds < Timeout)
			{
				try
				{
					var elements = DecoratedWebDriver.FindElements(by);
					Logger?.AddLine(" => Find elements called");
					if (!elements.Any()) continue;
					var element = elements.First();
					Logger?.AddLine(" => ELEMENT FOUND - before element dump");
					TestReport.AppendEntry("FindElement", description, true, "ELEMENT FOUND:" + Environment.NewLine + element.DumpWebElement());
					Logger?.AddLine(" => ELEMENT FOUND - after element dump");
					sw.Stop();
					Logger?.AddLine(" => returning safe web element");
					return new SafeWebElement(element, this);
				}
				catch
				{
				}
				finally
				{
					Logger?.AddLine(String.Format(" => thread sleep for {0}", PollingInterval));
					Thread.Sleep(PollingInterval);
					Logger?.AddLine(String.Format(" => after sleep"));
				}
			}
			sw.Stop();
			Logger?.AddLine(" => stop stopwatch");
			TestReport.AppendEntry("FindElement", description, false, "ELEMENT NOT FOUND");
			throw new Exception("Unable to find element:" + description);

		}

		public ReadOnlyCollection<IWebElement> FindElements(By by)
		{
			return FindElementsInternal(by, true);
		}

		//this does not throw an exception
		public ReadOnlyCollection<IWebElement> SafeFindElements(By by)
		{
			return FindElementsInternal(by, false);
		}

		// ReSharper disable once UnusedParameter.Local
		private ReadOnlyCollection<IWebElement> FindElementsInternal(By by, bool throwException)
		{
			var timeWaited = 0;
			var description = GetDescription(by);
			Thread.Sleep(2000);
			while (timeWaited < Timeout)
			{
				try
				{
					var elements = DecoratedWebDriver.FindElements(by);
					if (elements == null || !elements.Any())
						continue;
					var result = elements.Select(p => new SafeWebElement(p, this) as IWebElement).ToList().AsReadOnly();
					return result;

				}
				catch
				{
				}
				finally
				{
					Thread.Sleep(PollingInterval);
					timeWaited += PollingInterval;
				}
			}
			if (throwException)
				throw new Exception("Unable to find element:" + description);
			return new ReadOnlyCollection<IWebElement>(new List<IWebElement>());
		}


		#endregion

		#region Helper

		public object ExecuteJavascript(string script)
		{
			object result = null;
			try
			{

				var executor = DecoratedWebDriver as IJavaScriptExecutor;
				if (executor != null)
				{
					result = executor.ExecuteScript(script);
				}
				TestReport.AppendEntry("ExecuteJavascript", script, true, "");
			}
			catch (Exception ex)
			{
				TestReport.AppendEntry("ExecuteJavascript", script, false, ex.Message);
			}
			return result;
		}

		public void FileUpload(By fileUploadBy, string filePath)
		{
			var fileUploadElement = FindElement(fileUploadBy);
			if (Browser == BrowserEnum.InternetExplorer)
			{
				fileUploadElement.Click();
				NativeWindowHelper.UploadFileInIE(filePath);
			}
			else
			{
				fileUploadElement.SendKeys(filePath);
			}

		}

		private string GetDescription(By by)
		{
			try
			{
				var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
				var descPropertyInfo = typeof(By).GetProperty("Description", bindingFlags);
				var description =
					descPropertyInfo.GetValue(by, bindingFlags, null, null, CultureInfo.InvariantCulture)
						.ToString()
						.Substring(3);
				return description;
			}
			catch (Exception ex)
			{
				TestReport.AppendEntry("GetDescription", "", false, ex.Message);
				return "";
			}
		}

		#endregion
	}
}