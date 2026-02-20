namespace OCC.UI.TestingFramework.PageObject.Common
{
	/// <summary>
	/// Represents a partial view that is rendered multiple times on the same page.
	/// It is aware of it's index.
	/// </summary>
	public abstract class RepeatedPartialPage : PageObjectBase
	{
		internal int Index { get; set; }

		public RepeatedPartialPage(PageObjectBase parent, int index) : base(parent)
		{
			Index = index;
		}

		public override string GetPageObjectPrefix()
		{
			return base.GetPageObjectPrefix() + string.Format("[{0}]", Index);
		}

		public override string GetViewIdPrefix()
		{
			return base.GetViewIdPrefix() + string.Format("[{0}]", Index);
		}
	}
}