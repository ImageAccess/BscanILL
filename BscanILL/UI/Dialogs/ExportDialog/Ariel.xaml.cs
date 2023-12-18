using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Reflection;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Ariel.xaml
	/// </summary>
	public partial class Ariel : PanelBase, IPanel
	{
		bool	updateIlliad;
		bool	changeStatusToRequestFinished;
		bool	updateILLiadNegativeIDs;
		List<string> avoidIpList;

		
		#region Ariel()
		public Ariel()
		{
			InitializeComponent();

			this.updateIlliad = _settings.Export.Ariel.UpdateILLiad;
			this.changeStatusToRequestFinished = _settings.Export.Ariel.ChangeRequestToFinished;
			this.updateILLiadNegativeIDs = _settings.Export.Ariel.UpdateILLiadNegativeIDs;
			this.avoidIpList = _settings.Export.Ariel.IpAddressesToNotUpdateILLiad;

			this.DataContext = this;
		}
		#endregion


		#region combo ComboItemArielVersion
		public class ComboItemArielVersion
		{
			int majorVersion = 4;

			public ComboItemArielVersion(int majorVersion)
			{
				this.majorVersion = majorVersion;
			}

			public int Value { get { return majorVersion; } }

			public override string ToString()
			{
				if (majorVersion == 3)
					return "3.X";
				else
					return "4.X";
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Scan.FileFormat FileFormat { get { return Scan.FileFormat.Tiff; } }
		public bool						MultiImage { get { return true; } }
		public bool						PdfA { get { return false; } }

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return this.updateIlliad; }
			set
			{
				this.updateIlliad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return this.changeStatusToRequestFinished; }
			set
			{
				this.changeStatusToRequestFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateArticlesWithNegativeId
		public bool UpdateArticlesWithNegativeId
		{
			get { return this.updateILLiadNegativeIDs; }
			set
			{
				this.updateILLiadNegativeIDs = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region TextAvoidIp
		public List<string> TextAvoidIp
		{
			get { return this.avoidIpList; }
			set
			{
				this.avoidIpList = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region TextAvoidIpStr
		public string TextAvoidIpStr
		{
			get 
			{
				string avoidIpStr = (this.avoidIpList.Count > 0) ? this.avoidIpList[0] : "";

				for (int i = 1; i < avoidIpList.Count; i++)
					avoidIpStr += Environment.NewLine + avoidIpList[i];

				return avoidIpStr;
			}
			set
			{
				this.avoidIpList = value.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();

				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		
		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
			return new BscanILL.Export.AdditionalInfo.AdditionalAriel(this.FileNamePrefix, this.UpdateILLiad,
				this.ChangeStatusToRequestFinished, this.UpdateArticlesWithNegativeId, this.TextAvoidIp);
		}
		#endregion
	
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBox.Visibility = (_settings.Export.Ariel.Enabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region BrowseExe_Click()
		private void BrowseExe_Click(object sender, RoutedEventArgs e)
		{
			/*try
			{
				System.Windows.Forms.OpenFileDialog selectArielExeDialog = new System.Windows.Forms.OpenFileDialog();

				string initialDirectory = "";

				try { initialDirectory = new System.IO.FileInfo(this.ArielExePath).Directory.FullName; }
				catch { }

				selectArielExeDialog.DefaultExt = "exe";
				selectArielExeDialog.FileName = "WinAriel.exe";
				selectArielExeDialog.Filter = "(*.exe)|*.exe";
				selectArielExeDialog.InitialDirectory = initialDirectory;
				selectArielExeDialog.ReadOnlyChecked = true;
				selectArielExeDialog.ShowReadOnly = true;

				if (selectArielExeDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					this.ArielExePath = selectArielExeDialog.FileName;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}*/
		}
		#endregion

		#region CheckUpdateILLiad_CheckedChanged()
		private void CheckUpdateILLiad_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.gridUpdateILLiad.IsEnabled = checkUpdateILLiad.IsChecked.Value;
		}
		#endregion

		#endregion

	}
}
