using OpenQA.Selenium;
using System;


namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	[Serializable]
	public class TextInputElement : PageValueElementBase<string>
	{
		public TextInputElement(PageObjectBase owner, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(owner, inputSelector, selectBy)
		{
		}

		#region Overrides

		public override void SetValue(string value)
		{
			try
			{
				var el = OwnerPage.Driver.FindElement(GetBy());
				Clear(el);
				el.SendKeys(value.ToString());
			}
			catch (Exception) { }
		}

		public override string GetValue()
		{
			WaitForLoadingScreenToClose();
			var value = OwnerPage.Driver.FindElement(GetBy()).GetAttribute("Value");
			return value;
		}

		#endregion Overrides

		public void Clear(IWebElement el = null)
		{
			IWebElement element = el;
			try
			{
				if (element == null)
				{
					element = OwnerPage.Driver.FindElement(GetBy());
				}

				element.Click(); //focus
				//element.SendKeys(Keys.Backspace);
				element.SendKeys(Keys.Control + "a" + Keys.Delete);
				element.Clear();
			}
			catch (Exception) { }
		}

		public void Click()
		{
			var el = OwnerPage.Driver.FindElement(GetBy());
			el.Click();
		}

		public void NonNullableMonetaryFieldSetValue(string text)
		{
			try
			{
				var el = OwnerPage.Driver.FindElement(GetBy());
				el.Clear();
				el.SendKeys(Keys.Backspace);
				el.SendKeys(Keys.Backspace);
				el.SendKeys(Keys.Backspace);
				el.SendKeys(text.ToString());
			}
			catch (Exception) { }
		}

		public string GetText()
		{
			return OwnerPage.Driver.FindElement(GetBy()).Text;
		}
		public string GetFieldValue
		{
			get
			{
				var input = Driver.FindElement(GetBy())
					.FindElement(By.TagName("input"));
				return input.GetAttribute("value");
			}
		}
	}
}
