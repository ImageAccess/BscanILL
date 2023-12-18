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
using System.Threading;
using ViewPane.Hierarchy;
using BscanILL.Hierarchy;
using ImageProcessing.IpSettings;

namespace BscanILL.UI.Frames.ItResults
{
	/// <summary>
	/// Interaction logic for FrameScanUi.xaml
	/// </summary>
	public partial class FrameItResultsUi : UserControl
	{
		BscanILL.Hierarchy.Article		article = null;

		public event BscanILL.Misc.VoidHnd GoToStartClick;
		public event BscanILL.Misc.VoidHnd GoToScanClick;
		public event BscanILL.Misc.VoidHnd GoToExportClick;

		public event BscanILL.Misc.VoidHnd ResendClick;
		public event BscanILL.Misc.VoidHnd HelpClick;


		#region constructor
		public FrameItResultsUi()
		{
			InitializeComponent();

			this.viewPanel.Licensing.Ip = ViewPane.ImageProcessingMode.Advanced;
			//this.viewPanel.Licensing.PostProcessing = ViewPane.PostProcessingMode.Advanced;

			this.controlPanel.DoneClick += new BscanILL.Misc.VoidHnd(ControlPanel_DoneClick);
			this.controlPanel.ItSettingsClick += new BscanILL.Misc.VoidHnd(ControlPanel_ItSettingsClick);
			this.controlPanel.ResetCurrentClick += new BscanILL.Misc.VoidHnd(ControlPanel_ResetCurrentClick);
			this.controlPanel.ResetAllClick += new BscanILL.Misc.VoidHnd(ControlPanel_ResetAllClick);
			this.controlPanel.HelpClick += new BscanILL.Misc.VoidHnd(ControlPanel_HelpClick);
		}
		#endregion


		#region enum ItApplyTo
		public enum ItApplyTo
		{
			Current,
			All,
			RestOfArticle
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public BscanILL.Hierarchy.IllImage	SelectedImage { get { return GetIllImage(viewPanel.SelectedImage); } }
		//public ViewPane.ViewPanel			ViewP { get { return this.viewPanel; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public void Open(BscanILL.Hierarchy.Article article)
		{
			LoadArticle(article);
			this.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion

		#region Dispose()
		public void Dispose()
		{
			this.viewPanel.Dispose();
		}
		#endregion

		#region Reset()
		public void Reset(bool rememberItImages)
		{
			if (rememberItImages)
			{
			}
			
			this.viewPanel.Clear();
		}
		#endregion

		#region ShowDefaultImage()
		public void ShowDefaultImage()
		{
			this.viewPanel.SelectImage(null);
		}
		#endregion

		#region ShowImage()
		public void ShowImage(BscanILL.Hierarchy.IllImage illImage)
		{
			VpImage vpImage = GetVpImage(illImage);

			if (vpImage != null)
				vpImage.Select();
		}

		public void ShowImage(VpImage vpImage)
		{
			this.viewPanel.SelectImage(vpImage);
		}
		#endregion

		#region ScannerChanged()
		public void ScannerChanged(bool scannerConnected)
		{
			//buttonScanPullslip.IsEnabled = scannerConnected;
		}
		#endregion

		#region Lock()
		public void Lock()
		{
			Window parentWindow = Window.GetWindow(this);

			if(parentWindow != null)
			{
				parentWindow.IsEnabled = false;
				parentWindow.Cursor = Cursors.Wait;
			}
		}
		#endregion

		#region UnLock()
		public void UnLock()
		{
			Window parentWindow = Window.GetWindow(this);

			if (parentWindow != null)
			{
				parentWindow.IsEnabled = true;
				parentWindow.Cursor = null;
			}
		}
		#endregion

		#region GetIllImage()
		public BscanILL.Hierarchy.IllImage GetIllImage(VpImage vpImage)
		{
			if (vpImage != null && vpImage.Tag is BscanILL.Hierarchy.IllImage)
				return (BscanILL.Hierarchy.IllImage)vpImage.Tag;

			return null;
		}
		#endregion
	
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region GoToPrev_Click()
		private void GoToPrev_Click(object sender, EventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToNext_Click()
		private void GoToNext_Click(object sender, EventArgs e)
		{
			if (GoToExportClick != null)
				GoToExportClick();
		}
		#endregion

		#region ControlPanel_DoneClick()
		void ControlPanel_DoneClick()
		{
			try
			{
				Lock();
				this.viewPanel.LockWithProgress();

				Thread thread = new Thread(new ParameterizedThreadStart(ExecuteArticle));
				thread.Name = "ExecuteArticleThread";
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Start(new object[]{this.article, this.viewPanel.Images});
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);
				
				this.viewPanel.UnLock();
				UnLock();
			}
			finally
			{
			}
		}
		#endregion

		#region ExecuteArticle()
		void ExecuteArticle(object obj)
		{
			try
			{
				object[]		objArray = (object[]) obj;
				Article			article = (Article)objArray[0];
				VpImages		vpImages = (VpImages)objArray[1];
				ItExecution		itExecution = new ItExecution();

				article.DeletePages(false);

				itExecution.OperationError += delegate(Article a, Exception ex)
				{
					this.Dispatcher.Invoke((Action)delegate(){ItExecutionError(a, ex);});
				};
				
				itExecution.ProgressChanged += delegate(Article a, double progress)
				{
					this.Dispatcher.Invoke((Action)delegate(){ItExecutionProgressChanged(a, progress);});
				};

				System.IO.DirectoryInfo exportDir = new System.IO.DirectoryInfo(article.PagesPath);

				exportDir.Create();

				itExecution.Execute(article, vpImages, exportDir);
			}
			catch (Exception ex)
			{
				this.Dispatcher.Invoke((Action)delegate() { ItExecutionError(article, ex); });
			}
		}
		#endregion

		#region ItExecutionSuccessfull()
		void ItExecutionSuccessfull(Article article, List<BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage> pairs)
		{
			/*foreach (IllImage illImage in article.Scans)
				illImage.DeletePages();*/
			
			foreach (BscanILL.UI.Frames.Edit.ItExecution.ItExecutionPage pair in pairs)
			{
				IllImage illImage = GetIllImage(pair.VpImage);

				if (illImage != null)
				{
					illImage.IllPages.Add(illImage, pair.File, pair.FileFormat, pair.ColorMode, pair.Dpi, pair.Brightness, pair.Contrast);
				}
			}
			
			this.viewPanel.UnLock();
			UnLock();

			if (GoToExportClick != null)
				GoToExportClick();
		}
		#endregion

		#region ItExecutionError()
		void ItExecutionError(Article article, Exception ex)
		{
			MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "", MessageBoxButton.OK, MessageBoxImage.Error);

			this.viewPanel.UnLock();
			UnLock();
		}
		#endregion

		#region ItExecutionProgressChanged()
		void ItExecutionProgressChanged(Article article, double progress)
		{
			this.viewPanel.ProgressChanged(progress);
		}
		#endregion
	
		#region LoadArticle()
		void LoadArticle(BscanILL.Hierarchy.Article article)
		{
			if (article != null)
			{
				this.article = article;
				//this.articleControl.LoadArticle(article);
				ViewPane.Hierarchy.VpImage firstEditable = null;

				if (article.Pullslip != null)
				{
					VpImage vpImage = CreateVpImage(article.Pullslip, false);

					this.viewPanel.AddImage(vpImage);
				}

				foreach (BscanILL.Hierarchy.IllImage illImage in article.Scans)
				{
					ViewPane.Hierarchy.VpImage vpImage = CreateVpImage(illImage, true);

					if (firstEditable == null)
						firstEditable = vpImage;

					this.viewPanel.AddImage(vpImage);
				}

				if (firstEditable != null)
					firstEditable.Select();
			}
			else
			{
				Reset(false);
			}
		}
		#endregion

		#region GoToStart_Click()
		private void GoToStart_Click(object sender, RoutedEventArgs e)
		{
			if (GoToStartClick != null)
				GoToStartClick();
		}
		#endregion

		#region GoToScan_Click()
		private void GoToScan_Click(object sender, RoutedEventArgs e)
		{
			if (GoToScanClick != null)
				GoToScanClick();
		}
		#endregion

		#region GoToResend_Click
		private void GoToResend_Click(object sender, RoutedEventArgs e)
		{
			if (ResendClick != null)
				ResendClick();
		}
		#endregion

		#region Help_Click
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region GetVpImage()
		private VpImage GetVpImage(BscanILL.Hierarchy.IllImage illImage)
		{
			foreach (VpImage vpImage in this.viewPanel.Images)
				if (vpImage.Tag == illImage)
					return vpImage;

			return null;
		}
		#endregion

		#region CreateVpImage()
		private VpImage CreateVpImage(BscanILL.Hierarchy.IllImage illImage, bool itActive)
		{
			ViewPane.Hierarchy.VpImage vpImage = new ViewPane.Hierarchy.VpImage(illImage.FilePath.FullName, illImage.FilePath.FullName + ".reduced",
				illImage.FilePath.FullName + ".preview", illImage.FilePath.FullName + ".thumbnail", itActive, false);

			vpImage.Tag = illImage;
			return vpImage;
		}
		#endregion

		#region ControlPanel_ItSettingsClick()
		void ControlPanel_ItSettingsClick()
		{
			try
			{
				Lock();
				BscanILL.UI.ItSettings.ItSettingsDlg dlg = new ItSettings.ItSettingsDlg();
				dlg.ShowDialog();
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ControlPanel_ChangeDependencyClick()
		void ControlPanel_ChangeDependencyClick()
		{
			try
			{
				VpImage selectedVpImage = viewPanel.SelectedImage;

				if (selectedVpImage != null && selectedVpImage.ItImage != null && selectedVpImage.IsFixed == false)
				{
					Lock();

					BscanILL.UI.Dialogs.DependencyDlg dlg = new BscanILL.UI.Dialogs.DependencyDlg();
					BscanILL.UI.Dialogs.DependencyDlg.Result result = dlg.Open();

					if (result == BscanILL.UI.Dialogs.DependencyDlg.Result.Dependent || result == BscanILL.UI.Dialogs.DependencyDlg.Result.Independent)
					{
						ChangeDependency(dlg.ResultParameters, selectedVpImage);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				UnLock();
			}
		}
		#endregion

		#region ControlPanel_ChangeDependencyClick()
		void ControlPanel_HelpClick()
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		#region ChangeDependency()
		private void ChangeDependency(BscanILL.UI.Dialogs.DependencyDlg.DependencyParameters parameters, VpImage selectedVpImage)
		{
			ItImages itImages = this.viewPanel.ItImages;

			if (itImages != null && itImages.Count > 0)
			{
				Lock();

				ItImages itImagesList = GetRelevantImagesList(itImages, parameters, selectedVpImage.ItImage);

				if (parameters.DialogResult == BscanILL.UI.Dialogs.DependencyDlg.Result.Dependent)
				{
					BIP.Geometry.InchSize? clipSize = itImages.GetDependantClipsSize();

					if (clipSize == null)
					{
						double maxWidth = 0;
						double maxHeight = 0;

						foreach (ItImage itImage in itImagesList)
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
						float dependencyHorizontalSizeMargin = (float)BscanILL.SETTINGS.Settings.Instance.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyHorizontalToleranceInches;
						float dependencyVericalSizeMargin = (float)BscanILL.SETTINGS.Settings.Instance.ImageTreatment.AutoImageTreatment.ImageDependency.DependencyVerticalToleranceInches;

						foreach (ItImage itImage in itImagesList)
							itImage.IsIndependent = false;

						List<ItImage> exceptions = itImagesList.MakeDependantClipsSameSize(0, clipSize.Value, dependencyHorizontalSizeMargin, dependencyVericalSizeMargin);

						if (exceptions != null && exceptions.Count > 0)
						{
							string message = string.Format("For the image size inconsistency, {0} image(s) had to be changed to independent. Image(s): {1}", exceptions.Count, (itImages.IndexOf(exceptions[0]) + 1));

							for (int i = 1; i < exceptions.Count; i++)
							{
								message += string.Format(", {0}", itImages.IndexOf(exceptions[i]) + 1);
							}

							MessageBox.Show(message + ".", "", MessageBoxButton.OK, MessageBoxImage.Warning);
						}
					}
					else
						throw new Exception("Can't get clips size!");
				}
				else if (parameters.DialogResult == BscanILL.UI.Dialogs.DependencyDlg.Result.Independent)
				{
					foreach (ItImage itImage in itImagesList)
						itImage.IsIndependent = true;
				}

				/*foreach (ViewPaneLibrary.Hierarchy.VpImage vpImage in this.viewPanel.Images)
					if (vpImage.ScanData != null)
						this.viewPanel.ImageItSettingsChanged(vpImage.ScanData);*/
			}
		}
		#endregion

		#region GetRelevantImagesList()
		ItImages GetRelevantImagesList(ItImages itImages, BscanILL.UI.Dialogs.DependencyDlg.DependencyParameters parameters, ItImage selectedImage)
		{
			ItImages selectedImages = new ItImages();

			if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.All)
			{
				foreach (ItImage itImage in itImages)
				{
					if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
						selectedImages.Add(itImage);
				}
			}
			else if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.Range)
			{
				for (int i = Math.Max(0, parameters.From - 1); i <= Math.Min(itImages.Count - 1, parameters.To - 1); i++)
				{
					ItImage itImage = itImages[i];

					if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
						selectedImages.Add(itImage);
					else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
						selectedImages.Add(itImage);
				}
			}
			else if (parameters.SelectionRange == BscanILL.UI.Dialogs.DependencyDlg.SelectionRangeEnum.FromCurrentOn && selectedImage != null)
			{
				int from = itImages.IndexOf(selectedImage);

				if (from >= 0)
				{
					for (int i = from; i < itImages.Count; i++)
					{
						ItImage itImage = itImages[i];

						if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.AllSizes)
							selectedImages.Add(itImage);
						else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Portrait && itImage.InchSize.Width < itImage.InchSize.Height)
							selectedImages.Add(itImage);
						else if (parameters.SelectionFilter == BscanILL.UI.Dialogs.DependencyDlg.SelectionFilterEnum.Landscape && itImage.InchSize.Width > itImage.InchSize.Height)
							selectedImages.Add(itImage);
					}
				}
			}

			return selectedImages;
		}
		#endregion

		#region ControlPanel_ResetCurrentClick()
		void ControlPanel_ResetCurrentClick()
		{
			try
			{
				this.Lock();
				ImageProcessing.IpSettings.ItImage itImage = this.viewPanel.SelectedItImage;

				if (itImage != null && itImage.IsFixed == false)
					itImage.Reset(false);
					//this.viewPanel.ItImages.ResetItImage(itImage);
			}
			catch (Exception ex)
			{
				Notify(this, BscanILL.Misc.Notifications.Type.Error, "ControlPanel_ResetCurrentClick", ex.Message, ex);
				MessageBox.Show("Unexpected exception raised: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			finally
			{
				this.UnLock();
			}
		}
		#endregion

		#region ControlPanel_ResetAllClick()
		void ControlPanel_ResetAllClick()
		{
			try
			{
				this.Lock();
				ImageProcessing.IpSettings.ItImages itImages = this.viewPanel.ItImages;

				foreach (ImageProcessing.IpSettings.ItImage itImage in itImages)
					itImage.Reset(false);
				
				//itImages.ResetSettings();
			}
			catch (Exception ex)
			{
				Notify(this, BscanILL.Misc.Notifications.Type.Error, "ControlPanel_ResetAllClick", ex.Message, ex);
				MessageBox.Show("Unexpected exception raised: " + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			finally
			{
				this.UnLock();
			}
		}
		#endregion

		#region Notify()
		protected void Notify(object sender, BscanILL.Misc.Notifications.Type type, string methodName, string message, Exception ex)
		{
			BscanILL.Misc.Notifications.Instance.Notify(sender, type, "FrameItResultsUi, " + methodName + "(): " + message, ex);
		}
		#endregion

		#endregion

	}
}
