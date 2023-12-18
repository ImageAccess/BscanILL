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
using BscanILL.Hierarchy;

namespace BscanILL.UI.Frames.Edit
{
	/// <summary>
	/// Interaction logic for ControlPanel.xaml
	/// </summary>
	public partial class ControlPanel : UserControl
	{
		public event BscanILL.Misc.VoidHnd		DoneClick;
		public event BscanILL.Misc.VoidHnd		CurrentOnlyClick;
		public event FrameEditUi.RunAutoHnd		RunAutoItClick;
		public event FrameEditUi.RunManualHnd	ApplyManualItClick;

		public event BscanILL.Misc.VoidHnd		ItSettingsClick;
		public event BscanILL.Misc.VoidHnd		ChangeDependencyClick;
		public event BscanILL.Misc.VoidHnd		ResetCurrentClick;
		public event BscanILL.Misc.VoidHnd		ResetAllClick;
		public event BscanILL.Misc.VoidHnd		HelpClick;

		public event BscanILL.Misc.VoidHnd		SkipItClick;
		public event BscanILL.Misc.VoidHnd		ShowResultsClick;
        public event BscanILL.Misc.VoidHnd      UndoImageChangeClick;
        public event BscanILL.Misc.VoidHnd      RotateCCVClick;
		public event BscanILL.Misc.VoidHnd		RotateCVClick;
		public event BscanILL.Misc.VoidHnd		Rotate90CVClick;
		public event BscanILL.Misc.VoidHnd		Rotate90CCVClick;

		#region ControlPanel()
		public ControlPanel()
		{
			InitializeComponent();

			this.autoItControl.ExpandRequest += delegate()
			{
				this.autoItControl.IsExpanded = true;
				this.manualItControl.IsExpanded = false;
			};
			this.manualItControl.ExpandRequest += delegate()
			{
				this.autoItControl.IsExpanded = false;
				this.manualItControl.IsExpanded = true;
			};

			this.autoItControl.RunClick += delegate(FrameEditUi.ItSelection itAutoSelection, FrameEditUi.ItApplyTo applyTo)
			{
				if (RunAutoItClick != null)
					RunAutoItClick(itAutoSelection, applyTo);
			};

			this.manualItControl.ApplyClick += delegate(FrameEditUi.ItSelection itManualSelection, FrameEditUi.ItApplyTo applyTo, FrameEditUi.ItApplyToPages applyToPages)
			{
				if (ApplyManualItClick != null)
					ApplyManualItClick(itManualSelection, applyTo, applyToPages);
			};

		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		public bool ItAutoSelected { get { return (this.autoItControl.ItSelection != FrameEditUi.ItSelection.None); } }
		public bool ItManualSelected { get { return (this.manualItControl.ItSelection != FrameEditUi.ItSelection.None); } }
		public bool IsCurrentOnlyEbanled { set { buttonCurrentOnly.IsEnabled = value; } }
		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
			this.autoItControl.Reset();
			this.manualItControl.Reset();

			this.autoItControl.IsExpanded = false;
			this.manualItControl.IsExpanded = true;
		}
		#endregion

		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Settings_Click()
		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			if (ItSettingsClick != null)
				ItSettingsClick();
		}
		#endregion

		#region ChangeDependency_Click()
		private void ChangeDependency_Click(object sender, RoutedEventArgs e)
		{
			if (ChangeDependencyClick != null)
				ChangeDependencyClick();
		}
		#endregion

		#region ResetAllSettings_Click()
		private void ResetAllSettings_Click(object sender, RoutedEventArgs e)
		{
			if (this.ResetAllClick != null)
				this.ResetAllClick();
		}
		#endregion

		#region ResetCurrentSettings_Click()
		private void ResetCurrentSettings_Click(object sender, RoutedEventArgs e)
		{
			if (this.ResetCurrentClick != null)
				this.ResetCurrentClick();
		}
		#endregion

		#region Help_Click()
		private void Help_Click(object sender, RoutedEventArgs e)
		{
			if (HelpClick != null)
				HelpClick();
		}
		#endregion

		private void UndoImageChange_Click(object sender, RoutedEventArgs e)
		{
            if (UndoImageChangeClick != null)
                UndoImageChangeClick();            
		}

		private void RotateCCV_Click(object sender, RoutedEventArgs e)
		{
            if (RotateCCVClick != null)
                RotateCCVClick();              
		}

		private void RotateCV_Click(object sender, RoutedEventArgs e)
		{
			if (RotateCVClick != null)
				RotateCVClick();
		}

		private void Rotate90CV_Click(object sender, RoutedEventArgs e)
		{
			if (Rotate90CVClick != null)
				Rotate90CVClick();
		}

		private void Rotate90CCV_Click(object sender, RoutedEventArgs e)
		{
			if (Rotate90CCVClick != null)
				Rotate90CCVClick();
		}

		#region Done_Click()
		private void Done_Click(object sender, RoutedEventArgs e)
		{
			if (this.DoneClick != null)
				this.DoneClick();
		}
		#endregion

		#region CurrentOnly_Click()
		private void CurrentOnly_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentOnlyClick != null)
				CurrentOnlyClick();
		}
		#endregion

		#region SkipIt_Click()
		private void SkipIt_Click(object sender, RoutedEventArgs e)
		{
			if (this.SkipItClick != null)
				this.SkipItClick();
		}
		#endregion

		#region ShowResults_Click()
		private void ShowResults_Click(object sender, RoutedEventArgs e)
		{
			if (ShowResultsClick != null)
				ShowResultsClick();
		}
		#endregion

		#endregion

	}
}
