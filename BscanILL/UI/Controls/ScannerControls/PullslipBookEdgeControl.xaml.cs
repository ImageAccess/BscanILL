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
    /// Interaction logic for PullslipBookEdgeControl.xaml
    /// </summary>
    public partial class PullslipBookEdgeControl : UserControl, IScannerControl
    {
        #region constructor
        public PullslipBookEdgeControl()
        {
            InitializeComponent();

            this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.BookEdge;            
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

        //PRIVATE PROPERTIES
        #region private properties

        BscanILL.Misc.Notifications notifications { get { return BscanILL.Misc.Notifications.Instance; } }
        BscanILL.Scan.ScannersManager sm { get { return BscanILL.Scan.ScannersManager.Instance; } }

        #endregion

        // PRIVATE METHODS
        #region private methods

        #endregion
    }
}
