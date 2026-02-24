using System;
using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.PageObject.Common.Elements;
using OCC.UI.TestingFramework.Utility.InputSimulation;
using OpenQA.Selenium;

namespace Controcc.Web.UITesting.PageObjects.Elements
{
	[Serializable]
	public class CWebSelectElement : PageValueElementBase<string>
	{
		public CWebSelectElement(PageObjectBase page, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(page, inputSelector, selectBy)
		{
		}

		public override string GetValue()
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());

			try
			{
				var obj = wrapper.FindElement(By.XPath(".//div/div/div/div"));
				return string.IsNullOrEmpty(obj.Text) ? null : obj.Text.Trim();
			}
			catch (Exception) { }

			return null;
		}


		public override void SetValue(string value)
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var input = wrapper.FindElement(By.TagName("input"));

			if (value == null)
			{
				try
				{
					input.Click(); //get focus on IE
					input.SendKeys(""); //get focus
					VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.DELETE);
				}
				catch (Exception) { }
			}
			else
			{
				try
				{
					wrapper.Click(); //get focus on IE
					input.SendKeys(value);
					input.SendKeys(Keys.Tab);
				}
				catch (Exception) { }
			}
		}

		public bool HasErrorHighlighted()
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var obj = wrapper.FindElement(By.XPath("./../../../.."));
			string className = obj.GetAttribute("class");
			if (className == "slds-form-element slds-has-error")
			{
				return true;
			}
			return false;
		}

		public void SelectNthOption(int optNumber)
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var input = wrapper.FindElement(By.TagName("input"));

			try
			{
				wrapper.Click(); //get focus on IE
				input.SendKeys(""); //get focus
				int i = 0;
				while (i < optNumber + 1)
				{
					if (i == optNumber)
					{
						input.SendKeys(Keys.Tab);
					}
					else
					{
						input.SendKeys(Keys.Down);
					}
					i++;
				}
			}
			catch (Exception) { }
		}

		public void ClearDropDown()
		{
			var wrapper = OwnerPage.Driver.FindElement(GetBy());
			var input = wrapper.FindElement(By.TagName("input"));
			wrapper.Click();
			input.SendKeys(Keys.Delete);
		}
	}
}
