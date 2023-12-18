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
using ViewPane.Hierarchy;

namespace ViewPane2App
{
	/// <summary>
	/// Interaction logic for ItResultsWindow.xaml
	/// </summary>
	public partial class ItResultsWindow : Window
	{
		volatile bool close = false;


		#region constructor
		public ItResultsWindow()
		{
			InitializeComponent();
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public ViewPane.ItResults.ItResultsPanel ItResultsPanel { get { return this.panel; } }

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Dispose()
		public void Dispose()
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.close = true;
				this.ItResultsPanel.Dispose();
				this.Close();
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.close = true;
					this.ItResultsPanel.Dispose();
					this.Close();
				});
			}
		}
		#endregion

		#region LoadImages()
		public void LoadImages(List<VpImage> vpImages, VpImage selectedImage)
		{
			if (this.Dispatcher.CheckAccess())
			{
				foreach (VpImage vpImage in vpImages)
				{
					this.ItResultsPanel.AddImage(vpImage);

					if (vpImage == selectedImage)
						this.ItResultsPanel.SelectImage(vpImage);
				}
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					foreach (VpImage vpImage in vpImages)
					{
						this.ItResultsPanel.AddImage(vpImage);

						if (vpImage == selectedImage)
							this.ItResultsPanel.SelectImage(vpImage);
					}
				});
			}
		}
		#endregion

		#region SelectImage()
		public void SelectImage(VpImage vpImage)
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.ItResultsPanel.SelectImage(vpImage);
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.ItResultsPanel.SelectImage(vpImage);
				});
			}
		}
		#endregion

		#region Hide()
		new public void Hide()
		{
			if (this.Dispatcher.CheckAccess())
			{
				this.ItResultsPanel.Clear();
				base.Hide();
			}
			else
			{
				this.Dispatcher.Invoke((Action)delegate()
				{
					this.ItResultsPanel.Clear();
					base.Hide();
				});
			}
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Window_Closing()
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.close == false)
			{
				this.ItResultsPanel.Clear();
				base.Hide();
				e.Cancel = true;
			}
		}
		#endregion

		#endregion

	}
}
