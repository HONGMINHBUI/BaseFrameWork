using OCC.UI.TestingFramework.Utility;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Linq;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	public interface IPageObjectBase
	{
		string GetViewModelType();
	}

	public class PageObjectBase<TModel> : PageObjectBase, IPageObjectBase
	{
		public void SetValue(By selector, string value)
		{
			var elements = Driver.FindElements(selector);
			var element = elements.First();

			var inputType = GetInputType(element);

			switch (inputType)
			{
				case InputType.Text:
					Driver.FindElement(selector).Clear();
					Driver.FindElement(selector).SendKeys(value);
					break;
				case InputType.Radio:
					foreach (var webElement in elements)
					{
						if (webElement.GetAttribute("value") == value)
						{
							webElement.Click();
							break;
						}
					}
					break;
				case InputType.Select:
					var selectDomElement = Driver.FindElement(selector);
					var selectElement = new SelectElement(selectDomElement);
					selectElement.SelectByText(value);
					break;
				case InputType.File:
					Driver.FileUpload(selector, value);
					break;
				case InputType.CheckBox:
					if (element.Selected != (value.ToLower() == "true"))
						element.Click();
					break;
			}
		}

		public void SetValue(string selector, string value)
		{
			var s = By.XPath("//" + XpathHelper.ToXPathSelector(selector));
			SetValue(s, value);
		}

		public void Click(string selector)
		{
			var s = By.XPath("//" + XpathHelper.ToXPathSelector(selector));
			var element = Driver.FindElement(s);
			element.Click();
		}

		public InputType GetInputType(IWebElement element)
		{
			var tagName = element.TagName;

			if (tagName.ToLower() == "select")
				return InputType.Select;

			var inputType = element.GetAttribute("type");
			switch (inputType)
			{
				case "file":
					return InputType.File;
				case "radio":
					return InputType.Radio;
				case "checkbox":
					return InputType.CheckBox;
				case "text":
					return InputType.Text;

			}
			return InputType.Text;
		}

		public string GetViewModelType()
		{
			return typeof (TModel).FullName;
		}
	}

	public enum InputType
	{
		Text,
		Radio,
		Select,
		CheckBox,
		File
	}

}