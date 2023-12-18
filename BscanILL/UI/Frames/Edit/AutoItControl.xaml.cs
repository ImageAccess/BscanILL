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
	/// Interaction logic for AutoItControl.xaml
	/// </summary>
	public partial class AutoItControl : UserControl
	{
		public event BscanILL.UI.Frames.Edit.FrameEditUi.RunAutoHnd RunClick;
		public event BscanILL.Misc.VoidHnd ExpandRequest;

		static DependencyProperty isExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(AutoItControl),
			new PropertyMetadata(true, new PropertyChangedCallback(OnIsExpandedChanged)));

		#region constructor
		public AutoItControl()
		{
			InitializeComponent();                        
		}
		#endregion


		// PUBLIC PROPERTIES
		#region public properties

		#region ItSelection
		internal BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection ItSelection
		{
			get
			{
				BscanILL.UI.Frames.Edit.FrameEditUi.ItSelection selection = FrameEditUi.ItSelection.None;

				if (checkContent.IsChecked.Value) 
					selection |= FrameEditUi.ItSelection.Content;
				if (checkDeskew.IsChecked.Value)
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
				checkDeskew.IsChecked = ((value | FrameEditUi.ItSelection.Deskew) > 0);
				checkBookfold.IsChecked = ((value | FrameEditUi.ItSelection.Bookfold) > 0);
				checkFingers.IsChecked = ((value | FrameEditUi.ItSelection.Fingers) > 0);
			}
		}
		#endregion

		#region ApplyTo
		internal FrameEditUi.ItApplyTo ApplyTo
		{
			get
			{
                if (radioApplyToBatch.IsChecked.Value)
                    return FrameEditUi.ItApplyTo.EntireBatch;
                else if (radioApplyToRestBatch.IsChecked.Value)
                    return FrameEditUi.ItApplyTo.RestOfBatch;
				else if (radioApplyToArticle.IsChecked.Value)
                    return FrameEditUi.ItApplyTo.EntireArticle;
				else if (radioApplyToRest.IsChecked.Value)
					return FrameEditUi.ItApplyTo.RestOfArticle;
				else
					return FrameEditUi.ItApplyTo.Current;
			}
			set
			{
				switch (value)
				{
					case FrameEditUi.ItApplyTo.Current: radioApplyToCurrent.IsChecked = true; break;
                    case FrameEditUi.ItApplyTo.EntireArticle: radioApplyToArticle.IsChecked = true; break;
					case FrameEditUi.ItApplyTo.RestOfArticle: radioApplyToRest.IsChecked = true; break;
                    case FrameEditUi.ItApplyTo.EntireBatch: radioApplyToBatch.IsChecked = true; break;
                    case FrameEditUi.ItApplyTo.RestOfBatch: radioApplyToRestBatch.IsChecked  = true; break;						
				}
			}
		}
		#endregion

		#region IsExpanded
		public bool IsExpanded
		{
			get { return (bool)GetValue(isExpandedProperty); }
			set { SetValue(isExpandedProperty, value); }
		}
		#endregion

		#endregion


		// PUBLIC METHODS
		#region public methods

		#region Reset()
		public void Reset()
		{
			this.ItSelection = (FrameEditUi.ItSelection.Content | FrameEditUi.ItSelection.Deskew);
            if (BscanILL.SETTINGS.Settings.Instance.General.MultiArticleSupportEnabled == false)
            {
                this.ApplyTo = FrameEditUi.ItApplyTo.EntireArticle;
            }
            else
            {
                this.ApplyTo = FrameEditUi.ItApplyTo.EntireBatch;
            }
		}
		#endregion

        #region  AdjustMultiAticleMode()
        public void AdjustMultiAticleMode()
        {
            if (BscanILL.SETTINGS.Settings.Instance.General.MultiArticleSupportEnabled == false)
            {
                this.radioApplyToBatch.Visibility = Visibility.Collapsed;
                this.radioApplyToRestBatch.Visibility = Visibility.Collapsed;
                this.radioApplyToArticle.Content = "On Entire Article";
                this.radioApplyToRest.Content = "On the Rest of Article";
                this.radioApplyToArticle.IsChecked = true;
                this.radioApplyToBatch.IsChecked = false;
            }
            else
            {
                this.radioApplyToBatch.Visibility = Visibility.Visible;
                this.radioApplyToRestBatch.Visibility = Visibility.Visible;
                this.radioApplyToArticle.Content = "On Entire Current Article";
                this.radioApplyToRest.Content = "On the Rest of Current Article";
                this.radioApplyToArticle.IsChecked = false;
                this.radioApplyToBatch.IsChecked = true;
            }            
        }
        #endregion

        #endregion


        // PRIVATE METHODS
		#region private methods

		#region Run_Click()
		private void Run_Click(object sender, RoutedEventArgs e)
		{
			if (this.RunClick != null)
				this.RunClick(this.ItSelection, this.ApplyTo);
		}
		#endregion

		#region Expand_Click()
		private void Expand_Click(object sender, RoutedEventArgs e)
		{
			if (this.ExpandRequest != null)
				this.ExpandRequest();
		}
		#endregion

		#region OnIsExpandedChanged()
		private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			AutoItControl instance = (AutoItControl)sender;

			if ((bool)args.NewValue)
			{
				instance.gridCollapsed.Visibility = System.Windows.Visibility.Collapsed;
				instance.gridExpanded.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				instance.gridCollapsed.Visibility = System.Windows.Visibility.Visible;
				instance.gridExpanded.Visibility = System.Windows.Visibility.Collapsed;
			}
		}
		#endregion

		#endregion
	}
}
