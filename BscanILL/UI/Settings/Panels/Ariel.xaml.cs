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

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Ariel.xaml
	/// </summary>
	public partial class Ariel : PanelBase
	{
		
		
		#region Ariel()
		public Ariel()
		{
			InitializeComponent();

			this.comboArielVersion.Items.Add(new ComboItemArielVersion(3));
			this.comboArielVersion.Items.Add(new ComboItemArielVersion(4));

			this.groupBox.Visibility = System.Windows.Visibility.Collapsed;
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

		#region ArielEnabled
		public bool ArielEnabled
		{
			get { return _settings.Export.Ariel.Enabled; }
			set
			{
				if (_settings.Export.Ariel.Enabled != value)
				{
					_settings.Export.Ariel.Enabled = value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region ArielVersionSelectedItem
		public ComboItemArielVersion ArielVersionSelectedItem
		{
			get 
			{
				foreach (ComboItemArielVersion item in comboArielVersion.Items)
					if (item.Value == _settings.Export.Ariel.MajorVersion)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.Ariel.MajorVersion = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region ArielExePath
		public string ArielExePath
		{
			get { return _settings.Export.Ariel.Executable; }
			set
			{
				_settings.Export.Ariel.Executable = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateILLiad
		public bool UpdateILLiad
		{
			get { return _settings.Export.Ariel.UpdateILLiad; }
			set
			{
				_settings.Export.Ariel.UpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ChangeStatusToRequestFinished
		public bool ChangeStatusToRequestFinished
		{
			get { return _settings.Export.Ariel.ChangeRequestToFinished; }
			set
			{
				_settings.Export.Ariel.ChangeRequestToFinished = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region UpdateArticlesWithNegativeId
		public bool UpdateArticlesWithNegativeId
		{
			get { return _settings.Export.Ariel.UpdateILLiadNegativeIDs; }            
			set
			{
				_settings.Export.Ariel.UpdateILLiadNegativeIDs = value;                
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region TextAvoidIp
		public List<string> TextAvoidIp
		{
			get { return _settings.Export.Ariel.IpAddressesToNotUpdateILLiad; }
			set
			{
				_settings.Export.Ariel.IpAddressesToNotUpdateILLiad = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.Ariel.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.ArielClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.Ariel.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ArielClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.Ariel.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ArielClass.ExportNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion
		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods

		#region Enabled_CheckedChanged()
		private void Enabled_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.groupBox.Visibility = (this.ArielEnabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region BrowseExe_Click()
		private void BrowseExe_Click(object sender, RoutedEventArgs e)
		{
			try
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
			}
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
