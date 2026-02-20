using OCC.UI.TestingFramework.PageObject.Common.Elements;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OCC.UI.TestingFramework.Utility
{
	public class FileDownloadHelper
	{
		public static void DeleteExistingFiles(string filenamePattern, string pathName)
		{
			string fullpath = System.Environment.GetEnvironmentVariable("USERPROFILE") + pathName;
			string[] filePaths = Directory.GetFiles(fullpath);
			foreach (string p in filePaths)
			{
				if (p.Contains(filenamePattern))
				{
					File.Delete(p);
				}
			}
		}

		public static bool CheckFileDownloaded(string filename, string pathName)
		{
			Thread.Sleep(5000);
			bool downloadComplete = false;
			string fullpath = System.Environment.GetEnvironmentVariable("USERPROFILE") + pathName;
			string[] filePaths = Directory.GetFiles(fullpath);
			foreach (string p in filePaths)
			{
				if (p.Contains(filename))
				{
					Debug.WriteLine(p);
					FileInfo thisFile = new FileInfo(p);
					//Check that the file was downloaded within the last 2 minutes
					if (thisFile.LastWriteTime.ToShortTimeString() == DateTime.Now.ToShortTimeString() ||
					thisFile.LastWriteTime.AddMinutes(1).ToShortTimeString() == DateTime.Now.ToShortTimeString() ||
					thisFile.LastWriteTime.AddMinutes(2).ToShortTimeString() == DateTime.Now.ToShortTimeString())
					{
						downloadComplete = true;
					}
				}
			}
			return downloadComplete;
		}
	}
}
