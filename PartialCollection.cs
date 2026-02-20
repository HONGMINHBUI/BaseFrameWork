using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace OCC.UI.TestingFramework.PageObject.Common
{
	public interface IPartialCollection
	{
		PageObjectBase ParentPage { get; set; }
		RepeatedPartialPage getDummy();
	}

	/// <summary>
	/// This class represents a set of partials that are rendered multiple times on the page.
	/// Every time a partial gets accessed trough the indexer, it instanciates a pageObject for that.
	/// </summary>
	/// <typeparam name="TPartial">The type of the partial, that is rendered more than once</typeparam>
	public class PartialCollection<TPartial> : IPartialCollection
		where TPartial : RepeatedPartialPage
	{
		public PageObjectBase ParentPage { get; set; }

		public RepeatedPartialPage getDummy()
		{
			return this[0];
		}

		public TPartial this[int i]
		{
			get
			{
				var constructors = typeof (TPartial).GetConstructors();
				var args = new List<object> {ParentPage, i};
				var tmp =
					constructors[0].Invoke(BindingFlags.Public, null, args.ToArray(), CultureInfo.InvariantCulture) as TPartial;
				tmp.ParentPage = ParentPage;
				tmp.Driver = ParentPage.Driver;
				return tmp;
			}
		}
	}
}