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
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class FormsProcessing : PanelBase
	{

		#region constructor
		public FormsProcessing()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region MainScriptPath
		public string MainScriptPath
		{
			get { return _settings.FormsProcessing.BsaFilePath; }
			set
			{
				_settings.FormsProcessing.BsaFilePath = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ProcessScriptPath
		public string ProcessScriptPath
		{
			get { return _settings.FormsProcessing.ScriptFilePath; }
			set
			{
				_settings.FormsProcessing.ScriptFilePath = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region TrainingName
		public string TrainingName
		{
			get { return _settings.FormsProcessing.TrainingName; }
			set
			{
				_settings.FormsProcessing.TrainingName = value;
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

		#region Browse_Click()
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Forms.OpenFileDialog openFileDlg = new System.Windows.Forms.OpenFileDialog();

				openFileDlg.CheckFileExists = true;
				openFileDlg.DefaultExt = ".bsa";
				openFileDlg.Multiselect = false;
				openFileDlg.Title = "Select The Script File";
				openFileDlg.Filter = "bsa files (*.bsa)|*.bsa";

				try 
				{
					if (sender == this.buttonProcessPath && File.Exists(this.ProcessScriptPath))
					{
						openFileDlg.InitialDirectory = new FileInfo(this.ProcessScriptPath).Directory.FullName;
						openFileDlg.FileName = this.ProcessScriptPath;
					}
					else if (sender == this.buttonScriptPath && File.Exists(this.MainScriptPath))
					{
						openFileDlg.InitialDirectory = new FileInfo(this.MainScriptPath).Directory.FullName;
						openFileDlg.FileName = this.MainScriptPath;
					}
				}
				catch {}

				if (openFileDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					if (sender == this.buttonProcessPath)
						this.ProcessScriptPath = openFileDlg.FileName;
					else
						this.MainScriptPath = openFileDlg.FileName;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#endregion

	}
}
