using System;
using System.Collections.Generic;
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
using System.Threading;
using System.IO;
using System.Globalization;
using System.Windows.Media.Animation;
using BscanILL.UI.Windows;


namespace WpfTester
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		SecondWindow secondWindow;

		#region constructor
		public Window1()
		{			
			InitializeComponent();
		}
		#endregion

		#region Test1_Click()
		private void Test1_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				secondWindow = new SecondWindow();

				secondWindow.Show();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
			}
		}
		#endregion

		#region Test2_Click()
		private void Test2_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				secondWindow.ShowImage(@"C:\temp\BitCoin.png");
			}
			catch (Exception ex)
			{
				MessageBox.Show(GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region Test3_Click()
		private void Test3_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				secondWindow.ShowDefaultImage();

			}
			catch (Exception ex)
			{
				MessageBox.Show(GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region Test4_Click()
		private void Test4_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				secondWindow.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region Test5_Click()
		private void Test5_Click(object sender, RoutedEventArgs e)
		{
			try
			{

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region GetSession()
		/*private Kic.Hierarchy.Session GetSession()
		{
			Kic.Database.IDatabase database = new Kic.Database.SQLiteDatabase();
			Kic.Hierarchy.User user = new Kic.Hierarchy.User(database, new DirectoryInfo(@"c:\delete"), Kic.Price.FreePrices, "Default User", "*kic932&", BscanILLUi.UserType.Normal);
			Kic.Hierarchy.Category category = user.CreateDefaultCategory();
			return new Kic.Hierarchy.Session(category, "Bscan ILL Session", BscanILLUi.Hierarchy.SessionType.Regular);
		}*/
		#endregion

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				BscanILL.UI.Settings.SettingsDlg dlg = new BscanILL.UI.Settings.SettingsDlg( null, null );

				dlg.ShowDialog();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
			}
		}
		#endregion

		#region Form_Closing()
		private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ViewPane.IP.PreviewCreator.Instance.Dispose();
		}
		#endregion

		#region Hnd_Received()
		private void Hnd_Received()
		{
			try
			{
				Console.WriteLine("in");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
			}
		}
		#endregion

		#region GetArticle()
		/*private BscanILL.Hierarchy.Article GetArticle()
		{
			BscanILL.DB.DbArticle articleDb = new BscanILL.DB.DbArticle()
			{
				pArticleId = 666,
				TransactionNumber = 313598,
				IllNumber = "33312871",
				CreationDate = DateTime.Now,
				LastModifiedDate = DateTime.Now,
				Patron = "Curta, Florin",
				Address = "128.227.193.10",
				ExportType = (byte)BscanILL.Export.ExportType.SaveOnDisk,
				FolderName = "0000000029",
				Status = 0
			};
			BscanILL.Hierarchy.Article article = new BscanILL.Hierarchy.Article(articleDb);

			FileInfo file = new FileInfo(@"C:\ProgramData\DLSG\BscanILL\Articles\0000000029\Scans\00000001.png");
			BscanILL.DB.DbScan dbPullslip = new BscanILL.DB.DbScan()
			{
				fArticleId = articleDb.pArticleId,
				PreviousId = null,
				NextId = null,
				FileName = file.Name,
				ColorMode = (byte)Scanners.Twain.ColorMode.Bitonal,
				FileFormat = (byte)Scanners.FileFormat.Png,
				Dpi = 300,
				Status = (byte)BscanILL.Hierarchy.ScanStatus.Deleted
			};
			article.AddScanPullslip(dbPullslip);

			file = new FileInfo(@"C:\ProgramData\DLSG\BscanILL\Articles\0000000029\Scans\00000002.png");
			BscanILL.DB.DbScan dbScan = new BscanILL.DB.DbScan()
			{
				fArticleId = articleDb.pArticleId,
				PreviousId = null,
				NextId = null,
				FileName = file.Name,
				ColorMode = (byte)Scanners.Twain.ColorMode.Bitonal,
				FileFormat = (byte)Scanners.FileFormat.Png,
				Dpi = 300,
				Status = (byte)BscanILL.Hierarchy.ScanStatus.Active
			};
			article.Scans.Add(new IllImage(article, dbScan));

			file = new FileInfo(@"C:\ProgramData\DLSG\BscanILL\Articles\0000000029\Scans\00000003.png");
			dbScan = new BscanILL.DB.DbScan()
			{
				fArticleId = articleDb.pArticleId,
				PreviousId = null,
				NextId = null,
				FileName = file.Name,
				ColorMode = (byte)Scanners.Twain.ColorMode.Bitonal,
				FileFormat = (byte)Scanners.FileFormat.Png,
				Dpi = 300,
				Status = (byte)BscanILL.Hierarchy.ScanStatus.Active
			};
			article.Scans.Add(new IllImage(article, dbScan));

			return article;
		}*/
		#endregion

		#region resendPanel_ExpandRequest()
		private void resendPanel_ExpandRequest()
		{
		}
		#endregion

		#region FastKeyControl_HelpClick()
		private void FastKeyControl_HelpClick()
		{
		}
		#endregion

		#region GetErrorMessage()
		private string GetErrorMessage(Exception ex)
		{
			string error = ex.Message;

			while ((ex = ex.InnerException) != null)
				error += Environment.NewLine + ex.Message;

			return error;
		}
		#endregion

	}
}
