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

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for ILLiad.xaml
	/// </summary>
	public partial class ILLiad : PanelBase, IPanel
	{
		BscanILL.Scan.FileFormat	fileFormat;
		

		#region ILLiad()
		public ILLiad()
		{
			InitializeComponent();

			this.fileFormat = _settings.Export.ILLiad.ExportFileFormat;

			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));

            this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

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

		public BscanILL.Scan.FileFormat FileFormat { get { return this.fileFormat; } }
		public bool						MultiImage { get { return true; } }
		public bool						PdfA		{ get { return false; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } }

		#region FileFormatSelectedItem
		public ComboItemFileFormat FileFormatSelectedItem
		{
			get 
			{
				foreach (ComboItemFileFormat item in comboFileFormat.Items)
					if (item.Value == this.fileFormat)
						return item;

				return null;
			}
			set
			{
				if (value != null)
				{
					this.fileFormat = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);

					//this.textFileName.Text = this.FileNamePrefix + BscanILL.Misc.Io.GetFileExtension(this.FileFormat);
				}
			}
		}
		#endregion

		#region ILLiadDirectory
		public string ILLiadDirectory
		{
			get
			{
				return _settings.Export.ILLiad.OdysseyHelperDir;
			}
		}
		#endregion

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region LoadArticle()
		public override void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			this.article = article;

			this.FileNamePrefix = GetExportFileNamePrefixPreferringTransactionNumber(article);
			//this.textFileName.Text = this.FileNamePrefix + BscanILL.Misc.Io.GetFileExtension(this.FileFormat);
			//this.ExportDirPath = System.IO.Path.Combine(_settings.Export.SaveOnDisk.ExportDirPath, this.FileNamePrefix);
			//this.ILLiadDirectory = _settings.Export.ILLiad.OdysseyHelperDir;
            this.fileFormat = _settings.Export.ILLiad.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.ILLiad.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.ILLiad.FileExportQuality;

		}
		#endregion
	
		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
			//return new BscanILL.Export.AdditionalInfo.AdditionalIlliad(this.textFileName.Text, this.FileFormat);
            return new BscanILL.Export.AdditionalInfo.AdditionalIlliad(this.FileNamePrefix, this.FileFormat, this.FileColor, this.FileQuality);            
		}
		#endregion

		#endregion


		//PRIVATE METHODS
		#region private methods

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileFormat_SelectionChanged();
        }
        #endregion

        #region FileFormat_SelectionChanged
        private void FileFormat_SelectionChanged()
        {
            //if (fileformatCtrl.FileFormat == Scan.FileFormat.Pdf || fileformatCtrl.FileFormat == Scan.FileFormat.SPdf)
            if (FileFormatSelectedItem.Value == Scan.FileFormat.Pdf || FileFormatSelectedItem.Value == Scan.FileFormat.SPdf)
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Visible;

                FileColor_SelectionChanged();
            }
            else
            {
                this.fileparamCtrl.comboFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileColor.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

        #region FileColor_SelectionChanged
        private void FileColor_SelectionChanged()
        {
            if (this.fileparamCtrl.FileColor != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Visible;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.fileparamCtrl.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                this.fileparamCtrl.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
            }
        }
        #endregion

		#endregion

	}
}
