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
using System.Windows.Shapes;

namespace BscanILL.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for PrintOptionDlg.xaml
    /// </summary>
    public partial class PrintOptionDlg : Window
    {
        private bool forcePrint = false ;

        public PrintOptionDlg()
        {
            InitializeComponent();
        }

        // PRIVATE PROPERTIES
        #region private properties
        public bool ForcePrint
        {
            get { return forcePrint; }
            set { forcePrint = value; }
        }

        #endregion

        // PRIVATE METHODS
        #region private methods
        
        #region Print_Click()
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ForcePrint = true;
                this.DialogResult = true;                
            }
            catch (Exception)
            {
                // MessageBox.Show("Can't parse Transaction Number into number!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        #region Next_Click()
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ForcePrint = false;
                this.DialogResult = true;
            }
            catch (Exception)
            {
               // MessageBox.Show("Can't parse Transaction Number into number!", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        #endregion

        private void Window_Drag(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            this.DragMove();
        }
        #endregion
    }
}
