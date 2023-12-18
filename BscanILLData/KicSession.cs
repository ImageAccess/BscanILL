using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace BscanILLData
{
	public class KicSession
	{
		ISession session;


		#region constructor
		public KicSession(ISession session )
		{
			this.session = session;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		internal ISession Session { get { return this.session; } }
		internal bool IsOpen { get { return this.session.IsOpen; } }

		#endregion


		//PUBLIC METHODS
		#region public methods

		public void Flush()
		{
			this.session.Flush();
		}

		public void Save(object obj)
		{
			this.session.Save(obj);
		}

		public void Update(object obj)
		{
			this.session.Update(obj);
		}

		public void Close()
		{
			this.session.Close();
		}

		public ITransaction BeginTransaction()
		{
			return this.session.BeginTransaction();
		}

		#endregion

	}
}
