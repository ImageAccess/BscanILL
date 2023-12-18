using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient ;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;


namespace BscanILL.Export.ILL.ArielAddressBookExtractor
{
	public class DatabaseSql
	{
		#region GetAddressBook()
		public static List<AddressBookItem> GetAddressBook()
		{
			try
			{
				bool connectionOk = false;
				int attempts = 0;
				SqlConnection sqlConnection = new SqlConnection("Data Source=(local); Database='Ariel'; Integrated Security=True;Connect Timeout=10");

				while (connectionOk == false)
				{
					try
					{
						sqlConnection.Open();
						connectionOk = true;
					}
					catch (Exception ex)
					{
						if (attempts++ > 3)
							throw ex;
						else
							System.Threading.Thread.Sleep(1000);
					}
				}

				SqlCommand sqlCommand = new SqlCommand("SELECT Alias, Address, Comments, AutoRemCoverSheet FROM dbo.tblAddressBook", sqlConnection);
				List<AddressBookItem> addressBook = new List<AddressBookItem>();

				using (SqlDataReader sqlReader = sqlCommand.ExecuteReader())
				{

					while (sqlReader.Read())
					{
						AddressBookItem addressBookItem = new AddressBookItem(
							sqlReader.IsDBNull(0) ? "" : Convert.ToString(sqlReader[0]),
							sqlReader.IsDBNull(1) ? "" : Convert.ToString(sqlReader[1]),
							sqlReader.IsDBNull(2) ? "" : Convert.ToString(sqlReader[2]),
							sqlReader.IsDBNull(3) ? false : Convert.ToBoolean(sqlReader[3])
							);

						addressBook.Add(addressBookItem);
					}
				}

				return addressBook;
			}
			catch (Exception ex)
			{
				throw new Exception("SQL Database error: " + ex.Message);
			}
		}
		#endregion

	}
}
