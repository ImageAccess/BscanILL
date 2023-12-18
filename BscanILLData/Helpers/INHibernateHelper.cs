using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace BscanILLData.Helpers
{
	public interface INHibernateHelper
	{
		// PUBLIC PROPERTIES
		//KicSession		Session { get; }
		//ITransaction	Transaction { get; }

		//PUBLIC METHODS
		void			Dispose();
		/*void			OpenSession(IInterceptor interceptor);
		KicSession		OpenSession();
		void			CloseSession();
		void			BeginTransaction();
		void			CommitTransaction();
		void			RollbackTransaction();*/

	}
}
