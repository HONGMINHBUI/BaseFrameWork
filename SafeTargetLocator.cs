using System;
using OpenQA.Selenium;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public class SafeTargetLocator : ITargetLocator
	{
		internal SafeWebDriver SafeWebDriver { get; set; }
		internal ITargetLocator DecoratedTargetLocator { get; set; }

		public SafeTargetLocator(ITargetLocator decoratedTargetLocator, SafeWebDriver safeWebDriver)
		{
			DecoratedTargetLocator = decoratedTargetLocator;
			SafeWebDriver = safeWebDriver;
		}

		public IWebElement ActiveElement()
		{
			try
			{
				var activeElement = DecoratedTargetLocator.ActiveElement();
				SafeWebDriver.TestReport.AppendEntry("ActiveElement", "", true, "");
				return activeElement;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("ActiveElement", "", false, ex.Message);
				throw;
			}
		}

		public IAlert Alert()
		{
			try
			{
				var alert = DecoratedTargetLocator.Alert();
				SafeWebDriver.TestReport.AppendEntry("Alert", "", true, "");
				return alert;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Alert", "", false, ex.Message);
				throw;
			}
		}

		public IWebDriver DefaultContent()
		{
			try
			{
				var defContent = DecoratedTargetLocator.DefaultContent();
				SafeWebDriver.TestReport.AppendEntry("DefaultContent", "", true, "");
				return defContent;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("DefaultContent", "", false, ex.Message);
				throw;
			}
		}

		public IWebDriver Frame(IWebElement frameElement)
		{
			try
			{
				var frame = DecoratedTargetLocator.Frame(frameElement);
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameElement:" + frameElement.DumpWebElement(), true, "");
				return frame;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameElement:" + frameElement.DumpWebElement(), false, ex.Message);
				throw;
			}
		}

		public IWebDriver Frame(string frameName)
		{
			try
			{
				var frame = DecoratedTargetLocator.Frame(frameName);
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameName:" + frameName, true, "");
				return frame;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameName:" + frameName, false, ex.Message);
				throw;
			}
		}

		public IWebDriver Frame(int frameIndex)
		{
			try
			{
				var frame = DecoratedTargetLocator.Frame(frameIndex);
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameIndex:" + frameIndex, true, "");
				return frame;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Frame", "frameIndex:" + frameIndex, false, ex.Message);
				throw;
			}
		}

		public IWebDriver Window(string windowName)
		{
			try
			{
				var window = DecoratedTargetLocator.Window(windowName);
				SafeWebDriver.TestReport.AppendEntry("Window", "windowName:" + windowName, true, "");
				return window;
			}
			catch (Exception ex)
			{
				SafeWebDriver.TestReport.AppendEntry("Window", "windowName:" + windowName, false, ex.Message);
				throw;
			}
		}


		public IWebDriver ParentFrame()
		{
			return DecoratedTargetLocator.ParentFrame();
		}
	}
}
