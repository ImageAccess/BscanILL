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
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for General.xaml
	/// </summary>
	public partial class General : PanelBase
	{

		#region constructor
		public General()
		{
			InitializeComponent();            

			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region PullslipsDirectory
		public string PullslipsDirectory
		{
			get { return _settings.ILL.PullslipsDirectory; }
			set 
			{
				_settings.ILL.PullslipsDirectory = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			} 
		}
		#endregion

		#region ArticlesDirectory
		public string ArticlesDirectory
		{
			get { return _settings.General.ArticlesDir; }
			set 
			{
				_settings.General.ArticlesDir = value;
				RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
			} 
		}
		#endregion        

        #region OcrEngProfile
        public int OcrEngProfile
        {
            get
            {
                switch (_settings.General.OcrEngProfile)
                {
                    case BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed: return 0;
                    case BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Accuracy: return 1;
                    default: return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0: _settings.General.OcrEngProfile = BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed; break;
                    case 1: _settings.General.OcrEngProfile = BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Accuracy; break;
                    default: _settings.General.OcrEngProfile = BscanILL.SETTINGS.Settings.GeneralClass.OcrEngineProfile.Speed; break;
                }

                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region PdfImportColor
        public int PdfImportColor
        {
            get
            {
                switch (_settings.General.PdfImportColor)
                {
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto: return 0;
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal: return 1;
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale: return 2;
                    case BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color: return 3;
                    default: return 0;
                }
            }
            set
            {
                switch (value)
                {
                    case 0: _settings.General.PdfImportColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto; break;                        
                    case 1: _settings.General.PdfImportColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Bitonal; break;
                    case 2: _settings.General.PdfImportColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Grayscale; break;
                    case 3: _settings.General.PdfImportColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Color; break;
                    default: _settings.General.PdfImportColor = BscanILL.SETTINGS.Settings.GeneralClass.PdfColorDepth.Auto; break;                        
                }
           
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region PdfImportDpi
        public int PdfImportDpi
        {
            get
            {                
                switch (_settings.General.PdfImportDpi)
                {                        
                    case 150: return 0;
                    case 200: return 1;
                    case 250: return 2;
                    case 300: return 3;
                    case 350: return 4;
                    case 400: return 5;
                    default: return 3;
                }


            }
            set
            {                                               
                switch (value)
                {
                    case 0: _settings.General.PdfImportDpi = 150; break;
                    case 1: _settings.General.PdfImportDpi = 200; break;
                    case 2: _settings.General.PdfImportDpi = 250; break;
                    case 3: _settings.General.PdfImportDpi = 300; break;
                    case 4: _settings.General.PdfImportDpi = 350; break;
                    case 5: _settings.General.PdfImportDpi = 400; break;
                    default: _settings.General.PdfImportDpi = 300; break;
                }
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);                
            }
        }
        #endregion

        #region ForceDefaultPdfParams
        public bool ForceDefaultPdfParams
        {
            get { return _settings.General.ForceDefaultPdfParams; }
            set
            {
                _settings.General.ForceDefaultPdfParams = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region MultipleArticlesSupport
        public bool MultipleArticlesSupport
        {
            get { return _settings.General.MultiArticleSupportEnabled; }
            set
            {
                _settings.General.MultiArticleSupportEnabled = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region CheckPreviewWindowBind
        public bool CheckPreviewWindowBind
        {
            get { return _settings.General.PreviewWindowEnabled; }
            set
            {
                _settings.General.PreviewWindowEnabled = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

        #region CheckExportDialogBind
        public bool CheckExportDialogBind
        {
            get { return _settings.General.ExportDialogEnabled; }
            set
            {
                _settings.General.ExportDialogEnabled = value;
                RaisePropertyChanged(MethodBase.GetCurrentMethod().Name);
            }
        }
        #endregion

		#endregion


		//PUBLIC METHODS
		#region public methods
		#endregion


		//PRIVATE METHODS
		#region private methods
        #region CheckMultipleArticles_CheckedChanged()
        private void CheckMultipleArticles_CheckedChanged(object sender, RoutedEventArgs e)
        {
            //this.gridMultiArticleGroup.IsEnabled = checkMultipleArticles.IsChecked.Value;
            //if( ! checkMultipleArticles.IsChecked.Value )
            //{
            //    checkArticlesExportDialog.IsChecked = true;  //force export dialog when single article mode
            //}
        }
        #endregion
		#endregion

	}
}
