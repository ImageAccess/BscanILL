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
using System.Security;


namespace BscanILL.UI.Settings.Panels
{
    /// <summary>
    /// Interaction logic for WorldShareILL.xaml
    /// </summary>
    public partial class WorldShareILL : PanelBase
    {        
        #region constructor
        public WorldShareILL()
        {
            InitializeComponent();

            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));
            //this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Jpeg));
            //this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Png));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Text));
            this.comboFileFormat.Items.Add(new ComboItemFileFormat(Scan.FileFormat.Audio));

            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale);
            this.comboFileExportColorMode.Items.Add(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal);

            for (int i = 1; i <= 100; i++)
            {
                this.comboFileExportQuality.Items.Add(i);
            }

            groupBox.Visibility = System.Windows.Visibility.Hidden;

            this.DataContext = this;
        }
        #endregion

        //PUBLIC PROPERTIES
        #region public properties

        #region WorldShareILLEnabled
        public bool WorldShareILLEnabled
        {
            get { return _settings.Export.WorldShareILL.Enabled; }
            set
            {
                _settings.Export.WorldShareILL.Enabled = value;
                this.groupBox.Visibility = (value) ? Visibility.Visible : Visibility.Hidden;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region ExportNameIndex
        public int ExportNameIndex
        {
            get
            {
                switch (_settings.Export.WorldShareILL.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.WorldShareILLClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.WorldShareILL.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.WorldShareILLClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.WorldShareILL.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.WorldShareILLClass.ExportNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region InstitutionalSymbol
        public string InstitutionalSymbol
        {
            get { return _settings.Export.WorldShareILL.InstSymbol; }
            set
            {
                _settings.Export.WorldShareILL.InstSymbol = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region FileFormatSelectedItem
        public ComboItemFileFormat FileFormatSelectedItem
        {
            get
            {
                foreach (ComboItemFileFormat item in comboFileFormat.Items)
                    if (item.Value == _settings.Export.WorldShareILL.ExportFileFormat)
                        return item;

                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.Export.WorldShareILL.ExportFileFormat = value.Value;
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
                    if (item == _settings.Export.WorldShareILL.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.WorldShareILL.FileExportColorMode = value;

                if (_settings.Export.WorldShareILL.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
            get { return _settings.Export.WorldShareILL.FileExportQuality; }
            set
            {
                if (value != _settings.Export.WorldShareILL.FileExportQuality)
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
                    _settings.Export.WorldShareILL.FileExportQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
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
            this.groupBox.Visibility = (this.WorldShareILLEnabled) ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        private void textInstSymbol_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = textInstSymbol.Text.ToUpper();

            if (string.Compare(textInstSymbol.Text, text, false) != 0)
            {
                textInstSymbol.Text = textInstSymbol.Text.ToUpper();
                textInstSymbol.Select(textInstSymbol.Text.Length, 0);
            }

        }

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.WorldShareILL.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.WorldShareILL.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.WorldShareILL.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
