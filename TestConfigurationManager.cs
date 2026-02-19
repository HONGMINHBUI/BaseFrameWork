using System;
using System.Collections.Generic;
using System.Linq;
using OCC.UI.TestingFramework.WebDriverExtensions;
using Microsoft.Extensions.Configuration;

namespace OCC.UI.TestingFramework.Configuration
{
	public class TestConfigurationManager
	{
		private static readonly Lazy<TestConfiguration> TestConfiguration = new Lazy<TestConfiguration>(() =>
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{Environment.MachineName}.json", optional: true, reloadOnChange: true)
				.Build();

			return config.GetSection("TestConfiguration").Get<TestConfiguration>();
		});

		public static string WebDriverPath => TestConfiguration.Value.WebDriverPath;

		public static string ReportOutputPath => TestConfiguration.Value.ReportOutputPath;

		public static string TestFilesBasePath => TestConfiguration.Value.TestFilesBasePath;

		public static LoggerConfig LoggerConfig => TestConfiguration.Value.LoggerConfig;

		public static DatabaseConfiguration DatabaseConfiguration => TestConfiguration.Value.DatabaseConfiguration;


		public static Dictionary<string, TestCredential> TestCredentials
		{
			get { return TestConfiguration.Value.TestCredentials.ToDictionary(k => k.CredentialName, v => v); }
		}

		public static Dictionary<string, string> MachineSpecificSettings
		{
			get { return TestConfiguration.Value.MachineSpecificSettings.Cast<MachineSpecificSetting>().ToDictionary(k => k.Name, v => v.Value); }
		}

		public static BrowserConfiguration GetConfiguration(BrowserEnum browser)
		{
			var mergedConfig = new BrowserConfiguration();
			
			var commonConfig = TestConfiguration.Value.CommonBrowserConfiguration;
			var browserConfig = TestConfiguration.Value.BrowserConfigurations.FirstOrDefault(x => x.Browser == browser);

			mergedConfig.Browser = browserConfig.Browser;
			mergedConfig.BrowserPath = browserConfig.BrowserPath ?? string.Empty;
			mergedConfig.StartupUrl = browserConfig.StartupUrl ?? commonConfig.StartupUrl;
			mergedConfig.PollingInterval = browserConfig.PollingInterval ?? commonConfig.PollingInterval;
			mergedConfig.Speed = browserConfig.Speed ?? commonConfig.Speed;
			mergedConfig.TestsEnabled = browserConfig.TestsEnabled ?? commonConfig.TestsEnabled;
			mergedConfig.Timeout = browserConfig.Timeout ?? commonConfig.Timeout;
			mergedConfig.DetailedElementDump = browserConfig.DetailedElementDump ?? commonConfig.DetailedElementDump ?? false;

			return mergedConfig;
		}

		public static Lazy<IEnumerable<BrowserEnum>> ActiveBrowsers => new Lazy<IEnumerable<BrowserEnum>>(() =>
			TestConfiguration.Value.BrowserConfigurations.Where(c => c.TestsEnabled.HasValue && c.TestsEnabled.Value)
				.Select(c => c.Browser));

		private static bool NotNullOrEmptyCheck(object value)
		{
			return value != null && !string.IsNullOrWhiteSpace(value.ToString());
		}


	}
}
