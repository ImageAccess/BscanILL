using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace BscanILL.Export.ILL.ArielAddressBookExtractor
{
	public partial class ArielAddressBookExtractorForm : Form
	{
		public ArielAddressBookExtractorForm()
		{
			InitializeComponent();

			comboBoxVersion.SelectedIndex = 0;
		}

		private void Extract_Click(object sender, EventArgs e)
		{
			try
			{
				CreateTextFile(DatabaseSql.GetAddressBook());
				MessageBox.Show("Address book extracted successfully", "Address Book Extractor", MessageBoxButtons.OK, MessageBoxIcon.Information);
				Application.Exit();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Address Book Extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void CreateTextFile(List<AddressBookItem> addressBook)
		{
			using (StreamWriter writer = new StreamWriter(Application.StartupPath + @"\ALIASES.TXT"))
			{
				foreach(AddressBookItem addressBookItem in addressBook)
					writer.WriteLine(string.Format("{0} {1};{2};{3}", addressBookItem.Alias, addressBookItem.Address, 
						addressBookItem.Comments, addressBookItem.AutoRemoveCoverSheet ? "1" : "0"));
			}
		}

	}
}