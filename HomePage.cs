using Controcc.Web.UITesting.PageObjects.Notes;
using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.PageObject.Common.Elements;

namespace Controcc.Web.UITesting.PageObjects.Common
{
	public class HomePage : ControccWebPageObjectBase
	{
		public TPage GoToRecentlyViewedEntity<TPage>(string linkText) where TPage : PageObjectBase, new()
		{
			LinkElement recentlyViewedLink = new LinkElement(this, linkText);
			TPage entity = recentlyViewedLink.Click<TPage>();
			return entity;
		}

		public NotesPanel GoToMyActions(string actionText)
		{
			LinkElement actionLink = new LinkElement(this, actionText);
			NotesPanel notesPanel = actionLink.Click<NotesPanel>();
			return notesPanel;
		}
	}
}
