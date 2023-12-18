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
using System.IO;

namespace BscanILL.UI.Dialogs.ExportDialog
{
	/// <summary>
	/// Interaction logic for Odyssey.xaml
	/// </summary>
	public partial class Odyssey : PanelBase, IPanel
	{
		BscanILL.Scan.FileFormat fileFormat;

		
		#region Odyssey()
		public Odyssey()
		{
			InitializeComponent();

			this.fileFormat = _settings.Export.Odyssey.ExportFileFormat;

			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
			this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));

            this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Scan.FileFormat FileFormat { get { return this.fileFormat; } }
		public bool						MultiImage { get { return true; } }
		public bool						PdfA { get { return false; } }
		public string					OdysseyDirectory { get { return _settings.Export.Odyssey.ExportDirPath; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } } 

		#region FileFormatSelectedItem
		public ComboItemFileFormat FileFormatSelectedItem
		{
			get
			{
				foreach (ComboItemFileFormat item in comboFileFormat.Items)
					if (item.Value == this.FileFormat)
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
            this.fileFormat = _settings.Export.Odyssey.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.Odyssey.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.Odyssey.FileExportQuality;

		}
		#endregion
	
		#region GetAdditionalInfo()
		public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
		{
			//return new BscanILL.Export.AdditionalInfo.AdditionalOdyssey(this.textFileName.Text, this.FileFormat);
            return new BscanILL.Export.AdditionalInfo.AdditionalOdyssey(this.FileNamePrefix, this.FileFormat, this.FileColor, this.FileQuality);
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
