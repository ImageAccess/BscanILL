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
	public class ToolbarButton : Button
	{
		public static DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapSource), typeof(ToolbarButton), new PropertyMetadata(null));

		static ToolbarButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolbarButton), new FrameworkPropertyMetadata(typeof(ToolbarButton)));
		}

		public ToolbarButton()
		{
		}

		public ToolbarButton(BitmapSource bitmap, string hint)
			:this()
		{
			this.Image = bitmap;
			this.ToolTip = hint;
		}

		public BitmapSource Image
		{
			get { return (BitmapSource)GetValue(ImageProperty); }
			set { SetValue(ImageProperty, value); }
		}

		private BitmapImage CreateBitmap(Uri source, int preferredHeight)
		{
			BitmapImage thumbnail = new BitmapImage();

			thumbnail.BeginInit();
			thumbnail.DecodePixelHeight = preferredHeight;
			thumbnail.CacheOption = BitmapCacheOption.OnLoad;
			thumbnail.UriSource = source;
			thumbnail.EndInit();

			return thumbnail;
		}
	}
}
