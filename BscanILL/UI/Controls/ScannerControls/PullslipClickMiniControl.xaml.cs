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
    /// Interaction logic for PullslipClickMiniControl.xaml
    /// </summary>
    public partial class PullslipClickMiniControl : UserControl, IScannerControl
    {        
		#region constructor
        public PullslipClickMiniControl()
		{
			InitializeComponent();

            this.brightContrControl.ScanSettings = BscanILL.SETTINGS.ScanSettingsPullSlips.Instance.ClickMini;
		}
		#endregion

        // PUBLIC PROPERTIES
        #region public properties

        #region ColorMode
        public Scanners.ColorMode ColorMode
        {
            get
            {
               /* switch (this.colorModeControl.Value)
                {
                    case Scanners.Click.ClickColorMode.Bitonal: return Scanners.ColorMode.Bitonal;
                    case Scanners.Click.ClickColorMode.Grayscale: return Scanners.ColorMode.Grayscale;
                    default: return Scanners.ColorMode.Color;
                }*/
                return Scanners.ColorMode.Bitonal;
            }
        }
        #endregion

        #region FileFormat
        public Scanners.FileFormat FileFormat
        {
            get
            {
               /* switch (this.fileFormatControl.Value)
                {
                    case Scanners.FileFormat.Tiff: return Scanners.FileFormat.Tiff;
                    case Scanners.FileFormat.Png: return Scanners.FileFormat.Png;
                    default: return Scanners.FileFormat.Jpeg;
                }*/
                return Scanners.FileFormat.Tiff;
            }
        }
        #endregion

        #region Dpi
        public ushort Dpi
        {
          //  get { return this.dpiControl.Value; }
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
