using System;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	[Serializable]
	public class ButtonElement : PageElementBase
	{
		public ButtonElement(PageObjectBase page, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(page, inputSelector, selectBy)
		{
		}

		public void Click()
		{
			WaitForLoadingScreenToClose();
			OwnerPage.Driver.FindElement(GetBy()).Click();
		}

		public TPage Open<TPage>() where TPage : PageObjectBase, new()
		{
			Click();

			TPage page = Activator.CreateInstance<TPage>();
			page.Driver = OwnerPage.Driver;
			return page;
		}
	}
}
