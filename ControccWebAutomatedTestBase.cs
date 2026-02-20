using Controcc.Web.UITesting.PageObjects.Common;
using OCC.UI.TestingFramework.Configuration;
using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.WebDriverExtensions;
using OpenQA.Selenium;
using System;

namespace Controcc.Web.UITesting.PageObjects.ControccWebTestBaseClasses
{
	//A base class for specifically the Controcc Web tests, based off the corresponding MarketPlace base test classes.
	public class ControccWebAutomatedTestBase : AutomatedUITestBase<HomePage>
	{
		public override void TestStarted(BrowserEnum browser, string baseUrl, string driverPath = "",
			string browserPath = "", string reportPath = "",
			int pollingInterval = 100, int timeOut = 5000)
		{
			//backup or restore database
			BackupOrRestoreDatabase();

			base.TestStarted(browser, baseUrl, driverPath, browserPath, reportPath, pollingInterval: pollingInterval, timeOut: timeOut);

			var startupPage = new StartupPage();
			startupPage.SetDriver(driver);

			var config = TestConfigurationManager.LoggerConfig;
			if(config.Enabled.HasValue && config.Enabled.Value)
			{
				driver.Logger = new FileLogger(config.FolderPath, config.FileName);
			}

			driver.WaitAction = ControccWebWaitUntilLoadingScreensDisappear;

			if (driver.IsReused)
			{
				driver.Url = driver.BaseURL;
			}
			else
			{
				var loginPage = startupPage.GoToLoginPage();
				var credential = InitTestCredential();
				if (credential != null)
				{
					string errorMessage;
					loginPage.Login(credential.UserName, credential.Password);

					// One login retry if first attempt fails
					if (loginPage.HasLoginFailed(out errorMessage))
					{
						loginPage.Login(credential.UserName, credential.Password);

						if (loginPage.HasLoginFailed(out errorMessage))
						{
							// If login fails again throw a useful exception, rather than failing on the next attempt to find an element
							throw new SystemException(errorMessage);
						}
					}
				}
			}
		}

		public override TestCredential InitTestCredential()
		{
			return TestConfigurationManager.TestCredentials[Username ?? "fullweb"];
		}

		private void BackupOrRestoreDatabase()
		{
			if (!string.IsNullOrWhiteSpace(TestConfigurationManager.DatabaseConfiguration.InstanceName))
			{
				DatabaseHelper.InitialiseOrRestoreState("test", () => { });
			}
		}

		public void UpdateDatabase_EnableProtocolAndAddPermission()
		{
			DatabaseHelper.ActivateProtocol();
			DatabaseHelper.AddProtocolServiceRequestPermission_ToFullwebRole();
		}

		/// <summary>
		/// Wait for the Loading overlay to disappear
		/// </summary>
		/// <param></param>
		/// <returns></returns>
		private void ControccWebWaitUntilLoadingScreensDisappear(SafeWebDriver driver)
		{
			driver.Wait.Until(condition =>
				{
					try
					{
						var loadingScreen = driver.DecoratedWebDriver.FindElement(By.CssSelector(".loader__banner"));
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

