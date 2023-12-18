using System;
using System.Collections.Generic;
using System.Text;

namespace BscanILL.Export.ILL.ArielAddressBookExtractor
{
	public class AddressBookItem
	{
		public readonly string Alias;
		public readonly string Address;
		public readonly string Comments;
		public readonly bool AutoRemoveCoverSheet;

		public AddressBookItem(string alias, string address, string comments, bool autoRemoveCoverSheet)
		{
			this.Alias = alias;
			this.Address = address;
			this.Comments = comments;
			this.AutoRemoveCoverSheet = autoRemoveCoverSheet;
		}
	}
}


