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

namespace BscanILL.UI.Dialogs.ExportDialog
{
    /// <summary>
    /// Interaction logic for Rapido.xaml
    /// </summary>
    public partial class Rapido : PanelBase, IPanel
    {
        string apiKey = "";

        #region constructor
        public Rapido()
        {
            InitializeComponent();

            this.fileformatCtrl.Reload(BscanILL.Export.ExportType.Rapido);
            this.FileNamePrefix = BscanILL.Export.Misc.GetUniqueExportFilePrefix(Scan.FileFormat.Pdf, true);

            this.fileformatCtrl.UpdateFileParamControl += delegate ()
            {
                FileFormat_SelectionChanged();
            };

            this.DataContext = this;
        }
        #endregion

        //PUBLIC PROPERTIES
        #region public properties

        public BscanILL.Scan.FileFormat FileFormat { get { return this.fileformatCtrl.FileFormat; } }
        public bool MultiImage { get { return this.fileformatCtrl.MultiImage; } }
        public bool PdfA { get { return this.fileformatCtrl.PdfA; } }
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor { get { return this.fileparamCtrl.FileColor; } }
        public int FileQuality { get { return this.fileparamCtrl.FileQuality; } }

        #region ApiKey
        public string ApiKey
        {
            get { return this.apiKey; }
            set
            {
                this.apiKey = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #endregion

        // PUBLIC METHODS
        #region public methods

        #region LoadArticle()
        public override void LoadArticle(BscanILL.Hierarchy.Article article)
        {
            base.LoadArticle(article);

            this.FileNamePrefix = GetExportFileNamePrefixPreferringIllNumber(article);

            this.ApiKey = _settings.Export.Rapido.ApiKey;
            fileformatCtrl.FileFormat = _settings.Export.Rapido.ExportFileFormat;
            this.fileparamCtrl.FileColor = _settings.Export.Rapido.FileExportColorMode;
            this.fileparamCtrl.FileQuality = _settings.Export.Rapido.FileExportQuality;
        }
        #endregion

        #region GetAdditionalInfo()
        public BscanILL.Export.AdditionalInfo.IAdditionalInfo GetAdditionalInfo()
        {
            return new BscanILL.Export.AdditionalInfo.AdditionalRapido(this.ApiKey, this.FileNamePrefix, this.FileFormat,
                this.MultiImage, this.PdfA, this.FileColor, this.FileQuality);
        }
        #endregion

        #region FileFormat_SelectionChanged
        public void FileFormat_SelectionChanged()
        {
            if (fileformatCtrl.FileFormat == Scan.FileFormat.Pdf || fileformatCtrl.FileFormat == Scan.FileFormat.SPdf)
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
        public void FileColor_SelectionChanged()
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


        //PRIVATE METHODS
        #region private methods

        #endregion


    }
}
