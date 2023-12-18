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
//using System.Windows.Forms;
using System.IO;
using BscanILL.Export.ILL;
using BscanILL.Export.ILLiad;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for ILLiad.xaml
	/// </summary>
	public partial class ILLiad : PanelBase
	{
		
		#region ILLiad()
		public ILLiad()
		{
			InitializeComponent();

			foreach (BscanILL.Export.ILLiad.ILLiadVersion item in Enum.GetValues(typeof(BscanILL.Export.ILLiad.ILLiadVersion)))
				this.comboVersion.Items.Add(new ComboItemILLiadVersion(item));

			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));

            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal);

            for (int i = 1; i <= 100; i++)
            {
                this.comboFileExportQuality.Items.Add(i);
            }

			this.comboPullslipTnOrientation.Items.Add(new ComboItemPullslipTnOrientation(BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.Vertical));
			this.comboPullslipTnOrientation.Items.Add(new ComboItemPullslipTnOrientation(BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.Horizontal));
			this.comboPullslipTnOrientation.Items.Add(new ComboItemPullslipTnOrientation(BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.VerticalOrHorizontal));

			this.groupBox.Visibility = System.Windows.Visibility.Hidden;
			this.DataContext = this;
		}
		#endregion


		#region class ComboItemILLiadVersion
		public class ComboItemILLiadVersion
		{
			BscanILL.Export.ILLiad.ILLiadVersion version;

			public ComboItemILLiadVersion(BscanILL.Export.ILLiad.ILLiadVersion version)
			{
				this.version = version;
			}

			public BscanILL.Export.ILLiad.ILLiadVersion Value { get { return version; } }

			public override string ToString()
			{
				switch (version)
				{
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_1_8_0: return "ILLiad 7.1.8.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_2_0_0: return "ILLiad 7.2.0.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_3_0_0: return "ILLiad 7.3.0.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version7_4_0_0: return "ILLiad 7.4.0.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_0_0_0: return "ILLiad 8.0.0.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_1_0_0: return "ILLiad 8.1.0.0";
					case BscanILL.Export.ILLiad.ILLiadVersion.Version8_1_4_0: return "ILLiad 8.1.4.0";
					default: return "Unsupported Version!";
				}
			}
		}
		#endregion

		#region class ComboItemPullslipTnOrientation
		public class ComboItemPullslipTnOrientation
		{
			BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation value;

			public ComboItemPullslipTnOrientation(BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation value)
			{
				this.value = value;
			}

			public BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation Value { get { return value; } }

			public override string ToString()
			{
				switch (value)
				{
					case BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.Horizontal: return "Horizontal";
					case BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.Vertical: return "Vertical";
					case BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.TnOrientation.VerticalOrHorizontal: return "Vertical or Horizontal";
				}

				return value.ToString();
			}
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region ILLiadEnabled
		public bool ILLiadEnabled
		{
			get { return _settings.Export.ILLiad.Enabled; }
			set
			{
				if (_settings.Export.ILLiad.Enabled != value)
				{
					_settings.Export.ILLiad.Enabled = value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region ILLiadVersionSelectedItem
		public ComboItemILLiadVersion ILLiadVersionSelectedItem
		{
			get 
			{
				foreach (ComboItemILLiadVersion item in comboVersion.Items)
					if (item.Value == _settings.Export.ILLiad.Version)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.ILLiad.Version = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region FileFormatSelectedItem
		public ComboItemFileFormat FileFormatSelectedItem
		{
			get 
			{
				foreach (ComboItemFileFormat item in comboFileFormat.Items)
					if (item.Value == _settings.Export.ILLiad.ExportFileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.ILLiad.ExportFileFormat = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

        #region FileExportColorModeSelectedItem
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileExportColorModeSelectedItem
        {
            get
            {
                foreach (BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth item in comboFileExportColorMode.Items)
                    if (item == _settings.Export.ILLiad.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.ILLiad.FileExportColorMode = value;

                if (_settings.Export.ILLiad.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                }
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion 

        #region FileExportQuality
        public int FileExportQuality
        {
            get { return _settings.Export.ILLiad.FileExportQuality; }
            set
            {
                if (value != _settings.Export.ILLiad.FileExportQuality)
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    else
                        if (value > 100)
                        {
                            value = 100;
                        }
                    _settings.Export.ILLiad.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

		#region UpdateExtraMenuItems
		public int UpdateExtraMenuItems
		{
			get { return _settings.Export.ILLiad.UpdateExtraMenuItems; }
			set
			{
				_settings.Export.ILLiad.UpdateExtraMenuItems = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region ExportToOdysseyHelper
		public bool ExportToOdysseyHelper
		{
			get { return _settings.Export.ILLiad.ExportToOdysseyHelper; }
			set
			{
				_settings.Export.ILLiad.ExportToOdysseyHelper = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region OdysseyHelperDir
		public string OdysseyHelperDir
		{
			get { return _settings.Export.ILLiad.OdysseyHelperDir; }
			set
			{
				_settings.Export.ILLiad.OdysseyHelperDir = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region CheckSql
		public bool CheckSql
		{
			get { return _settings.Export.ILLiad.SqlEnabled; }
			set
			{
				_settings.Export.ILLiad.SqlEnabled = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SqlServer
		public string SqlServer
		{
			get { return _settings.Export.ILLiad.SqlServerUri; }
			set
			{
				_settings.Export.ILLiad.SqlServerUri = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region Port
		public ushort Port
		{
			get { return _settings.Export.ILLiad.SqlPort; }
			set
			{
				_settings.Export.ILLiad.SqlPort = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SqlDatabase
		public string SqlDatabase
		{
			get { return _settings.Export.ILLiad.SqlDatabaseName; }
			set
			{
				_settings.Export.ILLiad.SqlDatabaseName = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SqlWindowsCredentials
		public bool SqlWindowsCredentials
		{
			get { return _settings.Export.ILLiad.SqlWindowsCredentials; }
			set
			{
				_settings.Export.ILLiad.SqlWindowsCredentials = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SqlUsername
		public string SqlUsername
		{
			get { return _settings.Export.ILLiad.SqlUsername; }
			set
			{
				_settings.Export.ILLiad.SqlUsername = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region SqlPassword
		public string SqlPassword
		{
			get { return _settings.Export.ILLiad.SqlPasswordText; }
			set
			{
				_settings.Export.ILLiad.SqlPasswordText = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region PullslipTnOrientationSelectedItem
		public ComboItemPullslipTnOrientation PullslipTnOrientationSelectedItem
		{
			get 
			{
				foreach (ComboItemPullslipTnOrientation item in comboPullslipTnOrientation.Items)
					if (item.Value == _settings.Export.ILLiad.PullslipTnOrientation)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					_settings.Export.ILLiad.PullslipTnOrientation = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region PullslipTnZone
		public string PullslipTnZone
		{
			get 
			{
				System.Drawing.Rectangle r = _settings.Export.ILLiad.PullslipTnZone;
				return string.Format("{0:0.00},{1:0.00},{2:0.00},{3:0.00}", r.X / 100.0, r.Y / 100.0, r.Width / 100.0, r.Height / 100.0);
			}
			set
			{
				try
				{
					string[] array = value.Split(',');
					int x = (int)(double.Parse(array[0]) * 100);
					int y = (int)(double.Parse(array[1]) * 100);
					int w = (int)(double.Parse(array[2]) * 100);
					int h = (int)(double.Parse(array[3]) * 100);

					_settings.Export.ILLiad.PullslipTnZone = new System.Drawing.Rectangle(x, y, w, h);
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
				catch (Exception ex)
				{
					MessageBox.Show("Transaction # Zone parsing error: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
		#endregion

		#region PullslipTnMin
		public int PullslipTnMin
		{
			get { return _settings.Export.ILLiad.PullslipTnMin; }
			set
			{
				_settings.Export.ILLiad.PullslipTnMin = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

		#region PullslipTnMax
		public int PullslipTnMax
		{
			get { return _settings.Export.ILLiad.PullslipTnMax; }
			set
			{
				_settings.Export.ILLiad.PullslipTnMax = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			}
		}
		#endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.ILLiad.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.ILLiad.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.ILLiad.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.ILLiadClass.ExportNameBasedOn.IllName;

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
			this.groupBox.Visibility = (this.ILLiadEnabled) ? Visibility.Visible : Visibility.Hidden;
		}
		#endregion

		#region CheckExportToOdysseyHelper_CheckedChanged()
		private void CheckExportToOdysseyHelper_CheckedChanged(object sender, RoutedEventArgs e)
		{
			this.gridExportToOdysseyHelper.IsEnabled = checkExportToOdysseyHelper.IsChecked.Value;
		}
		#endregion

		#region BrowseOdysseyHelperDir_Click()
		private void BrowseOdysseyHelperDir_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Forms.FolderBrowserDialog browserDlg = new System.Windows.Forms.FolderBrowserDialog();

				try { browserDlg.SelectedPath = (BscanILL.Misc.Io.DirectoryExists(this.OdysseyHelperDir)) ? this.OdysseyHelperDir : @"c:\"; }
				catch { browserDlg.SelectedPath = @"c:\"; }

				browserDlg.ShowNewFolderButton = true;
				browserDlg.Description = "Please select directory where the articles will be stored when selecting ILLiad.";

				if (browserDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					this.OdysseyHelperDir = browserDlg.SelectedPath;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region CheckSql_CheckedChanged()
		private void CheckSql_CheckedChanged(object sender, RoutedEventArgs e)
		{
			gridCheckSql.IsEnabled = this.checkSql.IsChecked.Value;
		}
		#endregion

		#region TestConnection_Click()
		private void TestConnection_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = System.Windows.Input.Cursors.Wait;
				
				if (comboVersion.SelectedIndex == 5)
				{
					DatabaseSQL8_1 db = new DatabaseSQL8_1(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					db.Logout();

					MessageBox.Show("Connection to the database was successfull.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else if (comboVersion.SelectedIndex == 3 || comboVersion.SelectedIndex == 4)
				{
					DatabaseSQL7_4 db = new DatabaseSQL7_4(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					db.Logout();

					MessageBox.Show("Connection to the database was successfull.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else if (comboVersion.SelectedIndex == 2)
				{
					DatabaseSQL7_3 db = new DatabaseSQL7_3(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					db.Logout();

					MessageBox.Show("Connection to the database was successfull.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					DatabaseSQL7_2 db = new DatabaseSQL7_2(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					db.Logout();

					MessageBox.Show("Connection to the database was successfull.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Connection to the database failed. " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region RequestArticle_Click()
		private void RequestArticle_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILL.Export.ILL.TransactionPair pair = null;

				if (comboVersion.SelectedIndex == 6)
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;
					DatabaseSQL8_1_4_0 db = new DatabaseSQL8_1_4_0(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					pair = db.GetRequest(int.Parse(textArticleId.Text));
					db.Logout();
				}
				else if (comboVersion.SelectedIndex == 5)
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;
					DatabaseSQL8_1 db = new DatabaseSQL8_1(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					pair = db.GetRequest(int.Parse(textArticleId.Text));
					db.Logout();
				}
				else if (comboVersion.SelectedIndex == 3 || comboVersion.SelectedIndex == 4)
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;
					DatabaseSQL7_4 db = new DatabaseSQL7_4(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					pair = db.GetRequest(int.Parse(textArticleId.Text));
					db.Logout();
				}
				else if (comboVersion.SelectedIndex == 2)
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;
					DatabaseSQL7_3 db = new DatabaseSQL7_3(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					pair = db.GetRequest(int.Parse(textArticleId.Text));
					db.Logout();
				}
				else
				{
					this.IsEnabled = false;
					this.Cursor = Cursors.Wait;

					DatabaseSQL7_2 db = new DatabaseSQL7_2(this.SqlServer, this.SqlDatabase,
					this.SqlWindowsCredentials, this.SqlUsername, this.SqlPassword);

					db.Login();
					pair = db.GetRequest(int.Parse(textArticleId.Text));
					db.Logout();
				}

				if (pair != null && pair.TransactionRow != null)
				{
					BscanILL.UI.Settings.Dialogs.ArticleForm form = new BscanILL.UI.Settings.Dialogs.ArticleForm();
					form.Open(pair.TransactionRow, pair.LenderAddressRow);
				}
				else
					MessageBox.Show("Transaction '" + textArticleId.Text + "' is not in the database!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region ComboVersion_SelectinChanged()
		private void ComboVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			BscanILL.Export.ILLiad.ILLiadVersion v = ((ComboItemILLiadVersion)comboVersion.SelectedItem).Value;
			
			gridAdditionalLines.Visibility = (v == BscanILL.Export.ILLiad.ILLiadVersion.Version7_4_0_0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
			groupBoxUpdateILLiad.Visibility = ((int)v >= (int)BscanILL.Export.ILLiad.ILLiadVersion.Version7_3_0_0) ? Visibility.Visible : System.Windows.Visibility.Collapsed;
		}
		#endregion

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.ILLiad.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.ILLiad.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.ILLiad.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                    this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Hidden;
                this.comboFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Hidden;
                this.textFileExportQuality.Visibility = System.Windows.Visibility.Hidden;
            }

        }
        #endregion

		#endregion

	}
}
