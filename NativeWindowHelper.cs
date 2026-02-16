using System;
using System.Runtime.InteropServices;
using System.Threading;
using OCC.UI.TestingFramework.Utility.InputSimulation;

namespace OCC.UI.TestingFramework.WebDriverExtensions
{
	public class NativeWindowHelper
	{

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName,string lpWindowName);

		[DllImport("USER32.DLL")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private static void AuthenticateFirefox(string userName,string password)
		{
			var hWnd = FindWindow(null, "Hitelesítés szükséges");

			if (!hWnd.Equals(IntPtr.Zero))
			{
				SetForegroundWindow(hWnd);
				VirtualKeyBoard.SimulateTextEntry(userName);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateTextEntry(password);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.SPACE);
			}
		}

		private static void AuthenticateSafari(string url,string userName, string password)
		{

			var hWnd = FindWindow(null, url);

			if (!hWnd.Equals(IntPtr.Zero))
			{
				VirtualKeyBoard.SimulateTextEntry(userName);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateTextEntry(password);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.SPACE);
			}
		}


		internal static void Authenticate(BrowserEnum browser, string startupUrl, string userName, string password)
		{
			if(browser==BrowserEnum.Firefox)
				AuthenticateFirefox(userName,password);
			else if(browser==BrowserEnum.Safari)
				AuthenticateSafari(startupUrl,userName,password);
		}

		public static void UploadFileInIE(string filePath)
		{
			var hWnd = FindWindow(null, "Choose File to Upload");

			if (!hWnd.Equals(IntPtr.Zero))
			{
				SetForegroundWindow(hWnd);
				VirtualKeyBoard.SimulateTextEntry(filePath);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.TAB);
				VirtualKeyBoard.SimulateKeyPress(VirtualKeyCode.SPACE);
				Thread.Sleep(2000);
			}
		}
	}

   
}