using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Globalization;
//using System.Management;
using System.Text;
using BscanILL.Misc;



namespace BscanILL.Export.ILLiad
{
	public class DatabaseSQLBase
	{
		#region variables

		protected SqlConnection sqlConnection;

		protected string cmdTransactionsSelectText = "";
		protected string cmdLenderAddressesSelectText;
		protected string cmdUserNvtgcString;

		protected bool singleStationSystem = false;
		protected bool isNVTGCNullable = false;

		public event ProgressChangedHandle ProgressChanged;
		public event ProgressCommentHandle ProgressComment;

		#endregion

		#region constructor
		public DatabaseSQLBase(string sqlServerUri, string databaseName, bool windowsCredentials, string username, string password)
		{
			string connectionString;

			if (windowsCredentials)
				connectionString = string.Format("Server={0};Database={1};Connection Timeout=10;Integrated Security=true;", sqlServerUri, databaseName);
			else
				connectionString = string.Format("Server={0};Database={1};Connection Timeout=10;Integrated Security=false;User ID={2};Password={3};", sqlServerUri, databaseName, username, password);

			//MessageBox.Show(connectionString);
					
			this.sqlConnection = new SqlConnection(connectionString);
			this.sqlConnection.FireInfoMessageEventOnUserErrors = false;
		}

		public DatabaseSQLBase()
		{
			string connectionString;
			BscanILL.SETTINGS.Settings settings = BscanILL.SETTINGS.Settings.Instance;

			if (settings.Export.ILLiad.SqlWindowsCredentials)
			{
				connectionString = string.Format("Server={0};Database={1};Connection Timeout=10;Integrated Security=true;", settings.Export.ILLiad.SqlServerUri, settings.Export.ILLiad.SqlDatabaseName);
			}
			else
			{
				string password = "";

				try
				{
					password = settings.Export.ILLiad.SqlPasswordText;
				}
				catch (Exception ex)
				{
					MessageBox.Show("Can't decrypt ILLiad SQL database password! " + ex.Message);
					password = "";
				}

				/*if (settings.Export.ILLiad.SqlPassword != null)
				{
					byte[] sqlPasswordArray = new byte[settings.Export.ILLiad.SqlPassword.Length];

					for (int i = 0; i < settings.Export.ILLiad.SqlPassword.Length; i++)
						sqlPasswordArray[i] = (byte)settings.Export.ILLiad.SqlPassword[i];

					try { password = ExportSettings.DecryptText(sqlPasswordArray); }	
					catch 
					{ 
						password = "";
					}
				}*/

				connectionString = string.Format("Server={0};Database={1};Connection Timeout=10;Integrated Security=false;User ID={2};Password={3};",
					settings.Export.ILLiad.SqlServerUri, settings.Export.ILLiad.SqlDatabaseName, settings.Export.ILLiad.SqlUsername, password);
			}

			this.sqlConnection = new SqlConnection(connectionString);
			this.sqlConnection.FireInfoMessageEventOnUserErrors = false;
		}
		#endregion

		//PUBLIC METHODS
		#region public methods

		#region GetDatabase()
		public static IDatabaseSQL GetDatabase()
		{
			try
			{
				switch (BscanILL.SETTINGS.Settings.Instance.Export.ILLiad.Version)
				{
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_1_8_0: 
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_2_0_0: return new DatabaseSQL7_2();
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_3_0_0: return new DatabaseSQL7_3();
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_4_0_0: return new DatabaseSQL7_4();
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_0_0_0:
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_1_0_0: return new DatabaseSQL8_1();
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_1_4_0: return new DatabaseSQL8_1_4_0();
					default: throw new IllException(ErrorCode.ILLiadUnsupportedVersion);
				}
			}
			catch (IllException ex)
			{
				throw ex;
			}
			catch (Exception ex)
			{
				throw new IllException(ErrorCode.IlliadNotRunning, ex.Message);
			}
		}
		#endregion
	
		#region Login
		public virtual void Login()
		{
			if (sqlConnection.State != ConnectionState.Open && sqlConnection.State != ConnectionState.Connecting)
			{
				Progress_Changed(null, -1, "Logining to the SQL database...");

				bool connectionOk = false;
				int attempts = 0;

				while (connectionOk == false)
				{
					try
					{
						sqlConnection.Open();
						connectionOk = true;
						Progress_Changed(null, -1, "Login to the SQL database was successfull.");

						SqlCommand sqlCommand = new SqlCommand("SELECT Count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME = 'LenderAddresses'", sqlConnection);
						sqlCommand.CommandTimeout = 10000;

						int count = (int)sqlCommand.ExecuteScalar();
						this.singleStationSystem = (count > 0);

						if (this.singleStationSystem)
							sqlCommand = new SqlCommand("SELECT IS_NULLABLE  FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LenderAddresses' AND COLUMN_NAME = 'NVTGC'", sqlConnection);
						else
							sqlCommand = new SqlCommand("SELECT IS_NULLABLE  FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LenderAddressesALL' AND COLUMN_NAME = 'NVTGC'", sqlConnection);
							
						sqlCommand.CommandTimeout = 10000;
						string nullable = (string)sqlCommand.ExecuteScalar();
						this.isNVTGCNullable = (nullable.ToLower() == "yes");

						Progress_Changed(null, -1, "SQL structure received.");
					}
					catch (Exception ex)
					{
						if (attempts++ > 3)
						{
							Progress_Changed(null, -1, "Can't connect to the ILLiad SQL server!. " + ex.Message);
							throw new Exception("Can't connect to the ILLiad SQL server!. " + ex.Message);
						}

						else
							System.Threading.Thread.Sleep(1000);
					}
				}
			}
		}
		#endregion

		#region Logout
		public void Logout()
		{
			if (sqlConnection.State == ConnectionState.Open)
			{
				sqlConnection.Close();
				Progress_Changed(null, -1, "Logout from ILLiad SQL server was successfull.");
			}
		}
		#endregion

		#region GetRequest()
		public virtual BscanILL.Export.ILL.TransactionPair GetRequest(int transactionId)
		{
			throw new Exception("Method DatabaseSQLBase:GetRequest() must be overriden!");
		}
		#endregion

		#region GetRequestFromIllNumber()
		public BscanILL.Export.ILL.TransactionPair GetRequestFromIllNumber(string illNumber)
		{
			Progress_Changed(null, -1, "Getting request by ILL Number '" + illNumber + "' from ILLiad SQL server...");
			Login();

			SqlCommand sqlCommand = new SqlCommand("SELECT TransactionNumber FROM Transactions WHERE ILLNumber = '" + illNumber + "'", sqlConnection);
			sqlCommand.CommandTimeout = 20000;

			try
			{
				Progress_Changed(null, -1, "Executing ILL Number search in ILLiad SQL server...");
				object transactionNumberObj = sqlCommand.ExecuteScalar();

				if (transactionNumberObj != null)
				{
					int transactionNumber = 0;

					if (transactionNumberObj is double)
						transactionNumber = Convert.ToInt32(transactionNumberObj);
					else
						transactionNumber = (int)transactionNumberObj;

					Progress_Changed(null, -1, "Transaction number " + transactionNumber.ToString() + " was found for ILL Number " + illNumber + " in ILLiad SQL server.");

					return GetRequest(transactionNumber);
				}
				else
				{
					Progress_Changed(null, -1, "Transaction Number not found for ILL Number " + illNumber + " in ILLiad SQL server...");
					return GetRequest(Convert.ToInt32(illNumber));
				}
			}
			catch
			{
				Progress_Changed(null, -1, "Transaction number not found for ILL Number " + illNumber + " in ILLiad SQL server...");
				return GetRequest(Convert.ToInt32(illNumber));
			}
		}
		#endregion

		#region GetRequests()
		public virtual List<BscanILL.Export.ILL.TransactionPair> GetRequests(string[] transactionStatuses, bool loans, bool articles)
		{
			throw new Exception("Method DatabaseSQLBase:GetRequests() must be overriden!");
		}
		#endregion

		#endregion

		//PRIVATE METHODS
		#region private methods

		#region Progress_Changed()
		protected void Progress_Changed(ExportUnit exportUnit, int progress, string description)
		{
			if (ProgressChanged != null)
				ProgressChanged(exportUnit, progress, description);
		}
		#endregion

		#region Progress_Comment()
		protected void Progress_Comment(ExportUnit exportUnit, string comment)
		{
			if (ProgressComment != null)
				ProgressComment(exportUnit, "Sql Database: " + comment);
		}
		#endregion

		#endregion

	}
}
