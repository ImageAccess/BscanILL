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
    /// Interaction logic for Tipasa.xaml
    /// </summary>
    public partial class Tipasa : PanelBase
    {
        #region constructor
        public Tipasa()
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

        #region TipasaEnabled
        public bool TipasaEnabled
        {
            get { return _settings.Export.Tipasa.Enabled; }
            set
            {
                _settings.Export.Tipasa.Enabled = value;
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
                switch (_settings.Export.Tipasa.ExportNameBase)
                {
                    case BscanILL.SETTINGS.Settings.ExportClass.TipasaClass.ExportNameBasedOn.TransactionName: return 1;
                    default: return 0;
                }
            }
            set
            {
                if (value == 1)
                    _settings.Export.Tipasa.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.TipasaClass.ExportNameBasedOn.TransactionName;
                else
                    _settings.Export.Tipasa.ExportNameBase = BscanILL.SETTINGS.Settings.ExportClass.TipasaClass.ExportNameBasedOn.IllName;

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region InstitutionalSymbol
        public string InstitutionalSymbol
        {
            get { return _settings.Export.Tipasa.InstSymbol; }
            set
            {
                _settings.Export.Tipasa.InstSymbol = value;
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
                    if (item.Value == _settings.Export.Tipasa.ExportFileFormat)
                        return item;

                return null;
            }
            set
            {
                if (value != null)
                {
                    _settings.Export.Tipasa.ExportFileFormat = value.Value;
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
                    if (item == _settings.Export.Tipasa.FileExportColorMode)
                        return item;

                return BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;
            }
            set
            {
                _settings.Export.Tipasa.FileExportColorMode = value;

                if (_settings.Export.Tipasa.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
            get { return _settings.Export.Tipasa.FileExportQuality; }
            set
            {
                if (value != _settings.Export.Tipasa.FileExportQuality)
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
                    _settings.Export.Tipasa.FileExportQuality = value;
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
            this.groupBox.Visibility = (this.TipasaEnabled) ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion
        
        private void textInstSymbol_TextChanged(object sender, TextChangedEventArgs e)
        {
           string text = textInstSymbol.Text.ToUpper();

           if (string.Compare(textInstSymbol.Text, text,false) != 0)
           {
                textInstSymbol.Text = textInstSymbol.Text.ToUpper();
                textInstSymbol.Select(textInstSymbol.Text.Length,0);
           }           

        }

        #region ComboFileFormat_SelectionChanged()
        private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if(comboFileFormat.SelectedItem is ComboBoxItem)

            if (_settings.Export.Tipasa.ExportFileFormat == Scan.FileFormat.Pdf || _settings.Export.Tipasa.ExportFileFormat == Scan.FileFormat.SPdf)
            {
                this.comboFileExportColorMode.Visibility = System.Windows.Visibility.Visible;
                this.textFileExportColorMode.Visibility = System.Windows.Visibility.Visible;

                if (_settings.Export.Tipasa.FileExportColorMode != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
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
