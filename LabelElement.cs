using System;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	[Serializable]
	public class LabelElement : PageElementBase
	{
		public LabelElement(PageObjectBase page, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(page, inputSelector, selectBy)
		{
		}

		public void Click()
		{
			OwnerPage.Driver.FindElement(GetBy()).Click();
		}

		public string GetText()
		{
			WaitForLoadingScreenToClose();
			return OwnerPage.Driver.FindElement(GetBy()).Text;
		}
	}
}
