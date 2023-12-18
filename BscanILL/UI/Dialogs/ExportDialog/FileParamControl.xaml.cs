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

namespace BscanILL.UI.Dialogs.ExportDialog
{
    /// <summary>
    /// Interaction logic for FileParamControl.xaml
    /// </summary>
    /// 

    public partial class FileParamControl : PanelBase
    {
        ObservableCollection<ComboItemFileColor> fileColors = new ObservableCollection<ComboItemFileColor>();
        BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth fileColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;

        ObservableCollection<int> fileQualities = new ObservableCollection<int>();
        int fileQuality = 100;

        #region constructor
        public FileParamControl()
        {
            InitializeComponent();


            fileColors.Add(new ComboItemFileColor(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto));
            fileColors.Add(new ComboItemFileColor(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color));
            fileColors.Add(new ComboItemFileColor(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale));
            fileColors.Add(new ComboItemFileColor(BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal));

            this.fileColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto;

            for (int i = 1; i <= 100; i++)
            {
                fileQualities.Add(i);
            }

            this.fileQuality = 100;

            this.DataContext = this;
        }
        #endregion


        //PUBLIC PROPERTIES
        #region public properties

        #region FileColorSelectedItem
        public ComboItemFileColor FileColorSelectedItem
        {
            get
            {
                foreach (ComboItemFileColor item in comboFileColor.Items)
                    if (item.Value == this.fileColor)
                        return item;

                return null;
            }
            set
            {
                if (value != null)
                {
                    this.FileColor = value.Value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion

        #region FileQualitySelectedItem
        public int FileQualitySelectedItem
        {
            get
            {
                foreach (int item in comboFileQuality.Items)
                    if (item == this.fileQuality)
                    {
                        return item;
                    }

                return 85;
            }
            set
            {                               
                    this.fileQuality = value;
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);                
            }
        }
        #endregion

        #region FileColors
        public ObservableCollection<ComboItemFileColor> FileColors
        {
            get { return this.fileColors; }
        }
        #endregion

        #region FileQualities
        public ObservableCollection<int> FileQualities
        {
            get { return this.fileQualities; }
        }
        #endregion

        #region FileColor
        public BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth FileColor
        {
            get { return this.fileColor; }
            set
            {
                if (this.fileColor != value)
                {
                    this.fileColor = value;

                    foreach (ComboItemFileColor item in comboFileColor.Items)
                        if (item.Value == value)
                            this.FileColorSelectedItem = item;

                    if (this.fileColor != BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal)
                    {
                        this.comboFileQuality.Visibility = System.Windows.Visibility.Visible;
                        this.textFileQuality.Visibility = System.Windows.Visibility.Visible;
/*
                        if ((ExportType == BscanILL.Export.ExportType.ArticleExchange) || (ExportType == BscanILL.Export.ExportType.Tipasa) || (ExportType == BscanILL.Export.ExportType.WorldShareILL))
                        {
                            this.checkMultiImage.IsEnabled = false;
                        }
                        else
                        {
                            this.checkMultiImage.IsEnabled = true;
                        }
*/
                    }
                    else
                    {
                        this.comboFileQuality.Visibility = System.Windows.Visibility.Hidden;
                        this.textFileQuality.Visibility = System.Windows.Visibility.Hidden;
                    }
                                            
                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);

                }
            }
        }
        #endregion	

        #region FileQuality
        public int FileQuality
        {
            get { return this.fileQuality; }
            set
            {
                if (this.fileQuality != value)
                {
                    this.fileQuality = value;

                    foreach (int item in comboFileQuality.Items)                        
                            if (item == value)
                            {
                                this.FileQualitySelectedItem = item;
                            }

                    RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
                }
            }
        }
        #endregion	

        #endregion
    }
}
