using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BscanILL.UI.Controls.ScannerControls
{
    /// <summary>
    /// Interaction logic for PullslipAdfControl.xaml
    /// </summary>
    public partial class PullslipAdfControl : UserControl, IScannerControl
    {
        #region constructor
        public PullslipAdfControl()
        {
            InitializeComponent();
            
            GradientStopCollection gradientStopCollection = new GradientStopCollection(4) ;
            gradientStopCollection.Add( new GradientStop( System.Windows.Media.Color.  FromRgb(50,50,50) , 0) ) ;
            gradientStopCollection.Add(new GradientStop(System.Windows.Media.Color.FromRgb(158, 158, 158), 0.33));
            gradientStopCollection.Add(new GradientStop(System.Windows.Media.Color.FromRgb(158, 158, 158), 0.66));
            gradientStopCollection.Add( new GradientStop( System.Windows.Media.Color.FromRgb(50,50,50) , 1) ) ;

            this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.Adf;
            this.brightContrControl.gridBrightness.IsEnabled = false;
            this.brightContrControl.rectBrightPointer.Fill = new LinearGradientBrush(gradientStopCollection, new Point( 1, 0) , new Point( 0, 0) ) ;            
        }
        #endregion

        // PUBLIC PROPERTIES
        #region public properties

        #region ColorMode
        public Scanners.ColorMode ColorMode
        {
            get
            {
/*
                switch (this.colorModeControl.Value)
                {
                    case Scanners.Twain.ColorMode.Bitonal: return Scanners.ColorMode.Bitonal;
                    case Scanners.Twain.ColorMode.Grayscale: return Scanners.ColorMode.Grayscale;
                    default: return Scanners.ColorMode.Color;
                }
*/
                return Scanners.ColorMode.Bitonal;
            }
        }
        #endregion

        #region FileFormat
        public Scanners.FileFormat FileFormat
        {
            get
            {
/*
                switch (this.fileFormatControl.Value)
                {
                    case Scanners.Twain.FileFormat.Tiff: return Scanners.FileFormat.Tiff;
                    case Scanners.Twain.FileFormat.Png: return Scanners.FileFormat.Png;
                    default: return Scanners.FileFormat.Jpeg;
                }
*/
                return Scanners.FileFormat.Tiff;
            }
        }
        #endregion

        #region Dpi
        public ushort Dpi
        {
            //get { return this.dpiControl.Value; }
            get { return 300; }
        }
        #endregion

        #region Brightness
        public double Brightness
        {
            get { return this.brightContrControl.Brightness; }
        }
        #endregion

        #region Contrast
        public double Contrast
        {
            get { return this.brightContrControl.Contrast; }
        }
        #endregion

        #endregion



        // PRIVATE METHODS
        #region private methods

        #endregion
    }
}
