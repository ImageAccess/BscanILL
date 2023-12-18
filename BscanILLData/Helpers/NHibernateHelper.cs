using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Cache;

namespace BscanILLData.Helpers
{
	/// <summary>
	/// Handles creation and management of sessions and transactions. It is a singleton because building the initial session factory is 
	/// very expensive. Inspiration for this class came from Chapter 8 of Hibernate in Action by Bauer and King. Although it is a sealed 
	/// singleton you can use TypeMock (http://www.typemock.com) for more flexible testing.
	/// </summary>
	public class NHibernateHelper : IDisposable, INHibernateHelper
	{
		ISessionFactory _sessionFactory;
		//KicSession		_session = null;
		//ITransaction	_transaction = null;


		#region constructor
		/// <summary>
		/// Private constructor to enforce singleton
		/// </summary>
		public NHibernateHelper(BscanILLData.Models.SqlEngine sqlEngine /*string sessionFactoryConfigPath,*/, string connectionString)
		{
			if (sqlEngine == Models.SqlEngine.SQLite)
			{
				CreateSQLiteDatabaseFile(connectionString);
				
				_sessionFactory = FluentNHibernate.Cfg.Fluently.Configure()
										.Database(FluentNHibernate.Cfg.Db.SQLiteConfiguration.Standard.ConnectionString(connectionString))
										.Mappings(m =>
										{
											m.FluentMappings.AddFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
										})
										.BuildSessionFactory();
			}
			else
			{
				_sessionFactory = FluentNHibernate.Cfg.Fluently.Configure()
										.Database(FluentNHibernate.Cfg.Db.MsSqlConfiguration.MsSql2012.ConnectionString(connectionString))
										.Mappings(m =>
										{
											m.FluentMappings.AddFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());
										})
										.BuildSessionFactory();

	
				/*if (string.IsNullOrEmpty(sessionFactoryConfigPath))
					throw new ArgumentNullException("sessionFactoryConfigPath may not be null nor empty");

				if (!File.Exists(sessionFactoryConfigPath))
					// It would be more appropriate to throw a more specific exception than ApplicationException
					throw new ApplicationException("The config file at '" + sessionFactoryConfigPath + "' could not be found");

				NHibernate.Cfg.Configuration cfg = new NHibernate.Cfg.Configuration();
				cfg.Configure(sessionFactoryConfigPath);
				cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);

				//  Now that we have our Configuration object, create a new SessionFactory
				_sessionFactory = cfg.BuildSessionFactory();

				if (_sessionFactory == null)
				{
					throw new InvalidOperationException("cfg.BuildSessionFactory() returned null.");
				}*/
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		internal ISessionFactory SessionFactory { get { return _sessionFactory; } private set { _sessionFactory = value; } }
		//public KicSession		Session { get { return _session; } private set { _session = value; } }
		//public ITransaction		Transaction { get { return _transaction; } private set { _transaction = value; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			//CloseSession();

			if (_sessionFactory != null && _sessionFactory.IsClosed == false)
				_sessionFactory.Close();

			_sessionFactory = null;
		}
		#endregion

		#region OpenSession()
		/*public void OpenSession(IInterceptor interceptor)
		{
			if (this.Session != null && this.Session.IsOpen)
				throw new CacheException("You cannot register an interceptor once a session has already been opened");

			OpenSessionInternal(interceptor);
		}*/
		#endregion

		#region OpenSession()
		/*public KicSession OpenSession()
		{
			return OpenSessionInternal(null);
		}*/
		#endregion

		#region CloseSession()
		/*public void CloseSession()
		{
			if (this.Session != null && this.Session.IsOpen)
				this.Session.Close();

			this.Session = null;
			this.Transaction = null;
		}*/
		#endregion

		#region BeginTransaction()
		/*public void BeginTransaction()
		{
			if (this.Transaction == null)
			{
				if (this.Session == null)
					this.Transaction = OpenSessionInternal(null).BeginTransaction();
				else
					this.Transaction = this.Session.BeginTransaction();
			}
		}*/
		#endregion

		#region CommitTransaction()
		/*public void CommitTransaction()
		{
			try
			{
				if (this.Transaction != null && !this.Transaction.WasCommitted && !this.Transaction.WasRolledBack)
					this.Transaction.Commit();
			}
			catch (HibernateException)
			{
				RollbackTransaction();
				throw;
			}
		}*/
		#endregion

		#region RollbackTransaction()
		/*public void RollbackTransaction()
		{
			try
			{
				if (this.Transaction != null && !this.Transaction.WasCommitted && !this.Transaction.WasRolledBack)
					this.Transaction.Rollback();
			}
			finally
			{
				//CloseSession();
			}
		}*/
		#endregion

		#region Save()
		public void Save(object obj)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						session.Save(obj);
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}

		public void Save(List<object> objects)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						foreach (object obj in objects)
							session.Save(obj);					
						
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}
		#endregion

		#region SaveOrUpdate()
		public void SaveOrUpdate(object obj)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						session.SaveOrUpdate(obj);						
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}

		public void SaveOrUpdate(List<object> objects)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						foreach (object obj in objects)
							session.SaveOrUpdate(obj);
						
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}
		#endregion

		#region Update()
		public void Update(object obj)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						session.Update(obj);
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}

		public void Update(List<object> objects)
		{
			using (ISession session = this.SessionFactory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					try
					{
						foreach (object obj in objects)
							session.Update(obj);

						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						session.Clear();
						throw;
					}
				}
			}
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region OpenSessionInternal()
		/*private KicSession OpenSessionInternal(IInterceptor interceptor)
		{
			if (this.Session != null)
				throw new ApplicationException("There is already open session!");

			if (interceptor != null)
				this.Session = new KicSession(_sessionFactory.OpenSession(interceptor));
			else
				this.Session = new KicSession(_sessionFactory.OpenSession());

			if (this.Session == null)
				// It would be more appropriate to throw a more specific exception than ApplicationException
				throw new ApplicationException("Can't open session!");

			return this.Session;
		}*/
		#endregion

		#region CreateSQLiteDatabaseFile()
		protected void CreateSQLiteDatabaseFile(string connectionString)
		{
			FileInfo sqlFile = ExtractSQLiteDatabaseFile(connectionString);

			sqlFile.Directory.Create();
			bool exists = sqlFile.Exists;

			if (exists == false)
			{
				FileInfo sourceFile = new FileInfo(Path.Combine(Misc.Miscelaneous.StartupDir.FullName, @"Data\BscanILLData.db3"));

				sourceFile.CopyTo(sqlFile.FullName);
				sqlFile.Refresh();
				
				Misc.Io.SetFullControl(sqlFile);
			}
		}
		#endregion

		#region ExtractSQLiteDatabaseFile()
		protected FileInfo ExtractSQLiteDatabaseFile(string connectionString)
		{
			//Data Source="C:\ProgramData\DLSG\BscanILL\Data\BscanILLData.db3"
			int index = connectionString.ToLower().IndexOf("source");
			int indexDoubleQuote1 = connectionString.IndexOf('\"', index);
			int indexDoubleQuote2 = connectionString.IndexOf('\"', indexDoubleQuote1 + 1);

			if (indexDoubleQuote1 < 0 && indexDoubleQuote2 < 0)
			{
				indexDoubleQuote1 = connectionString.IndexOf('\'', index);
				indexDoubleQuote2 = connectionString.IndexOf('\'', indexDoubleQuote1 + 1);
			}

			if (indexDoubleQuote1 > 0 && indexDoubleQuote2 > 0)
			{
				string file = connectionString.Substring(indexDoubleQuote1 + 1, indexDoubleQuote2 - indexDoubleQuote1 - 1);

				return new FileInfo(file);
			}
			else
				throw new Exception("Can't extract database file path from conncetion string '" + connectionString + "'!");
		}
		#endregion

		#endregion

	}
}
