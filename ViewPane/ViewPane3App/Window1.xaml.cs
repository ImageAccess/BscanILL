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
using System.IO;
using ViewPane.Hierarchy;
using ViewPane.ItResults;


namespace ViewPane2App
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		ItResultsWindow			itResultsWindow = null;


		#region constructor
		public Window1()
		{
			InitializeComponent();

			ViewPane.Licensing licensing = new ViewPane.Licensing();
			licensing.Ip = ViewPane.ImageProcessingMode.Advanced;
			licensing.PostProcessing = ViewPane.PostProcessingMode.Advanced;
			this.viewPanel.Licensing = licensing;

		}
		#endregion

		
		#region Load_Click()
		private void Load_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				foreach (VpImage vpImage in GetVpImages())
					this.viewPanel.AddImage(vpImage);
				
				buttonResults.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region Results_Click()
		private void Results_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				//dlg.Open(this.viewPanel.Thumbnails);

			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region Dlg_Click
		private void Dlg_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				ViewPane.Dialogs.IpDlg dlg = new ViewPane.Dialogs.IpDlg();
				bool									advanced = false;
				List<VpImage>							images = this.viewPanel.Images;
				int										selectedIndex = this.viewPanel.SelectedImageIndex;

				if (selectedIndex >= 0)
				{
					dlg.Open(advanced, images, selectedIndex);
				}
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

		#region Window_Closing()
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			this.viewPanel.Dispose();
			//dlg.Close();
			this.itResultsWindow.Dispose();
		}
		#endregion

		#region Window_Closed()
		private void Window_Closed(object sender, EventArgs e)
		{
		}
		#endregion

		#region MakeIndependent_Click()
		void MakeIndependent_Click(object sender, EventArgs e)
		{
			try
			{
				ImageProcessing.IpSettings.ItImages itImages = this.viewPanel.ItImages;

				if (itImages != null && itImages.Count > 0)
				{
					ImageProcessing.IpSettings.ItImages itImagesList = itImages;

						foreach (ImageProcessing.IpSettings.ItImage itImage in itImagesList)
							itImage.IsIndependent = true;

					foreach (ViewPane.Hierarchy.VpImage vpImage in this.viewPanel.Images)
						if (vpImage != null)
							vpImage.ItImage.IsIndependent = true;
				}
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

		#region MakeDependent_Click()
		void MakeDependent_Click(object sender, EventArgs e)
		{
			try
			{
				ImageProcessing.IpSettings.ItImages itImages = this.viewPanel.ItImages;

				if (itImages != null && itImages.Count > 0)
				{
					ImageProcessing.IpSettings.ItImages		itImagesList = itImages;
					BIP.Geometry.InchSize?					clipSize = itImages.GetDependantClipsSize();

					if (clipSize == null)
					{
						double maxWidth = 0;
						double maxHeight = 0;

						foreach (ImageProcessing.IpSettings.ItImage itImage in itImagesList)
						{
							if (itImage.TwoPages)
							{
								if (maxWidth < itImage.PageL.ClipRectInch.Width)
									maxWidth = itImage.PageL.ClipRectInch.Width;
								if (maxHeight < itImage.PageL.ClipRectInch.Height)
									maxHeight = itImage.PageL.ClipRectInch.Height;
								if (maxWidth < itImage.PageR.ClipRectInch.Width)
									maxWidth = itImage.PageR.ClipRectInch.Width;
								if (maxHeight < itImage.PageR.ClipRectInch.Height)
									maxHeight = itImage.PageR.ClipRectInch.Height;
							}
							else
							{
								if (itImage.PageL.ClipSpecified)
								{
									if (maxWidth < itImage.PageL.ClipRectInch.Width)
										maxWidth = itImage.PageL.ClipRectInch.Width;
									if (maxHeight < itImage.PageL.ClipRectInch.Height)
										maxHeight = itImage.PageL.ClipRectInch.Height;
								}
							}
						}

						if (maxWidth > 0 && maxHeight > 0)
							clipSize = new BIP.Geometry.InchSize(maxWidth, maxHeight);
					}

					if (clipSize != null)
					{
						foreach (ImageProcessing.IpSettings.ItImage itImage in itImagesList)
							itImage.IsIndependent = false;

						List<ImageProcessing.IpSettings.ItImage> exceptions = itImagesList.MakeDependantClipsSameSize(0, clipSize.Value);

						if (exceptions != null && exceptions.Count > 0)
						{
							itImages.IndexOf(exceptions[0]);

							string message = string.Format("For the image size inconsistency" + ", {0} image(s) had to be changed to independent. Image(s): {1}", exceptions.Count, (itImages.IndexOf(exceptions[0]) + 1));

							for (int i = 1; i < exceptions.Count; i++)
								message += string.Format(", {0}", itImages.IndexOf(exceptions[i]) + 1);

							MessageBox.Show(message + ".", "", MessageBoxButton.OK, MessageBoxImage.Error);
						}
					}
					else
						throw new Exception("Can't get clips size!");

					//foreach (ViewPaneLibrary.Hierarchy.VpImage vpImage in this.viewPanel.Images)
					//	if (vpImage.ScanData != null)
					//		this.viewPanel.ImageItSettingsChanged(vpImage.ScanData);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region Test1_Click()
		private void Test1_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				this.itResultsWindow = new ItResultsWindow();
				this.itResultsWindow.Show();

				Test2_Click(null, null);
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
				this.IsEnabled = false;
				this.Cursor = Cursors.Wait;

				foreach (VpImage vpImage in this.viewPanel.Images)
				{
					itResultsWindow.ItResultsPanel.AddImage(vpImage);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				this.IsEnabled = true;
				this.Cursor = null;
			}
		}
		#endregion

		#region Test3_Click()
		private void Test3_Click(object sender, RoutedEventArgs e)
		{
			try
			{
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
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		#endregion

		#region GetVpImages()
		private List<VpImage> GetVpImages()
		{
			List<VpImage> vpImages = new List<VpImage>();
			List<FileInfo> files = new List<FileInfo>();
			files.AddRange(new DirectoryInfo(@"C:\delete\Full").GetFiles("*.tif"));
			files.AddRange(new DirectoryInfo(@"C:\delete\Full").GetFiles("*.jpg"));

			foreach (FileInfo file in files)
			{
				string reducedPath = file.FullName + ".reduced";
				string previewPath = file.FullName + ".preview";
				string thumbnailPath = file.FullName + ".thumb";

				VpImage vpImage;

				if (file.Extension.ToLower() == ".tif")
					vpImage = new VpImage(file.FullName, reducedPath, previewPath, thumbnailPath, false, false);
				else
				{
					vpImage = new VpImage(file.FullName, reducedPath, previewPath, thumbnailPath, true, false);
					vpImage.IsFixed = false;
					vpImage.ItImage.PageL.Activate(new BIP.Geometry.RatioRect(0.1, 0.1, 0.35, 0.9), false);
					vpImage.ItImage.PageR.Activate(new BIP.Geometry.RatioRect(0.6, 0.05, 0.35, 0.9), false);
				}

				vpImages.Add(vpImage);
			}

			return vpImages;
		}
		#endregion

	}
}
