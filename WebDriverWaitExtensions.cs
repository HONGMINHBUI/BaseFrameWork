using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	/// <summary>
	/// This class contains helper methods to deal with browser alert dialogs.
	/// </summary>
	public static class WebDriverWaitExtensions
	{
		public static void WaitForAutoSuggestResults(this SafeWebDriver driver, By selector)
		{
			try
			{
				new WebDriverWait(driver, TimeSpan.FromSeconds(60)).Until(
					(d) =>
					{
						try
						{
							var rows = driver.FindElement(selector)
									.FindElement(By.ClassName("react-autosuggest__suggestion--first"));
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					});
			}
			catch (Exception)
			{
				throw;
			}
		}		

		public static void TryToAcceptAlert(this SafeWebDriver driver)
		{
			try
			{

				new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(
					(d) =>
					{
						try
						{
							var alert = d.SwitchTo().Alert();
							alert.Accept();
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					);
				driver.TestReport.AppendEntry("TryToAcceptAlert", "", true, "");
			}
			catch (Exception ex)
			{
				driver.TestReport.AppendEntry("TryToAcceptAlert", "", false, ex.Message);
				throw;
			}
		}

		public static void TryToDismissAlert(this SafeWebDriver driver)
		{
			try
			{
				new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(
					(d) =>
					{
						try
						{
							var alert = d.SwitchTo().Alert();
							alert.Dismiss();
							return true;
						}
						catch (Exception)
						{
							return false;
						}
					}
					);
				driver.TestReport.AppendEntry("TryToDismissAlert", "Alert dismissed", true, "");
			}
			catch (Exception ex)
			{
				driver.TestReport.AppendEntry("TryToDismissAlert", "Alert cannot be dismissed", false, ex.Message);
				throw;
			}
		}

		public static void TryToWaitPageLoad(this SafeWebDriver driver)
		{
			Thread.Sleep(2000);
		}

		public static void TryToWait(this SafeWebDriver driver, int waitTime)
		{
			Thread.Sleep(waitTime);
		}

		public static void TryToWaitForPopup(this SafeWebDriver driver)
		{
			try
			{
				var handles = driver.WindowHandles;
				var newWindow = (new WebDriverWait(driver.DecoratedWebDriver, TimeSpan.FromSeconds(10))).Until(
					(d) =>
					{
						var newHandles = driver.WindowHandles;
						var tmp = newHandles.Except(handles).ToArray();
						return tmp.Length > 0 ? tmp.First() : null;
					});
				driver.TestReport.AppendEntry("TryToWaitForPopup", "", true, "");
			}
			catch (Exception ex)
			{
				driver.TestReport.AppendEntry("TryToWaitForPopup", "", false, ex.Message);
				throw;
			}

		}
	}
}
