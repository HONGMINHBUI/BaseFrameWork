using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.PageObject.Common.Elements;
using OpenQA.Selenium;
using System;
using System.Linq;

namespace Controcc.Web.UITesting.PageObjects.Common
{
	public abstract class ModalBase : ControccWebPageObjectBase
	{
		private readonly LabelElement modalTitle;

		public LabelElement ModalTitle => modalTitle;

		public ModalBase()
		{
			modalTitle = new LabelElement(this, ".slds-modal__header > h2", SelectByEnum.CSS);
		}

		public abstract void CloseModal();

		public abstract bool PageIsLoaded();

		public TPage OpenHelpOption<TPage>(string modalPrefix, string value) where TPage : PageObjectBase, new()
		{
			var moreHelp = Driver.FindElement(By.CssSelector($"[data-uitesting-id='{modalPrefix}More help']"));
			moreHelp.Click();
			return Help<TPage>(modalPrefix, value, true);
		}

		public TPage HelpButton<TPage>(string modalPrefix, string value) where TPage : PageObjectBase, new()
		{
			return Help<TPage>(modalPrefix, value, false);
		}

		private TPage Help<TPage>(string modalPrefix, string value, bool isDropdown) where TPage : PageObjectBase, new()
		{
			var initialWindowHandles = Driver.WindowHandles.Count;
			var dropdownPrefix = isDropdown ? "dropdown-" : string.Empty;
			var element = Driver.FindElement(By.CssSelector($"[data-uitesting-id='{dropdownPrefix}{modalPrefix}Help for {value}']"));
			element.Click();
		
			if (Driver.WindowHandles.Count > initialWindowHandles)
			{
				Driver.SwitchTo().Window(Driver.WindowHandles.Last());
			}

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = Driver;
			return page;
		}
	}
}
