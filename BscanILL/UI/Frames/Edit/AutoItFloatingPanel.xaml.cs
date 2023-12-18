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

namespace BscanILL.UI.Frames.Edit
{
	/// <summary>
	/// Interaction logic for AutoItFloatingPanel.xaml
	/// </summary>
	public partial class AutoItFloatingPanel : UserControl
	{
		public event BscanILL.Misc.VoidHnd GoClick;
		public event BscanILL.Misc.VoidHnd CancelClick;


		#region constructor
		public AutoItFloatingPanel()
		{
			InitializeComponent();

			this.checkContent.IsChecked = Properties.FrameEdit.Default.IsContentChecked;
			this.checkSkew.IsChecked = Properties.FrameEdit.Default.IsDeskewChecked;
			this.checkBookfold.IsChecked = Properties.FrameEdit.Default.IsBookfoldChecked;
			this.checkFingers.IsChecked = Properties.FrameEdit.Default.IsFingersChecked;
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool ItAutoSelected { get { return (IsContentLocationChecked || IsSkewChecked || IsBookfoldChecked || IsFingerRemovalChecked); } }

		#region ItSelection
		internal BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection ItSelection
		{
			get
			{
				BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection selection = FrameEditUi.ItSelection.None;

				if (checkContent.IsChecked.Value)
					selection |= FrameEditUi.ItSelection.Content;
				if (checkSkew.IsChecked.Value)
					selection |= FrameEditUi.ItSelection.Deskew;
				if (checkBookfold.IsChecked.Value)
					selection |= FrameEditUi.ItSelection.Bookfold;
				if (checkFingers.IsChecked.Value)
					selection |= FrameEditUi.ItSelection.Fingers;

				return selection;
			}
			set
			{
				checkContent.IsChecked = ((value | FrameEditUi.ItSelection.Content) > 0);
				checkSkew.IsChecked = ((value | FrameEditUi.ItSelection.Deskew) > 0);
				checkBookfold.IsChecked = ((value | FrameEditUi.ItSelection.Bookfold) > 0);
				checkFingers.IsChecked = ((value | FrameEditUi.ItSelection.Fingers) > 0);
			}
		}
		#endregion

		#region IsContentLocationChecked
		internal bool IsContentLocationChecked
		{
			get { return (checkContent.IsChecked.Value); }
			set { checkContent.IsChecked = value; }
		}
		#endregion

		#region IsSkewChecked
		internal bool IsSkewChecked
		{
			get { return checkSkew.IsChecked.Value; }
			set { checkSkew.IsChecked = value; }
		}
		#endregion

		#region IsBookfoldChecked
		internal bool IsBookfoldChecked
		{
			get { return checkBookfold.IsChecked.Value; }
			set { checkBookfold.IsChecked = value; }
		}
		#endregion

		#region IsFingerRemovalChecked
		internal bool IsFingerRemovalChecked
		{
			get { return checkFingers.IsChecked.Value; }
			set { checkFingers.IsChecked = value; }
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
			this.IsContentLocationChecked = true;
			this.IsSkewChecked = true;
			this.IsBookfoldChecked = false;
			this.IsFingerRemovalChecked = false;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Go_Click()
		private void Go_Click(object sender, RoutedEventArgs e)
		{
			if (this.GoClick != null)
				this.GoClick();
		}
		#endregion

		#region Cancel_Click()
		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			if (this.CancelClick != null)
				this.CancelClick();
		}
		#endregion

		#region Check_CheckedChanged()
		private void Check_CheckedChanged(object sender, RoutedEventArgs e)
		{
			buttonProceed.IsEnabled = this.ItAutoSelected;

			try
			{
				if (this.IsLoaded)
				{
					Properties.FrameEdit.Default.IsContentChecked = this.checkContent.IsChecked.Value;
					Properties.FrameEdit.Default.IsDeskewChecked = this.checkSkew.IsChecked.Value;
					Properties.FrameEdit.Default.IsBookfoldChecked = this.checkBookfold.IsChecked.Value;
					Properties.FrameEdit.Default.IsFingersChecked = this.checkFingers.IsChecked.Value;
					Properties.FrameEdit.Default.Save();
				}
			}
			catch { }
		}
		#endregion

		#endregion

	}
}
