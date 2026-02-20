using OCC.UI.TestingFramework.Interface;
using System;
using System.IO;

namespace Controcc.Web.UITesting.PageObjects.ControccWebTestBaseClasses
{
	public class FileLogger : IFileLogger
	{
		public string FilePath { get; set; }
		public string FileName { get; set; }

		public FileLogger(string path, string filename)
		{
			FilePath = path;
			FileName = filename;
		}

		public bool Enabled { get; set; }

		public void AddLine(string text)
		{
			string msg = string.Format("{0}: {1}", DateTime.Now.ToString(), text);
			File.AppendAllText(string.Format("{0}\\{1}", FilePath, FileName), msg + Environment.NewLine);
		}
	}
}
