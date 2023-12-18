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
	/// Interaction logic for ManualItControl.xaml
	/// </summary>
	public partial class ManualItControl : UserControl
	{
		public event FrameEditUi.RunManualHnd	ApplyClick;
		public event BscanILL.Misc.VoidHnd		ExpandRequest;

		static DependencyProperty isExpandedProperty = DependencyProperty.Register("IsExpanded", typeof(bool), typeof(ManualItControl),
			new PropertyMetadata(true, new PropertyChangedCallback(OnIsExpandedChanged)));
		static DependencyProperty applyToProperty = DependencyProperty.Register("ApplyTo", typeof(FrameEditUi.ItApplyTo), typeof(ManualItControl),
            new PropertyMetadata(FrameEditUi.ItApplyTo.EntireArticle, new PropertyChangedCallback(OnItApplyToChanged)));


		#region constructor
		public ManualItControl()
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
			get { return (FrameEditUi.ItApplyTo)GetValue(applyToProperty); }
			set { SetValue(applyToProperty, value); }
		}
		#endregion

		#region ItApplyToPages
		internal FrameEditUi.ItApplyToPages ApplyToPages
		{
			get
			{
				if (ApplyTo == FrameEditUi.ItApplyTo.RestOfArticle)
				{
					if (radioApplyLeftRestCurrentArticle.IsChecked.Value)   
						return FrameEditUi.ItApplyToPages.Left;
                    else if (radioApplyRightRestCurrentArticle.IsChecked.Value)
						return FrameEditUi.ItApplyToPages.Right;
					else
						return FrameEditUi.ItApplyToPages.All;
				}
				else
                if( ApplyTo == FrameEditUi.ItApplyTo.EntireArticle)
				{
                    if (radioApplyLeftEntireCurrentArticle.IsChecked.Value)
						return FrameEditUi.ItApplyToPages.Left;
                    else if (radioApplyRightEntireCurrentArticle.IsChecked.Value)
						return FrameEditUi.ItApplyToPages.Right;
					else
						return FrameEditUi.ItApplyToPages.All;
				}
                else
                if( ApplyTo == FrameEditUi.ItApplyTo.RestOfBatch)
                {
                    if (radioApplyLeftRestBatch.IsChecked.Value)
                        return FrameEditUi.ItApplyToPages.Left;
                    else if (radioApplyRightRestBatch.IsChecked.Value)
                        return FrameEditUi.ItApplyToPages.Right;
                    else
                        return FrameEditUi.ItApplyToPages.All;
                }
                else
                {
                    //entire batch
                    if (radioApplyLeftEntireBatch.IsChecked.Value)
                        return FrameEditUi.ItApplyToPages.Left;
                    else if (radioApplyRightEntireBatch.IsChecked.Value)
                        return FrameEditUi.ItApplyToPages.Right;
                    else
                        return FrameEditUi.ItApplyToPages.All;
                }

			}
			set
			{
				switch (value)
				{
                    case FrameEditUi.ItApplyToPages.Left: radioApplyLeftRestCurrentArticle.IsChecked = true; radioApplyLeftEntireCurrentArticle.IsChecked = true; radioApplyLeftRestBatch.IsChecked = true; radioApplyLeftEntireBatch.IsChecked = true; break;
                    case FrameEditUi.ItApplyToPages.Right: radioApplyRightRestCurrentArticle.IsChecked = true; radioApplyRightEntireCurrentArticle.IsChecked = true; radioApplyRightRestBatch.IsChecked = true; radioApplyRightEntireBatch.IsChecked = true; break;
                    case FrameEditUi.ItApplyToPages.All: radioApplyLeftAndRightRestCurrentArticle.IsChecked = true; radioApplyLeftAndRightEntireCurrentArticle.IsChecked = true; radioApplyLeftAndRightRestBatch.IsChecked = true; radioApplyLeftAndRightEntireBatch.IsChecked = true; break;
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
            this.ApplyTo = FrameEditUi.ItApplyTo.EntireArticle;
			this.ApplyToPages = FrameEditUi.ItApplyToPages.All;
		}
		#endregion

		#region Collapse()
		public void Collapse()
		{
			gridCollapsed.Visibility = System.Windows.Visibility.Visible;
			gridExpanded.Visibility = System.Windows.Visibility.Collapsed;
		}
		#endregion

		#region Expand()
		public void Expand()
		{
			gridCollapsed.Visibility = System.Windows.Visibility.Collapsed;
			gridExpanded.Visibility = System.Windows.Visibility.Visible;
		}
		#endregion


        #region  AdjustMultiAticleMode()
        public void AdjustMultiAticleMode()
        {
            if (BscanILL.SETTINGS.Settings.Instance.General.MultiArticleSupportEnabled == false)
            {
                  this.radioApplyToEntireBatch.Visibility = Visibility.Collapsed;
                  this.gridPagesEntireBatch.Visibility = Visibility.Collapsed;
                  this.radioApplyToRestBatch.Visibility = Visibility.Collapsed;
                  this.gridPagesRestBatch.Visibility = Visibility.Collapsed;
                  this.radioApplyToEntireCurrentArticle.Content = "On Entire Article";
                  this.radioApplyToRestCurrentArticle.Content = "On the Rest of Article";
                  this.radioApplyToEntireCurrentArticle.IsChecked = true;
                  this.radioApplyToEntireBatch.IsChecked = false;
            }
            else
            {
                  this.radioApplyToEntireBatch.Visibility = Visibility.Visible;
                  this.gridPagesEntireBatch.Visibility = Visibility.Visible;
                  this.radioApplyToRestBatch.Visibility = Visibility.Visible;
                  this.gridPagesRestBatch.Visibility = Visibility.Visible;
                  this.radioApplyToEntireCurrentArticle.Content = "On Entire Current Article";
                  this.radioApplyToRestCurrentArticle.Content = "On the Rest of Current Article";
                  this.radioApplyToEntireCurrentArticle.IsChecked = false;
                  this.radioApplyToEntireBatch.IsChecked = true;
            }
        }
        #endregion
		#endregion


		// PRIVATE METHODS
		#region private methods

		#region Run_Click()
		private void Run_Click(object sender, RoutedEventArgs e)
		{
			if (this.ApplyClick != null)
				this.ApplyClick(this.ItSelection, this.ApplyTo, this.ApplyToPages);
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
			ManualItControl instance = (ManualItControl)sender;

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

		#region OnItApplyToChanged()
		private static void OnItApplyToChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			ManualItControl instance = (ManualItControl)sender;

			if (((FrameEditUi.ItApplyTo)args.NewValue) == FrameEditUi.ItApplyTo.RestOfArticle)
			{
				instance.radioApplyToRestCurrentArticle.IsChecked = true;
                instance.gridPagesRestCurrentArticle.Visibility = Visibility.Visible;
                instance.gridPagesEntireCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesRestBatch.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireBatch.Visibility = Visibility.Collapsed;
			}
			else
            if (((FrameEditUi.ItApplyTo)args.NewValue) == FrameEditUi.ItApplyTo.EntireArticle)
			{
                instance.radioApplyToEntireCurrentArticle.IsChecked = true;
                instance.gridPagesRestCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireCurrentArticle.Visibility = Visibility.Visible;
                instance.gridPagesRestBatch.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireBatch.Visibility = Visibility.Collapsed;
			}
            else
            if (((FrameEditUi.ItApplyTo)args.NewValue) == FrameEditUi.ItApplyTo.RestOfBatch)
            {
                instance.radioApplyToRestBatch.IsChecked = true;
                instance.gridPagesRestCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesRestBatch.Visibility = Visibility.Visible;
                instance.gridPagesEntireBatch.Visibility = Visibility.Collapsed;
            }
            else
            {
                //FrameEditUi.ItApplyTo.EntireBatch
                instance.radioApplyToEntireBatch.IsChecked = true;
                instance.gridPagesRestCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireCurrentArticle.Visibility = Visibility.Collapsed;
                instance.gridPagesRestBatch.Visibility = Visibility.Collapsed;
                instance.gridPagesEntireBatch.Visibility = Visibility.Visible;
            }
		}
		#endregion

		#region RadioApply_Checked()
		private void RadioApply_Checked(object sender, RoutedEventArgs e)
		{
            if (sender == radioApplyToRestCurrentArticle)
            {
				this.ApplyTo = FrameEditUi.ItApplyTo.RestOfArticle;
            }
            else
            if (sender == radioApplyToEntireCurrentArticle)
            {
                this.ApplyTo = FrameEditUi.ItApplyTo.EntireArticle;
            }
            else
            if (sender == radioApplyToRestBatch)
            {
                this.ApplyTo = FrameEditUi.ItApplyTo.RestOfBatch;
            }
            else
            if (sender == radioApplyToEntireBatch)
            {
                this.ApplyTo = FrameEditUi.ItApplyTo.EntireBatch;
            }
		}
		#endregion

		#endregion

	}
}
