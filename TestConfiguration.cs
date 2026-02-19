using System;
using System.Collections.Generic;
using System.Configuration;
using OCC.UI.TestingFramework.WebDriverExtensions;

namespace OCC.UI.TestingFramework.Configuration
{
	[Serializable]
	public class TestConfiguration
	{
		public string MachineName { get; set; }

		public string WebDriverPath { get; set; }

		public string ReportOutputPath { get; set; }

		public string TestFilesBasePath { get; set; }

		public LoggerConfig LoggerConfig { get; set; }

		public BrowserConfiguration CommonBrowserConfiguration { get; set; }

		public List<BrowserConfiguration> BrowserConfigurations { get; set; }

		public List<TestCredential> TestCredentials { get; set; }

		public DatabaseConfiguration DatabaseConfiguration { get; set; }

		public MachineSpecificSettingCollection MachineSpecificSettings { get; set; }
	}

	[Serializable]
	public class DatabaseConfiguration
	{
		public string InstanceName { get; set; }

		public string DatabaseName { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public string SaUserName { get; set; }

		public string SaPassword { get; set; }

		public string DatabaseBackupPath { get; set; }

		public string ConnectionStringFilePath { get; set; }

		public string DatabaseMailProfileName { get; set; }
		public bool? UseIntegratedUser { get; set; }
		public bool ClearCacheOnDateChange { get; set; }
		public bool ForceClearCache { get; set; }
	}

	[Serializable]
	public class BrowserConfiguration
	{
		public BrowserEnum Browser { get; set; }
		public string BrowserPath { get; set; }
		public bool? TestsEnabled { get; set; }
		public string StartupUrl { get; set; }
		public int? PollingInterval { get; set; }
		public int? Timeout { get; set; }
		public int? Speed { get; set; }
		public bool? DetailedElementDump { get; set; }
		public IgnoreTestCollection IgnoreTests { get; set; }
	}

	public class LoggerConfig
	{
		public string FolderPath { get; set; }
		public string FileName { get; set; }
		public bool? Enabled { get; set; }
	}


	public class IgnoreTest : ConfigurationElement
	{
		public string ClassName { get; set; }

		public string MethodName { get; set; }
	}

	public class TestCredential
	{
		public string CredentialName { get; set; }
		public string UserName { get; set; }
		public string Password { get; set; }
		public string SecretWord { get; set; }
	}

	public class MachineSpecificSettingCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new MachineSpecificSetting();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var e = (MachineSpecificSetting)element;
			return e.Name;
		}

		public void SetElements(List<MachineSpecificSetting> collection)
		{
			BaseClear();
			foreach (var e in collection)
			{
				BaseAdd(e);
			}
		}
	}

	[Serializable]
	public class MachineSpecificSetting : ConfigurationElement
	{
		public string Name { get; set; }

		public string Value { get; set; }
	}

	public class IgnoreTestCollection : ConfigurationElementCollection
	{

		protected override ConfigurationElement CreateNewElement()
		{
			return new IgnoreTest();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			var filter = (IgnoreTest)element;
			return string.Format("{0}.{1}", filter.ClassName, filter.MethodName);
		}

		public void SetElements(List<IgnoreTest> collection)
		{
			BaseClear();
			foreach (var ignoreTest in collection)
			{
				BaseAdd(ignoreTest);
			}
		}
	}
}