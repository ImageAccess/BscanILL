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

namespace BscanILL.UI.Controls
{
	/// <summary>
	/// Interaction logic for CleanUpControl.xaml
	/// </summary>
	public partial class CleanUpControl
	{
		public delegate void FinishedHnd();
		public event FinishedHnd Finished;

		#region constructor
		public CleanUpControl()
		{
			InitializeComponent();

			this.DataContext = this;
		}
		#endregion

		//PUBLIC PROPERTIES
		#region public properties

		#endregion


		//PUBLIC METHODS
		#region public methods

		#region Run()
		public void Run()
		{
			try
			{
				Lock();

				Thread thread = new Thread(new ThreadStart(CleanUpThread));
				thread.Name = "CleanUpControl, CleanUp_Click()";
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

		#endregion


		//PRIVATE METHODS
		#region private methods

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

				cleanUp.Execute(null, new DirectoryInfo(_settings.General.ArticlesDir));
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
			this.Dispatcher.Invoke((Action)delegate() 
			{ 
				Unlock(); 

				if(this.Finished != null)
					this.Finished();
			});
		}
		#endregion

		#region OprerationError()
		private void OprerationError(Exception ex)
		{
			this.Dispatcher.Invoke((Action)delegate()
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "BscanILL", MessageBoxButton.OK, MessageBoxImage.Error);
				Unlock();

				if(this.Finished != null)
					this.Finished();
			});
		}
		#endregion

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{

		}

		#endregion
	}
}
