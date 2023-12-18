using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewPane
{
	class VpException : Exception
	{

		public VpException(string message)
			:base(message)
		{
		}
	}
}
