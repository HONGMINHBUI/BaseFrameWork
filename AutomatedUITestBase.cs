using OCC.UI.TestingFramework.Configuration;
using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.Utility;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using OCC.UI.TestingFramework.Interface;
using System.Linq;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public interface IAutomatedUITestBase
	{
		MethodBase Testmethod { get; set; }
		string ReportPath { get; set; }
		string BaseUrl { get; set; }
		SafeWebDriver driver { get; set; }
		BrowserEnum Browser { get; set; }
		void TestFinished(bool succeeded, Exception ex = null);
		void InitializeEnvironment();
		void TestStarted(BrowserEnum browser, string baseUrl, string driverPath = "", string browserPath = "", string reportPath = "", int pollingInterval = 100, int timeOut = 5000);

	}

	/// <summary>
	/// This is the base class for the Automated UI tests
	/// I try to keep it decoupled from the page objects
	/// </summary>
	public abstract class AutomatedUITestBase<TStartPage> : IAutomatedUITestBase, IDisposable
		where TStartPage : PageObjectBase, new()
	{
		public ILogExtractor LogExtractor { get; set; }
		public BrowserEnum Browser { get; set; }
		public SafeWebDriver driver { get; set; }
		public string BaseUrl { get; set; }
		public string ReportPath { get; set; }
		public MethodBase Testmethod { get; set; }
		public TStartPage StartPage { get; set; }
		public bool ForceLoginForNextTest { get; set; }
		public string FullDriverPath { get; set; }
		public string Username { get; set; }

		public static IEnumerable<object[]> Browsers =>
			TestConfigurationManager.ActiveBrowsers.Value.Select(e => new object[] { e });

		#region Test Starting Methods

		public void StartTest(BrowserEnum browser, Action action)
		{
			InitializeEnvironment();

			var actConfig = TestConfigurationManager.GetConfiguration(browser);

			SafeWebElement.DetailedElementDump = actConfig.DetailedElementDump ?? false;

			TestStarted(
				browser,
				actConfig.StartupUrl,
				FullDriverPath,
				actConfig.BrowserPath,
				TestConfigurationManager.ReportOutputPath,
				actConfig.PollingInterval ?? 200,
				actConfig.Timeout ?? 5000
			);

			string methodName;
			string className;
			string testAssembly;
			int depth = 1;
			var stack = new StackTrace();

			do
			{
				var frame = stack.GetFrame(depth);
				methodName = frame.GetMethod().Name;
				className = frame.GetMethod().DeclaringType.ToString();
				testAssembly = frame.GetMethod().DeclaringType.Assembly.ToString();
				depth++;

			} while (methodName.Equals("StartTest") && depth < 5);


			driver.TestReport.TestStarted(browser, className, methodName, testAssembly, "module", "version");
			driver.Logger?.AddLine("-------------> Test started");

			try
			{
				action();
				TestFinished(true);
				driver.TestReport.TestFinished(true);
				driver.Logger?.AddLine("-------------> Test finished with success");
			}
			catch (Exception ex)
			{
				TestFinished(false, ex);
				driver.TestReport.TestFinished(false);
				driver.Logger?.AddLine("-------------> Test finished with failure");
				throw;
			}
		}

		public void StartTest(BrowserEnum[] browsers, Action action)
		{
			foreach (var browser in browsers)
			{
				StartTest(browser, action);
			}
		}

		public void StartTest(string username, BrowserEnum[] browsers, Action action)
		{
			Username = username;
			StartTest(browsers, action);
		}

		public void StartTest(string username, BrowserEnum browser, Action action)
		{
			Username = username;
			StartTest(browser, action);
		}

		#endregion Test Starting Methods


		/// <summary>
		/// This method creates the Safe WebDriver instance and initializes it.
		/// It's virtual so it enables descendant classes to add some extra initialization steps.
		/// </summary>
		public virtual void TestStarted(BrowserEnum browser, string baseUrl, string driverPath = "", string browserPath = "", string reportPath = "",
			int pollingInterval = 100, int timeOut = 5000)
		{
			Browser = browser;
			BaseUrl = baseUrl;
			ReportPath = reportPath;
			driver = SafeWebDriverFactory.CreateDriver(InitTestCredential(), false, browser, browserPath, driverPath, BaseUrl, pollingInterval, timeOut);
			driver.Url = baseUrl;
			driver.Wait = new WebDriverWait(driver, new TimeSpan(0, 0, 90)); //Added by MDT
			StartPage = new TStartPage { Driver = driver };
		}

		public virtual void TestFinished(bool succeeded, Exception ex = null)
		{
			//driver.PerformanceTimer.Stop();
			List<ILogEntry> logEntries = null;
			if (!succeeded && LogExtractor != null)
			{
				logEntries = LogExtractor.GetLogEntries();
			}
			driver.TestFinished(succeeded, ReportPath, ex, logEntries);

			SafeWebDriverFactory.RemoveDriver(driver);
		}

		public virtual TestCredential InitTestCredential()
		{
			throw new NotImplementedException();
		}

		public virtual void InitializeEnvironment()
		{
			ReportPath = TestConfigurationManager.ReportOutputPath;

			var driverPath = TestConfigurationManager.WebDriverPath;
			var basePath = PathHelper.GetProjectWorkingDirectory();
			if (String.IsNullOrWhiteSpace(driverPath))
				FullDriverPath = basePath;
			else
				FullDriverPath = PathHelper.Combine(basePath, driverPath);

			if (!Directory.Exists(FullDriverPath))
			{
				throw new Exception("Driver path does not exist");
			}

			ForceLoginForNextTest = false;
		}

		public string GetTestFileAbsolutePath(string filePath)
		{
			var projectBase = PathHelper.GetProjectPathWithStackTrace(2);
			var artifactBase = PathHelper.Combine(projectBase, TestConfigurationManager.TestFilesBasePath);
			var fullFilePath = Path.Combine(artifactBase, filePath);
			return PathHelper.HackPath(fullFilePath);
		}

		public string GetAbsolutePath(string filePath)
		{
			var projectBase = PathHelper.GetProjectPathWithStackTrace(2);
			var fullFilePath = PathHelper.Combine(projectBase, filePath);
			return PathHelper.HackPath(fullFilePath);
		}

		/// <summary>
		/// Navigates to URL relative to baseUrl
		/// </summary>
		/// <param name="path"></param>
		private void NavigateToRelativeUrl(string path)
		{
			var url = new Uri(new Uri(BaseUrl), path);
			driver.Navigate().GoToUrl(url);
		}

		/// <summary>
		/// Navigates to URL relative to baseUrl and returns navigated page as an instance of <typeparamref name="TPage"/>
		/// </summary>
		/// <typeparam name="TPage"></typeparam>
		/// <param name="path"></param>
		/// <returns></returns>
		public TPage NavigateToRelativeUrl<TPage>(string path) where TPage : PageObjectBase, new()
		{
			NavigateToRelativeUrl(path);

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = driver;
			return page;
		}

		/// <summary>
		/// Navigates to URL relative to baseUrl and returns navigated page as PageObjectBase after verifying it is assignable from <paramref name="pageType"/>
		/// </summary>
		/// <param name="path"></param>
		/// <param name="pageType"></param>
		/// <returns></returns>
		public PageObjectBase NavigateToRelativeUrl(string path, Type pageType)
		{
			if (!typeof(PageObjectBase).IsAssignableFrom(pageType))
				throw new ArgumentException($"{pageType} is not a derived type of {typeof(PageObjectBase)}");

			NavigateToRelativeUrl(path);

			PageObjectBase page = Activator.CreateInstance(pageType) as PageObjectBase;
			page.Driver = driver;
			return page;
		}

		public void Dispose()
		{
			//try to remove driver if something goes bad
			SafeWebDriverFactory.RemoveDriver(driver);
		}
	}
}