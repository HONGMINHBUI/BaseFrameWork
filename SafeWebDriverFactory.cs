using OCC.UI.TestingFramework.Configuration;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public class SafeWebDriverFactory
	{
		private static Dictionary<Tuple<BrowserEnum, TestCredential>, SafeWebDriver> _drivers;

		static SafeWebDriverFactory()
		{
			_drivers = new Dictionary<Tuple<BrowserEnum, TestCredential>, SafeWebDriver>();
		}
		/// <summary>
		/// Instantiates the approiate driver based on the enum, and wraps it in a SafeWebDriver
		/// </summary>
		public static SafeWebDriver CreateDriver(TestCredential credential, bool forceLoginForThisTest, BrowserEnum browserEnum = BrowserEnum.Firefox, string browserPath = "",
			string driverPath = "", string baseURL = "", int pollingInterval = 100, int timeout = 5000)
		{
			var key = new Tuple<BrowserEnum, TestCredential>(browserEnum, credential);

			if (_drivers.ContainsKey(key))
			{
				if (!_drivers[key].IsOpen)
					_drivers.Remove(key);
				else if (forceLoginForThisTest)
				{
					_drivers[key].Close();
					_drivers[key].Quit();
					_drivers.Remove(key);
				}
			}

			if (_drivers.ContainsKey(key))
			{
				_drivers[key].IsReused = true;
				return _drivers[key];
			}
			else
			{
				SafeWebDriver driver = null;
				if (browserEnum == BrowserEnum.Safari)
				{
					driver = new SafeWebDriver(browserEnum,
						new SafariDriver(driverPath, new SafariOptions { AcceptInsecureCertificates = true, Proxy = null }), baseURL);
				}
				else if (browserEnum == BrowserEnum.Chrome)
				{
					ChromeOptions options = new ChromeOptions() { AcceptInsecureCertificates = true, Proxy = null };
					if (!string.IsNullOrWhiteSpace(browserPath))
					{
						options.BinaryLocation = browserPath;
					}
					options.AddUserProfilePreference("safebrowsing.enabled", "false");
					driver = new SafeWebDriver(browserEnum, new ChromeDriver(driverPath, options), baseURL);
				}
				else if (browserEnum == BrowserEnum.InternetExplorer)
				{
					driver = new SafeWebDriver(browserEnum, new InternetExplorerDriver(driverPath), baseURL);
				}
				else if (browserEnum == BrowserEnum.Firefox)
				{
					FirefoxDriverService geckoService = FirefoxDriverService.CreateDefaultService(driverPath);
					//Increases the running speed of the tests tenfold.
					geckoService.Host = "::1";
					FirefoxOptions ffprofile = new FirefoxOptions() { AcceptInsecureCertificates = true, Proxy = null };
					//Prevent Firefox from asking the user before saving zip files (e.g the OPG reports) to the Downloads folder.
					ffprofile.SetPreference("browser.helperApps.neverAsk.saveToDisk", "application/zip;text/csv");
					driver = new SafeWebDriver(browserEnum, new FirefoxDriver(geckoService, ffprofile), baseURL);
				}
				else if (browserEnum == BrowserEnum.Edge)
				{
					driver = new SafeWebDriver(browserEnum, new EdgeDriver(driverPath), baseURL);
				}

				if (driver == null)
					throw new ArgumentException("Unable to create driver");

				driver.PollingInterval = pollingInterval;
				driver.Timeout = timeout;

				_drivers[key] = driver;
				return driver;
			}
		}

		public static void CloseAllDrivers()
		{
			if (_drivers != null)
			{
				foreach (SafeWebDriver driver in _drivers.Values)
				{
					driver.DecoratedWebDriver.Close();
					driver.DecoratedWebDriver.Quit();
				}
			}
		}

		public static void RemoveDriver(SafeWebDriver driver)
		{
			if (_drivers.ContainsValue(driver))
			{
				var key = _drivers.FirstOrDefault(x => x.Value == driver).Key;

				driver.DecoratedWebDriver.Close();
				driver.DecoratedWebDriver.Quit();

				_drivers.Remove(key);
			}
		}

	}

}