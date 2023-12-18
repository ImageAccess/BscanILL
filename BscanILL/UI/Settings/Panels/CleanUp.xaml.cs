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
using System.Threading;
using System.IO;

namespace BscanILL.UI.Settings.Panels
{
	/// <summary>
	/// Interaction logic for CleanUp.xaml
	/// </summary>
	public partial class CleanUp : PanelBase
	{
        BscanILL.Hierarchy.SessionBatch batch = null;

		#region constructor
		public CleanUp()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

        #region Batch
        public BscanILL.Hierarchy.SessionBatch Batch
        {
            get { return batch; }
            set
            {
                batch = value;                
            }
        }
        #endregion

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

		#region KeepArticlesFor
		public int KeepArticlesFor
		{
			get { return _settings.General.KeepArticlesFor; }
			set 
			{
				_settings.General.KeepArticlesFor = value;
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

		#region CleanUp_Click
		private void CleanUp_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Lock();

				Thread thread = new Thread(new ThreadStart(CleanUpThread));
				thread.Name = "CleanUp, CleanUp_Click()";
				thread.SetApartmentState(ApartmentState.STA);
				thread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
				thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
				thread.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				Unlock();
			}
		}
		#endregion

		#region Lock
		private void Lock()
		{
			Window window = Window.GetWindow(this);

			if (window != null)
			{
				window.IsEnabled = false;
				window.Cursor = Cursors.Wait;
			}
		}
		#endregion

		#region Unlock
		private void Unlock()
		{
			Window window = Window.GetWindow(this);
			this.progressBar.Value = 0;
			this.textDescription.Text = "";

			if (window != null)
			{
				window.IsEnabled = true;
				window.Cursor = null;
			}
		}
		#endregion

		#region CleanUpThread()
		private void CleanUpThread()
		{
			try
			{
				BscanILL.Misc.CleanUp cleanUp = new BscanILL.Misc.CleanUp();
				cleanUp.DescriptionChanged += delegate(string description) { SetDescription(description); };
				cleanUp.ProgressChanged += delegate(double progress) { SetProgress(progress); };
				cleanUp.OperationDone += delegate() { OprerationSuccessfull(); };
				cleanUp.OperationError += delegate(Exception ex) { OprerationError(ex); };

				//cleanUp.Execute(null, new DirectoryInfo(_settings.General.ArticlesDir));
                cleanUp.Execute(batch, new DirectoryInfo(_settings.General.ArticlesDir));                
			}
			catch (Exception ex)
			{
				OprerationError(ex);
			}
		}
		#endregion

		#region SetProgress()
		private void SetProgress(double progress)
		{
			this.Dispatcher.Invoke((Action)delegate(){this.progressBar.Value = (progress * 100);});
		}
		#endregion

		#region SetDescription()
		private void SetDescription(string description)
		{
			this.Dispatcher.Invoke((Action)delegate() { this.textDescription.Text = description; });
		}
		#endregion

		#region OprerationSuccessfull()
		private void OprerationSuccessfull()
		{
			this.Dispatcher.Invoke((Action)delegate() { Unlock(); });
		}
		#endregion

		#region OprerationError()
		private void OprerationError(Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				Unlock();
			});
		}
		#endregion

		#endregion
	}
}
