using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public static class WebElementExtensions
	{
		/// <summary>
		/// Extracts all the attributes from a WebElement. The element has to exist in the DOM at the time this function is called.
		/// </summary>
		/// <returns></returns>
		internal static string DumpWebElement(this IWebElement element)
		{
			try
			{
				if (SafeWebElement.DetailedElementDump.HasValue && !SafeWebElement.DetailedElementDump.Value)
				{
					return element.ToString();
				}

				var stringBuilder = new StringBuilder();

				stringBuilder.Append("<");
				stringBuilder.Append(element.TagName);

				var attributes = new Dictionary<string, string>
				{
					{"id", element.GetAttribute("id")},
					{"class", element.GetAttribute("class")},
					{"value", element.GetAttribute("value")},
					{"alt", element.GetAttribute("alt")},
					{"name", element.GetAttribute("name")}
				};

				foreach (var attribute in attributes)
				{
					if (!string.IsNullOrEmpty(attribute.Value))
						stringBuilder.Append(" " + attribute.Key + "='" + attribute.Value + "'");
				}

				if (!string.IsNullOrEmpty(element.Text))
				{
					stringBuilder.Append(">");
					stringBuilder.Append(element.Text);
					stringBuilder.Append("/" + element.TagName + ">");
				}
				else
				{
					stringBuilder.Append("/>");
				}
				return stringBuilder.ToString();
			}
			catch (Exception ex)
			{
				return ex.Message;
			}

		}

		public static bool IsReadOnly(this IWebElement element)
		{
			if (!bool.TryParse(element.GetAttribute("readonly"), out bool readOnly))
				readOnly = false;
			if (!bool.TryParse(element.GetAttribute("disabled"), out bool disabled))
				disabled = false;

			return readOnly || disabled;
		}
	}
}
