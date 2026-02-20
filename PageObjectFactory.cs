using System;
using OCC.UI.TestingFramework.WebDriverExtensions;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	public class PageObjectFactory
	{
		public static TPage Create<TPage>(PageObjectBase parentPage)
			where TPage : PageObjectBase
		{
			var newPage = Activator.CreateInstance<TPage>();
			newPage.Driver = parentPage.Driver;
			newPage.ParentPage = parentPage;
			return newPage;
		}

		public static TPage Create<TPage>(SafeWebDriver driver)
			where TPage : PageObjectBase
		{
			var newPage = Activator.CreateInstance<TPage>();
			newPage.Driver = driver;
			return newPage;
		}

		public static PageObjectBase<TViewModel> CreateGeneric<TViewModel>(PageObjectBase parentPage)
		{
			return Create<PageObjectBase<TViewModel>>(parentPage);
		}

		public static PageObjectBase<TViewModel> CreateGeneric<TViewModel>(SafeWebDriver driver)
		{
			return Create<PageObjectBase<TViewModel>>(driver);
		}

	}
}
