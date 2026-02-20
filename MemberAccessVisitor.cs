using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OCC.UI.TestingFramework.Utility.Reflection
{
	public class MemberAccessVisitor : ExpressionVisitor
	{

		private List<Expression> path = new List<Expression>();

		public MemberAccessVisitor(Expression exp)
		{
			Visit(exp);
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			path.Add(m);
			return base.VisitMethodCall(m);
		}

		protected override Expression VisitMemberAccess(MemberExpression m)
		{
			path.Add(m);
			return base.VisitMemberAccess(m);
		}

		public void SetValue(object rootObject, string value)
		{
			object actObject = rootObject;

			for (int i = path.Count - 1; i > 0; i--)
			{
				var expression = path[i];
				if (expression is MethodCallExpression)
				{
					var methodExpr = expression as MethodCallExpression;

					actObject = methodExpr.Method.Invoke(actObject, BindingFlags.Public, null,
						GetArguments(methodExpr.Arguments),
						CultureInfo.InvariantCulture);
				}
				else
				{
					actObject = ((expression as MemberExpression).Member as PropertyInfo).GetValue(actObject,
						BindingFlags.Public, null, null,
						CultureInfo.InvariantCulture);
				}
			}
			var propertyInfo = ((path.First() as MemberExpression).Member as PropertyInfo);
			propertyInfo.SetValue(actObject, value, BindingFlags.Public, null, null, CultureInfo.InvariantCulture);
		}

		public List<Expression> GetPath()
		{
			return path;
		}

		private object[] GetArguments(IEnumerable<Expression> args)
		{
			return args.OfType<ConstantExpression>().Select(expression => expression.Value).ToArray();
		}
	}
}