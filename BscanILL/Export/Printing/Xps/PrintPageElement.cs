using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BscanILL.Export.Printing.Xps
{
	class PrintPageElement : UserControl
	{

		public PrintPageElement(string imageFile, double inchWidth, double inchHeight)
		{
			Grid grid = new Grid();

			this.AddChild(grid);

			Image image = new Image();
			BitmapImage bitmapImage = new BitmapImage();

			bitmapImage.BeginInit();
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.UriSource = new Uri(imageFile, UriKind.Absolute);
			bitmapImage.EndInit();

			if ((bitmapImage.PixelWidth / bitmapImage.DpiX < inchWidth) && (bitmapImage.PixelHeight / bitmapImage.DpiY < inchHeight))
				image.Stretch = System.Windows.Media.Stretch.None;

			image.Source = bitmapImage;
			grid.Children.Add(image);
		}

		/// <summary>
		/// for printing test sheets
		/// </summary>
		/// <param name="inchWidth"></param>
		/// <param name="inchHeight"></param>
		public PrintPageElement(double inchWidth, double inchHeight)
		{
			Grid grid = new Grid();

			this.AddChild(grid);

			string width = ((int)inchWidth == inchWidth) ? string.Format("{0}", (int)inchWidth) : string.Format("{0:0.0}", inchWidth);
			string height = ((int)inchHeight == inchHeight) ? string.Format("{0}", (int)inchHeight) : string.Format("{0:0.0}", inchHeight);

			TextBlock textBlock = new TextBlock();
			textBlock.Inlines.Clear();
			textBlock.FontFamily = new System.Windows.Media.FontFamily("Times New Roman");
			textBlock.FontSize = 18;
			textBlock.Text = "Bscan ILL Test Sheet" + Environment.NewLine + string.Format("Size: {0}\" x {1}\"", width, height);
			textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
			textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;
			textBlock.TextAlignment = System.Windows.TextAlignment.Center;

			Border border = new Border();
			border.Width = inchWidth * 96;
			border.Height = inchHeight * 96;
			border.BorderThickness = new System.Windows.Thickness(1);
			border.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
			border.Child = textBlock;

			grid.Children.Add(border);
		}

		/// <summary>
		/// empty page (for duplex odd page)
		/// </summary>
		/// <param name="inchWidth"></param>
		/// <param name="inchHeight"></param>
		public PrintPageElement()
		{
			Grid grid = new Grid();

			grid.Width = 1 * 96;
			grid.Height = 1 * 96;
			this.AddChild(grid);
		}

		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
		}
	
	}
}
