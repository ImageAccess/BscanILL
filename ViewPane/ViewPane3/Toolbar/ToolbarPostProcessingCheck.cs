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

namespace ViewPane.Toolbar
{
	[SmartAssembly.Attributes.DoNotPrune]
	[SmartAssembly.Attributes.DoNotPruneType]
	[SmartAssembly.Attributes.DoNotObfuscate]
	[SmartAssembly.Attributes.DoNotObfuscateType]
	public class ToolbarPostProcessingCheck : CheckBox
	{
		BitmapImage imageC = null, imageU = null, imageD = null;

		public static DependencyProperty ControlImageProperty = DependencyProperty.Register("ControlImage", typeof(BitmapImage), typeof(ToolbarPostProcessingCheck));

		#region constructor
		static ToolbarPostProcessingCheck()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarPostProcessingCheck), new FrameworkPropertyMetadata(typeof(ToolbarPostProcessingCheck)));
		}

		public ToolbarPostProcessingCheck()
		{
			this.IsEnabledChanged += delegate(object sender, DependencyPropertyChangedEventArgs e) { SetUi(); };
			this.Checked += delegate(object sender, RoutedEventArgs e) { SetUi(); };
			this.Unchecked += delegate(object sender, RoutedEventArgs e) { SetUi(); };
			this.Initialized += delegate(object sender, EventArgs e) { SetUi(); };
		}
		#endregion

		// PUBLIC PROPERTIES
		#region public properties

		public Uri CheckedUri { set { this.imageC = new BitmapImage(value); } }
		public Uri UncheckedUri { set { this.imageU = new BitmapImage(value); } }
		public Uri DisabledUri { set { this.imageD = new BitmapImage(value); } }

		public BitmapImage ControlImage
		{
			get { return (BitmapImage)GetValue(ControlImageProperty); }
			set { SetValue(ControlImageProperty, value); }
		}

		#endregion

		// PRIVATE METHODS
		#region private methods

		#region SetUi()
		void SetUi()
		{
			if (this.IsEnabled)
			{
				if (this.IsChecked.Value)
					this.ControlImage = imageC;
				else
					this.ControlImage = imageU;
			}
			else
			{
				this.ControlImage = imageD;
			}
		}
		#endregion

		#endregion
	}
}
