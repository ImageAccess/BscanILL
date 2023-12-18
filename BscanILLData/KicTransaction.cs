using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace BscanILLData
{
	public class KicTransaction
	{
		ITransaction transaction;

		public KicTransaction(KicSession session)
		{
			this.transaction = session.BeginTransaction();
		}

		public void Commit()
		{
			this.transaction.Commit();
		}

		public void RollBack()
		{
			this.transaction.Rollback();
		}

	}
}
