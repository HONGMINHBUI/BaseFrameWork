using OCC.UI.TestingFramework.PageObject.Common;
using OCC.UI.TestingFramework.PageObject.Common.Elements;

namespace Controcc.Web.UITesting.PageObjects.Common
{
	public class LoginPage : PageObjectBase
	{
		private readonly TextInputElement usernameInput;
		private readonly TextInputElement passwordInput;
		private readonly ButtonElement submitBtn;

		public LoginPage()
		{
			usernameInput = new TextInputElement(this, "#Username", SelectByEnum.Id);
			passwordInput = new TextInputElement(this, "#Password", SelectByEnum.Id);
			submitBtn = new ButtonElement(this, "#loginBtn", SelectByEnum.Id);
		}

		public HomePage Login(string username, string password)
		{
			Driver.Navigate().Refresh();
			usernameInput.Value = username;
			passwordInput.Value = password;
			return submitBtn.Open<HomePage>();
		}

		public bool HasLoginFailed(out string errorMessage)
		{
			try
			{
				// Attempt to find the validation summary label
				var validation = new LabelElement(this, "validation-summary-errors", SelectByEnum.Name);
				errorMessage = validation.GetText();
				return !string.IsNullOrEmpty(errorMessage);
			}
			catch
			{
				//If not found, assume there are no errors and login has succeeded
				errorMessage = null;
				return false;
			}
		}
	}
}
