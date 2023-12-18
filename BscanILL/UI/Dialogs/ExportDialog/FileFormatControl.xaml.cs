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
	/// Interaction logic for Ftp.xaml
	/// </summary>
	public partial class FileFormatControl : PanelBase
	{
		ObservableCollection<ComboItemFileFormat>		fileFormats = new ObservableCollection<ComboItemFileFormat>();
		BscanILL.Scan.FileFormat						fileFormat = Scan.FileFormat.Unknown;

        private BscanILL.Export.ExportType _exportType = Export.ExportType.None;
        
        public event BscanILL.Misc.VoidHnd UpdateFileParamControl;

		#region constructor
		public FileFormatControl()
		{
			InitializeComponent();

			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Jpeg));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Png));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Text));
			fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Audio));

			this.FileFormat = Scan.FileFormat.Pdf;

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		public bool MultiImage	{ get { return this.checkMultiImage.IsChecked.Value; } }
		public bool PdfA		{ get { return this.checkPdfa.IsChecked.Value; } }

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
					this.FileFormat = value.Value;
					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
				}
			}
		}
		#endregion

		#region FileFormats
		public ObservableCollection<ComboItemFileFormat> FileFormats
		{
			get { return this.fileFormats; }
		}
		#endregion

		#region FileFormat
		public BscanILL.Scan.FileFormat FileFormat
		{
			get { return this.fileFormat; }
			set
			{
				if (this.fileFormat != value)
				{
					this.fileFormat = value;

					foreach (ComboItemFileFormat item in comboFileFormat.Items)
						if (item.Value == value)
							this.FileFormatSelectedItem = item;

					if (this.fileFormat == Scan.FileFormat.Pdf || this.fileFormat == Scan.FileFormat.SPdf || this.fileFormat == Scan.FileFormat.Audio ||
						this.fileFormat == Scan.FileFormat.Text || this.fileFormat == Scan.FileFormat.Tiff)
                    {                        
                        this.checkMultiImage.Visibility = System.Windows.Visibility.Visible;
                        if ((ExportType == BscanILL.Export.ExportType.ArticleExchange) || (ExportType == BscanILL.Export.ExportType.Tipasa) || (ExportType == BscanILL.Export.ExportType.WorldShareILL) || (ExportType == BscanILL.Export.ExportType.Rapido))
                        {                            
                            this.checkMultiImage.IsEnabled = false;
                        }
                        else
                        {
                            this.checkMultiImage.IsEnabled = true;
                        }
                        
                    }					
					else
						this.checkMultiImage.Visibility = System.Windows.Visibility.Collapsed;

					this.checkPdfa.Visibility = (this.fileFormat == Scan.FileFormat.SPdf) ?  System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

					RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
  
                    if (UpdateFileParamControl != null)
                        UpdateFileParamControl();
				}
			}
		}
		#endregion	

        #region ExportType
        public BscanILL.Export.ExportType ExportType
        {
            get { return _exportType; }
            set
            {
                _exportType = value;
            }
        }

        #endregion

        #endregion


        //PRIVATE METHODS
		#region private methods

		#region ComboFileFormat_SelectionChanged()
		/*private void ComboFileFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(this.comboFileFormat.SelectedItem != null)
				this.fileFormat = ((ComboItemFileFormat)this.comboFileFormat.SelectedItem).Value;
		}*/
		#endregion

		#endregion

        //PUBLIC METHODS
        #region public methods

        #region Reload()
        public void Reload( BscanILL.Export.ExportType export )
        {
            ExportType = export;

            fileFormats.Clear();
            fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Pdf));
            fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.SPdf));
            fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Tiff));
            if ((ExportType != BscanILL.Export.ExportType.ArticleExchange) && (ExportType != BscanILL.Export.ExportType.Tipasa) && (ExportType != BscanILL.Export.ExportType.WorldShareILL)
				  && (ExportType != BscanILL.Export.ExportType.Rapido) )
            {
				//do not allow single image output for Article Exchange, Tipasa, WorldShareILL, Rapido
				fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Jpeg));
                fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Png));
            }
            fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Text));
            fileFormats.Add(new ComboItemFileFormat(Scan.FileFormat.Audio));

            this.FileFormat = Scan.FileFormat.Pdf;
            this.checkMultiImage.IsEnabled = false;

        }
        #endregion

        #endregion
    }
}
