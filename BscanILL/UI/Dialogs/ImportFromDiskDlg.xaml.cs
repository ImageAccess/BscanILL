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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;

namespace BscanILL.UI.Dialogs
{
	/// <summary>
	/// Interaction logic for ImportFromDiskDlg.xaml
	/// </summary>
	public partial class ImportFromDiskDlg : Window
	{
		ObservableCollection<FileInfo> collection = new ObservableCollection<FileInfo>();


		#region constructor
		public ImportFromDiskDlg()
		{
			InitializeComponent();

			collection.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Collection_CollectionChanged);
			listBox.ItemsSource = collection;
		}
		#endregion


		#region class FileNamesComparator
		public class FileNamesComparator : IComparer<string>
		{
			public int Compare(string s1, string s2)
			{
				return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase);
			}
		}
		#endregion


		// PRIVATE PROPERTIES
		#region private properties

		#region SelectedFiles
		List<FileInfo> SelectedFiles
		{
			get
			{
				List<FileInfo>  list = new List<FileInfo>();
				
				foreach (object obj in listBox.SelectedItems)
					if (obj is FileInfo)
						list.Add((FileInfo)obj);

				return list;
			}
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Open()
		public ObservableCollection<FileInfo> Open()
		{
			if (this.ShowDialog() == true)
				return this.collection;
			else
				return null;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region OK_Click()
		private void OK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		#endregion

		#region Up_Click()
		private void Up_Click(object sender, RoutedEventArgs e)
		{
			List<FileInfo> selectedFiles = this.SelectedFiles;

			if (selectedFiles.Count == 1 && collection.Contains(selectedFiles[0]) && collection.IndexOf(selectedFiles[0]) > 0)
			{
				FileInfo file = selectedFiles[0];
				int index = collection.IndexOf(file);

				collection.Remove(file);
				collection.Insert(index - 1, file);
				listBox.SelectedItem = file;
			}
		}
		#endregion

		#region Down_Click()
		private void Down_Click(object sender, RoutedEventArgs e)
		{
			List<FileInfo> selectedFiles = this.SelectedFiles;

			if (selectedFiles.Count == 1 && collection.Contains(selectedFiles[0]) && collection.IndexOf(selectedFiles[0]) < collection.Count - 1)
			{
				FileInfo file = selectedFiles[0];
				int index = collection.IndexOf(file);

				collection.Remove(file);
				collection.Insert(index + 1, file);
				listBox.SelectedItem = file;
			}
		}
		#endregion

		#region Browse_Click()
		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog ofd = new OpenFileDialog();

				string importFrom;

				if (BscanILL.Misc.Io.DirectoryExists(Properties.Settings.Default.ImportDlgInitialDirectory))
					importFrom = Properties.Settings.Default.ImportDlgInitialDirectory;
				else
					importFrom = @"c:\";

				// True for all scans except replacement scans...
				ofd.Multiselect = true;
				ofd.Title = "Import Selected Image Files";
				ofd.CheckFileExists = true;
				ofd.CheckPathExists = true;
				ofd.Filter = "Image Files(*.JPG;*.JPEG;*.TIF;*.TIFF;*.PNG;*.GIF;*.BMP;*.PDF)|*.JPG;*.JPEG;*.TIF;*.TIFF;*.PNG;*.GIF;*.BMP;*.PDF|All files (*.*)|*.*";
				ofd.InitialDirectory = importFrom;

				if (ofd.ShowDialog(this) == true && ofd.FileNames.Length > 0)
				{
					List<string> files = new List<string>(ofd.FileNames);
					files.Sort(new FileNamesComparator());

					foreach (string filePath in ofd.FileNames)
					{
						collection.Add(new FileInfo(filePath));
					}
					
					Properties.Settings.Default.ImportDlgInitialDirectory = new FileInfo(ofd.FileNames[0]).Directory.FullName;
					Properties.Settings.Default.Save();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(BscanILL.Misc.Misc.GetErrorMessage(ex), "Bscan ILL", MessageBoxButton.OK, MessageBoxImage.Error); 
			}
		}
		#endregion

		#region RemoveSelected_Click
		private void RemoveSelected_Click(object sender, RoutedEventArgs e)
		{
			List<FileInfo> selectedFiles = this.SelectedFiles;

			foreach (FileInfo file in selectedFiles)
				collection.Remove(file);
		}
		#endregion

		#region RemoveSelected_Click
		private void RemoveAll_Click(object sender, RoutedEventArgs e)
		{
			collection.Clear();
		}
		#endregion

		#region Collection_CollectionChanged()
		void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			ListBoxChanged();
		}
		#endregion

		#region ListBox_SelectionChanged()
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ListBoxChanged();
		}
		#endregion

		#region ListBoxChanged()
		private void ListBoxChanged()
		{
			List<FileInfo> selectedFiles = this.SelectedFiles;

			buttonImport.IsEnabled = collection.Count > 0;
			buttonRemoveAll.IsEnabled = collection.Count > 0;
			buttonRemoveSelected.IsEnabled = selectedFiles.Count > 0;

			if (selectedFiles.Count == 1 && collection.Contains(selectedFiles[0]) && collection.IndexOf(selectedFiles[0]) > 0)
				buttonUp.IsEnabled = true;
			else
				buttonUp.IsEnabled = false;

			if (selectedFiles.Count == 1 && collection.Contains(selectedFiles[0]) && collection.IndexOf(selectedFiles[0]) < collection.Count - 1)
				buttonDown.IsEnabled = true;
			else
				buttonDown.IsEnabled = false;
		}
		#endregion

		#region FormLoaded()
		private void FormLoaded(object sender, RoutedEventArgs e)
		{
			InvalidateVisual();
		}
		#endregion

		#endregion

	}
}
