using System;

namespace OCC.UI.TestingFramework.PageObject.Common.Elements
{
	/// <summary>
	/// Selects the option from the dropdown by it's text
	/// </summary>
	[Serializable]
	public abstract class PageValueElementBase<T> : PageElementBase
	{
		public PageValueElementBase(PageObjectBase owner, string inputSelector = "", SelectByEnum selectBy = SelectByEnum.TestId)
			: base(owner, inputSelector, selectBy)
		{
		}

		public abstract T GetValue();
		public abstract void SetValue(T value);

		public T Value
		{
			get { return GetValue(); }
			set { SetValue(value); }
		}

		public bool IsRequired()
		{
			try
			{
				var el = OwnerPage.Driver.FindElement(GetBy());
				return el.GetAttribute("required").ToLower().Equals("true");
			}
			catch (Exception) { }

			return false;
		}
	}
}