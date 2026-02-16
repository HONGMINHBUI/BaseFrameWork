namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public static class StringExtensions
	{
		public static string RemoveChars(this string input, params char[] charsToRemove)
		{
			foreach (var @char in charsToRemove)
			{
				input = input.Replace(@char.ToString(), "");
			}
			return input;
		}
	}
}
