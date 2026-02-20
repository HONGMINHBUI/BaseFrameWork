using System;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	[Serializable]
	public class CheckboxElement : PageValueElementBase<bool>
	{
		public CheckboxElement(PageObjectBase page, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(page, inputSelector, selectBy)
		{
		}

		public override void SetValue(bool value)
		{
			var selectDomElement = OwnerPage.Driver.FindElement(GetBy());
			if ((selectDomElement.Selected != value))
			{
				selectDomElement.Click();
			}
		}

		public override bool GetValue()
		{
			var value = OwnerPage.Driver.FindElement(GetBy()).Selected;
			return value;
		}
	}
}
