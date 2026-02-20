using System;
using Microsoft.Data.SqlClient;
using OCC.UI.TestingFramework.Configuration;
using System.IO;
using System.Data;

namespace Controcc.Web.UITesting.PageObjects.ControccWebTestBaseClasses
{
	public class DatabaseHelper
	{
		public delegate void InitialiseState();
		public delegate void AfterRestore();

		private static string getConnectionString(string dbName = null)
		{
			var dbConfig = TestConfigurationManager.DatabaseConfiguration;
			string strConnection = $"Data Source={dbConfig.InstanceName};initial catalog={dbName ?? dbConfig.DatabaseName}";

			if (dbConfig.UseIntegratedUser.HasValue && dbConfig.UseIntegratedUser.Value)
			{
				strConnection += ";Integrated Security=SSPI";
			}
			else
			{
				strConnection += ";User ID=" + dbConfig.UserName;
				strConnection += ";Password=" + dbConfig.Password;
			}

			strConnection += ";Encrypt=True;TrustServerCertificate=True";

			return strConnection;
		}

		private static SqlConnection getConnection(out string database)
		{
			var dbConfig = TestConfigurationManager.DatabaseConfiguration;
			database = dbConfig.DatabaseName;
			return new SqlConnection(getConnectionString(database));
		}

		public static void ActivateProtocol()
		{
			SqlConnection conn = getConnection(out string database);

			void runSQLOnCurrentDb(string sql) => runSQL(conn, $@"USE [{database}]{sql}");

			runSQLOnCurrentDb("UPDATE TRef_ContractType SET CPLIAuthorisationRequiredIfNotProtocol = 0");
			runSQLOnCurrentDb("UPDATE TSys_Setting SET HasProtocol = 1");
			runSQLOnCurrentDb("EXEC dbo.SYS_IgnoreDifferenceFromLASetting");
		}

		public static void AddProtocolServiceRequestPermission_ToFullwebRole()
		{
			SqlConnection conn = getConnection(out string database);

			void runSQLOnCurrentDb(string sql) => runSQL(conn, $@"USE [{database}]{sql}");

			runSQLOnCurrentDb(@"INSERT INTO dbo.TSys_SecurityRolePermission(SecurityRoleID, ContentTypeID, CanCreateUndelete, CanDelete)
								SELECT SR.SecurityRoleID, 48, 0, 0
								FROM dbo.TSys_SecurityRole SR
								WHERE SecurityRole = 'fullweb_role'
								EXCEPT
								SELECT XSRP.SecurityRoleID, XSRP.ContentTypeID, 0, 0
								FROM dbo.TSys_SecurityRolePermission XSRP");
		}

		public static void InitialiseOrRestoreState(string vstrStateDescription, InitialiseState vdelInitialise, AfterRestore vdelAfterRestore = null)
		{
			var dbConfig = TestConfigurationManager.DatabaseConfiguration;
			string strBackupDir = dbConfig.DatabaseBackupPath;
			string strDatabase = dbConfig.DatabaseName;

			if (String.IsNullOrEmpty(strBackupDir))
				strBackupDir = Path.GetTempPath();

			strBackupDir = strBackupDir.Trim();

			string strBackupFile = Path.Combine(strBackupDir, strDatabase + ".bak");

			// Create connection to "master" database on server
			var conn = new SqlConnection(getConnectionString("master"));

			// We include date prefix to backup name so when doing automated tests we reduce possibility
			// of using a backup made for an earlier version by forcing a reset if tests not run for
			// current date.
			bool bClearCacheOnDateChange = dbConfig.ClearCacheOnDateChange;
			string strPrefixDate = bClearCacheOnDateChange ? DateTime.Now.ToShortDateString() + ":" : "";

			// We will overwrite the backup file (to prevent it growing too large) if we are use normal case
			// of ClearCacheOnDateChange unless we find a backup in the file which is for this date.
			bool bOverwriteBackup = bClearCacheOnDateChange;

			// See if a cached version of database in required configuration exists in backup file
			string strBackupName = strPrefixDate + vstrStateDescription.Replace("'", "''");
			object objCachedPosition = DBNull.Value;

			//the following is useful when e.g. you have applied an updated SP and want
			//to ensure it included in the cache for future test runs
			if (dbConfig.ForceClearCache)
				bOverwriteBackup = true;
			else
			{
				try
				{
					DataSet dsBackupInfo = runSQLRetDataSet(conn,
						"Restore HeaderOnly "
						+ "From Disk='" + strBackupFile + "'");

					foreach (DataRow drBackup in dsBackupInfo.Tables[0].Rows)
					{
						if (((string)drBackup["BackupName"]).StartsWith(strPrefixDate))
							bOverwriteBackup = false;

						/* Note: Any apostrophes in vstrStateDescription are "doubled" when assigning strBackupName above. This is necessary to ensure that the "Backup
						 Database..." Sql command below is syntactically valid.  However, the BackupName returned by the "Restore HeaderOnly..." Sql query will *not* reflect the 
						 doubled apostrophes - so we need to "undouble" in order to match with strBackupName. */

						if ((string)drBackup["BackupName"] == strBackupName.Replace("''", "'"))
						{
							objCachedPosition = drBackup["Position"];
							break;
						}
					}
				}
				// Ignore exceptions here, typically if backup file does not exist in which case we will always create
				catch { }

			}

			// If cached version exists we restore it
			if (objCachedPosition != DBNull.Value)
			{
				// We must kill all connections to database prior to restore
				DataSet dsProcesses = runSQLRetDataSet(conn, "exec sp_who");
				foreach (DataRow drRow in dsProcesses.Tables[0].Rows)
				{
					try
					{
						if (!Convert.IsDBNull(drRow["dbname"]) &&
							string.Compare(strDatabase, Convert.ToString(drRow["dbname"]), true) == 0)
						{
							runSQL(conn, string.Format("kill {0}", Convert.ToInt32(drRow["spid"])));
						}
					}
					catch { }
				}

				// any existing connections are invalidated by restore above
				SqlConnection.ClearAllPools();

				runSQL(conn, "alter database [" + strDatabase + "] set offline with rollback immediate");

				runSQL(conn,
					$"Restore Database [{strDatabase}] From Disk='{strBackupFile}' With Replace, File={objCachedPosition} "
				);

				runSQL(conn, $"alter database [{strDatabase}] set online");

				runSQL(conn, $"Use [{strDatabase}]; exec SYS_EnableServiceBroker"); // Service broker is disabled for restored databases

				runSQL(conn, $"use [{strDatabase}] EXEC SYS_Deploy");

				RecalculateExceptions(conn, strDatabase);

				vdelAfterRestore?.Invoke();
			}

			// Otherwise we prepare database for running tests and then back a backup of this configuration
			else
			{
				// Connections could have been invalidated by killing of connections during a restore.  We
				// cannot just rely on ClearAllPools calling following this as may be in different test
				// assemblies. There is no significant performance loss starting with an empty pool for each
				// test executed.
				SqlConnection.ClearAllPools();

				vdelInitialise();

				runSQL(conn,
					"Backup Database [" + strDatabase + "] "
					+ "To Disk='" + strBackupFile + "' "
					+ "With Compression, Name='" + strBackupName + "'"
					// By defining a media name this means we will not overwrite backups which
					// have been created by other means and an exception will get thrown
					+ ",MediaName='ControccUITests'"
					+ (bOverwriteBackup ? ",Init" : "")
						);
			}
		}

		public static void RecalculateExceptions(SqlConnection conn = null, string strDatabase = null)
		{
			conn ??= getConnection(out strDatabase);
			runSQL(conn, $"use [{strDatabase}] EXEC Controcc_ApplicationService_BrokerProcessor_ProcessTargetMessages__Exceptions");
		}

		public static void RefreshAuditing(SqlConnection conn = null, string strDatabase = null)
		{
			conn ??= getConnection(out strDatabase);
			runSQL(conn, $"use [{strDatabase}] EXEC Controcc_ApplicationService_BrokerProcessor_ProcessTargetMessages__Auditing");
		}

		public static void ChangeLASetting(string onsCode, string storedProcedureSuffix, SqlConnection conn = null, string strDatabase = null)
		{
			conn ??= getConnection(out strDatabase);
			runSQL(conn, $"use [{strDatabase}] UPDATE TSys_Setting set ONSCode = '{onsCode}', StoredProcedureSuffix = '{storedProcedureSuffix}'");
		}

		/// <summary>
		/// Executes <paramref name="sql"/> on database referenced in configuration
		/// </summary>
		/// <param name="sql"></param>
		public static void RunSQL(string sql)
		{
			runSQL(new SqlConnection(getConnectionString()), sql);
		}
		
		private static void runSQL(SqlConnection conn, string sql)
		{
			try
			{
				conn.Open();

				var cmd = conn.CreateCommand();
				cmd.CommandTimeout = 240;
				cmd.Connection = conn;
				cmd.CommandType = System.Data.CommandType.Text;
				cmd.CommandText = sql;

				cmd.ExecuteNonQuery();
			}
			finally
			{
				conn.Close();
			}
		}

		private static DataSet runSQLRetDataSet(SqlConnection conn, string sql)
		{
			var dsRet = new DataSet();
			try
			{
				conn.Open();

				var cmd = conn.CreateCommand();
				cmd.CommandTimeout = 240;
				cmd.Connection = conn;
				cmd.CommandType = System.Data.CommandType.Text;
				cmd.CommandText = sql;

				using var adpTemp = new SqlDataAdapter(cmd);
				//NB we don't call FillDataSet as it records a Fill DA message.
				int? rowCount;

				rowCount = adpTemp.Fill(dsRet);
			}
			finally
			{
				conn.Close();
			}

			return dsRet;
		}
	}
}
