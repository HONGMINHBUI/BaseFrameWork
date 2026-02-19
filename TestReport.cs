using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using OCC.UI.TestingFramework.Interface;

namespace OCC.UI.TestingFramework.WebDriverExtensions.Report
{
	internal class TestReport
	{
		private List<TestEntry> entries;
	    private List<ILogEntry> logEntries; 
		private Dictionary<string, PageDump> userPageDumps;
		private PageDump errorDump;
		//env
		private BrowserEnum browser;
		private string machineID;
		private string user;
		private string application;
		private string module;
		private string version;

		private DateTime StartDate;
		private DateTime FinishedDate;
		private string TestClassName;
		private string TestMethodName;
		private bool TestSucceeded;

		public TestReport()
		{
			entries = new List<TestEntry>();
			userPageDumps = new Dictionary<string, PageDump>();
			machineID = Environment.MachineName;
			user = Environment.UserName;
            logEntries = new List<ILogEntry>();
		}

		internal void AppendEntry(string action, string parameter, bool succeeded, string details)
		{
			entries.Add(new TestEntry
			{
				Action = action,
				Parameter = parameter,
				Succeeded = succeeded,
				Details = details,
				DateTime = DateTime.Now

			});
		}

	    internal void AddLogEntry(ILogEntry logEntry)
	    {
	        logEntries.Add(logEntry);
	    }
		internal void AddPageDump(PageDump pageDump)
		{
			userPageDumps.Add(pageDump.Label, pageDump);
		}

		internal void AddErrorDump(PageDump pageDump)
		{
			errorDump = pageDump;
			errorDump.Description = "An error occurred during the test";
			errorDump.Label = "ERROR";
		}


		public void TestStarted(BrowserEnum browser, string testClass, string methodName, string application, string module,
			string version)
		{
			StartDate = DateTime.Now;
			this.application = application;
			this.module = module;
			this.version = version;
			this.TestMethodName = methodName;
			this.TestClassName = testClass;
			this.browser = browser;
		}

		internal void TestFinished(bool succeeded)
		{
			FinishedDate = DateTime.Now;
			TestSucceeded = succeeded;
		}

		internal void SaveReport(string path)
		{
			var d = StartDate;
			var reportName = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}", TestMethodName, browser,
				TestSucceeded ? "Passed" : "Failed", d.Year, d.Month, d.Day, d.Hour, d.Minute);

			path = Path.Combine(path, reportName);

			Directory.CreateDirectory(path);

			var builder = new StringBuilder();

			#region table

			builder.Append("<html><body>");

			builder.Append("<style>tr:nth-child(even) {background: #CCC}tr:nth-child(odd) {background: #FFF}</style>");

			builder.AppendFormat("<h1>{1}.{0} has {2}</h1>", TestClassName, TestMethodName, TestSucceeded ? "PASSED" : "FAILED");

			builder.Append("<h2>Summary</h2>");

			builder.Append("<table cellpadding=\"1\" cellspacing=\"1\" border=\"1\">");
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Browser", browser);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Application", application);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Component", module);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Version", version);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "User", user);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Machine", machineID);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Started", StartDate);
			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Finished", FinishedDate);

			builder.AppendFormat("<tr><td><b>{0}</b></td><td>{1}</td></tr>", "Result", TestSucceeded ? "PASSED" : "FAILED");

			builder.Append("</table>");


			builder.Append("<h2>Steps</h2>");

			builder.Append("<table cellpadding=\"1\" cellspacing=\"1\" border=\"1\">");
			builder.Append(
				"<thead><tr><td>Action</td><td>Parameters</td><td>Details</td><td>Succeded</td></tr></thead><tbody>");

			foreach (var testEntry in entries)
			{
				builder.Append("<tr>");
				builder.AppendLine(string.Format("<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td>", testEntry.Action,
					WebUtility.HtmlEncode(testEntry.Parameter),
					WebUtility.HtmlEncode(testEntry.Details),
					testEntry.Succeeded));
				builder.Append("</tr>");
			}

			builder.Append("<html><body>");

			#endregion

			#region Dumps

			WriteDumpToFile(path, errorDump);

			foreach (var userPageDump in userPageDumps)
			{
				WriteDumpToFile(path, userPageDump.Value);
			}

            #endregion

            #region
            foreach (var logEntry in logEntries)
            {
                File.WriteAllText(Path.Combine(path,"Error_"+logEntries.IndexOf(logEntry)+".xml"),logEntry.DetailsXml);
            }
            #endregion
            try
			{
				File.WriteAllText(Path.Combine(path, "Report.html"), builder.ToString());
			}
			catch (Exception ex)
			{
				File.WriteAllText(Path.Combine(path, "log.txt"), ex.Message);
			}
		}

		private void WriteDumpToFile(string path, PageDump dump)
		{
			if (dump == null) return;
			try
			{
				File.WriteAllText(Path.Combine(path, dump.Label + ".html"), dump.HtmlDump);
			}
			catch (Exception)
			{
			}
			try
			{
				File.WriteAllBytes(Path.Combine(path, dump.Label + ".jpg"), dump.ScreenDump);
			}
			catch (Exception)
			{
			}
		}


	}
}