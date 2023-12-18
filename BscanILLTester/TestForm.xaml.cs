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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Printing;
using System.Security;
using System.Net;
using System.Collections;


namespace BscanILLTester
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class TestForm : Window
	{
		Hashtable dictionary = new Hashtable();
		Scanners.S2N.Bookeye4 bookeye4 = null;
		int index = 0;


		#region constructor
		public TestForm()
		{
			InitializeComponent();
			this.DataContext = this;
		}
		#endregion


		//PUBLIC PROPERTIES
		#region public properties

		#region Enabled
		private bool Enabled
		{
			set { this.IsEnabled = value; }
		}
		#endregion

		#endregion


		//PRIVATE PROPERTIES
		#region private properties

		#endregion


		//PRIVATE METHODS
		#region private methods

		#region WndProc()
		IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			return IntPtr.Zero;
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
		}
		#endregion

		#region Log()
		private void Log(string log)
		{
			this.Dispatcher.Invoke((Action)delegate() { LogTU(log); });
		}
		#endregion

		#region LogTu()
		private void LogTU(string log)
		{
			this.richTextBox.Text = string.Format("{0}: {1}\n{2}", DateTime.Now.ToString("HH:mm:ss.ff"), log, this.richTextBox.Text);
		}
		#endregion

		#region Test1_Click()
		private void Test1_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Scanners.Twain.AdfSettings scanSettings = BscanILL.SETTINGS.ScanSettings.Instance.Adf;
				Scanners.Twain.AdfSettings scanSettings2 = scanSettings;
				scanSettings.Duplex.Value = false;

				Console.WriteLine(scanSettings.Duplex.Value);
				Console.WriteLine(BscanILL.SETTINGS.ScanSettings.Instance.Adf.Duplex.Value);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test2_Click()
		private void Test2_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLPostBuild.PostBuildCode postBuildCode = new BscanILLPostBuild.PostBuildCode();

				postBuildCode.Uninstall(dictionary);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test3_Click()
		private void Test3_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILL.UI.Settings.SettingsDlg dlg = new BscanILL.UI.Settings.SettingsDlg(null, null);
				dlg.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test4_Click()
		private void Test4_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILLData.Models.DbArticle dbArticle = new BscanILLData.Models.DbArticle()
				{
					Id = 166,
					TransactionNumber = 12345678,
					IllNumber = "654321",
					Patron = "patron",
					Address = "jirka.stybnar@dlsg.net",
					ExportType = BscanILLData.Models.ExportType.ArticleExchange,
					CreationDate = DateTime.Now,
					LastModifiedDate = DateTime.Now,
					Status = BscanILLData.Models.ArticleStatus.Creating,
					FolderName = "0000000166"
				};

				BscanILL.Hierarchy.Article article = new BscanILL.Hierarchy.Article(dbArticle);
				BscanILL.UI.Dialogs.ExportDialog.ExportDlg dlg = new BscanILL.UI.Dialogs.ExportDialog.ExportDlg( null );
                
				BscanILL.Export.ExportUnit exportUnit = dlg.Open(article, 1, 1, true);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test5_Click()
		private void Test5_Click(object sender, RoutedEventArgs e)
		{			
			try
			{
				Thread t = new Thread(new ThreadStart(Test5));
				t.SetApartmentState(ApartmentState.STA);
				t.Name = "ThreadPdfBuilder_QueueFormatFile";
				t.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				t.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				t.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test5()
		private void Test5()
		{			
			try
			{
#if IRIS_ENGINE
				BscanILL.Export.ExportFiles.Iris iris = BscanILL.Export.ExportFiles.Iris.Instance;

				FileInfo reducedImage = new FileInfo(@"C:\delete\pdf\000000_ocr.tif");
				FileInfo destXml = new FileInfo(@"C:\delete\pdf\000000_ocr.ocr");

				iris.FormatImage(reducedImage, destXml);
#endif
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region ScannerConnect_Click()
		private void ScannerConnect_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Scanners.Settings settings = Scanners.Settings.Instance;

				settings.General.ScannerType = Scanners.SettingsScannerType.S2N;
				settings.S2NScanner.Ip = "192.168.6.11";
				bookeye4 = Scanners.S2N.Bookeye4.GetInstance("192.168.6.11");
				this.bookeye4.ScanRequest += new Scanners.S2N.ScanRequestHnd(Bookeye4_ScanRequest);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region ScannerGetSettings_Click()
		private void ScannerGetSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DateTime start = DateTime.Now;
				string settingsText = bookeye4.Settings.ToString();
				Console.WriteLine(DateTime.Now.Subtract(start).ToString());

				settingsText.Replace("+", Environment.NewLine);
				this.richTextBox.Text = settingsText;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region ScannerSetSettings_Click()
		private void ScannerSetSettings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				//for (int i = 0; i < 10; i++)
				{
					DateTime start = DateTime.Now;
					if((index++ % 2) == 0)
						bookeye4.Settings.ColorMode.Value = Scanners.S2N.ColorMode.Grayscale;
					else
						bookeye4.Settings.ColorMode.Value = Scanners.S2N.ColorMode.Lineart;
					
					//bookeye4.SetDevice(null);
					Console.WriteLine(DateTime.Now.Subtract(start).ToString());
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Scan_Click()
		private void Scan_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				DateTime start = DateTime.Now;
				string path = @"c:\delete\" + DateTime.Now.ToString("HH-mm-ss") + "tiff";
				bookeye4.Scan(path);
				Console.WriteLine(DateTime.Now.Subtract(start).ToString());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region CheckScannerStatus_CheckedChanged
		private void CheckScannerStatus_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (this.checkScannerButtonPress.IsChecked.Value)
				this.bookeye4.StartTouchScreenMonitoring();
			else
				this.bookeye4.StopTouchScreenMonitoring();
		}
		#endregion

		#region Bookeye4_ScanRequest
		void Bookeye4_ScanRequest(Scanners.S2N.ScannerScanAreaSelection scanArea)
		{
			Scan_Click(null, null);
		}
		#endregion

		#region CheckScannerLockUi_CheckedChanged
		private void CheckScannerLockUi_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if (this.checkScannerLockUi.IsChecked.Value)
				this.bookeye4.LockUi( false );
			else
				this.bookeye4.UnlockUi( false );
		}
		#endregion

		#region GetOcrImage()
		private FileInfo GetOcrImage()
		{
			FileInfo source = new FileInfo(@"C:\ProgramData\DLSG\BscanILL\Articles\0000000164\Pages\000006.png");
			FileInfo ocrImage;

			using (ImageProcessing.BigImages.ItDecoder itDecoder = new ImageProcessing.BigImages.ItDecoder(source.FullName))
			{
				string extension = (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite) ? "tif" : "jpg";
				ocrImage = new FileInfo(string.Format("{0}\\{1}_ocr.{2}", source.Directory.FullName, System.IO.Path.GetFileNameWithoutExtension(source.Name), extension));

				ocrImage.Refresh();
				if (ocrImage.Exists)
					ocrImage.Delete();

				ocrImage.Refresh();

				ImageProcessing.BigImages.ResizingAndResampling resizing = new ImageProcessing.BigImages.ResizingAndResampling();

				if (itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.FormatBlackWhite)
				{
					if (itDecoder.DpiX <= 300)
					{
						ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
						copier.Copy(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.None));
					}
					else
					{
						double zoom = 300.0 / itDecoder.DpiX;
						//resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.None), ImageProcessing.PixelsFormat.FormatBlackWhite, zoom, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
						resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Tiff(ImageProcessing.IpSettings.ItImage.TiffCompression.None), ImageProcessing.PixelsFormat.FormatBlackWhite, zoom, 0, 0);
					}
				}
				else
				{					
					if (itDecoder.DpiX <= 200 && itDecoder.PixelsFormat == ImageProcessing.PixelsFormat.Format8bppGray)
					{
						ImageProcessing.BigImages.ImageCopier copier = new ImageProcessing.BigImages.ImageCopier();
						copier.Copy(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg(85));
					}
					else
					{
						double zoom = 200.0 / itDecoder.DpiX;
						//resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg(80), ImageProcessing.PixelsFormat.Format8bppGray, zoom, 0, 0, new ImageProcessing.ColorD(150, 150, 150));
						resizing.ResizeAndResample(itDecoder, ocrImage.FullName, new ImageProcessing.FileFormat.Jpeg(80), ImageProcessing.PixelsFormat.Format8bppGray, zoom, 0, 0);
					}
				}
			}

			ocrImage.Refresh();
			return ocrImage;
		}
		#endregion

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			BscanILL.SETTINGS.Settings settings = BscanILL.SETTINGS.Settings.Instance;

			BscanILL.UI.Settings.SettingsDlg dlg = new BscanILL.UI.Settings.SettingsDlg(null, null);

			dlg.ShowDialog();
		}
		#endregion

		#endregion

	}
}
