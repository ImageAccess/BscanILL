using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Data;


namespace BscanILL.UI.Converters
{

	#region class TabContentToPathConverter
	public class TabContentToPathConverter : IValueConverter
	{

		#region Convert()
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int height = 25;

			FrameworkElement cp = (FrameworkElement)value;
			double width = cp.ActualWidth + 32;
			bool isStroked = true;

			//cp.Height = KicConverters.Height;

			PathFigure pathFigure = new PathFigure();
			pathFigure.StartPoint = new Point(0, height);

			PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();

			pathSegmentCollection.Add(new BezierSegment(new Point(10, height - 3), new Point(0, 0), new Point(10, 0), isStroked));
			pathSegmentCollection.Add(new LineSegment(new Point(width - 10, 0), isStroked));
			pathSegmentCollection.Add(new BezierSegment(new Point(width, 0), new Point(width - 10, height - 3), new Point(width, height), isStroked));
			pathSegmentCollection.Add(new LineSegment(new Point(width, height), isStroked));

			pathFigure.Segments = pathSegmentCollection;
			pathFigure.IsClosed = false;

			PathFigureCollection pathFigureCollection = new PathFigureCollection();
			pathFigureCollection.Add(pathFigure);

			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures = pathFigureCollection;

			return pathGeometry;
		}
		#endregion

		#region ConvertBack()
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion

	}
	#endregion

	#region class TabImageContentToPathConverter
	public class TabImageContentToPathConverter : IValueConverter
	{

		#region Convert()
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			FrameworkElement cp = (FrameworkElement)value;
			double width = cp.ActualWidth + 24;
			int height = System.Convert.ToInt32(cp.ActualHeight + 6);
			bool isStroked = true;

			//cp.Height = KicConverters.Height;

			PathFigure pathFigure = new PathFigure();
			pathFigure.StartPoint = new Point(0, height);

			PathSegmentCollection pathSegmentCollection = new PathSegmentCollection();

			pathSegmentCollection.Add(new BezierSegment(new Point(15, height - 3), new Point(0, 0), new Point(20, 0), isStroked));
			pathSegmentCollection.Add(new LineSegment(new Point(width - 20, 0), isStroked));
			pathSegmentCollection.Add(new BezierSegment(new Point(width, 0), new Point(width - 15, height - 3), new Point(width, height), isStroked));
			pathSegmentCollection.Add(new LineSegment(new Point(width, height), isStroked));

			pathFigure.Segments = pathSegmentCollection;
			pathFigure.IsClosed = false;

			PathFigureCollection pathFigureCollection = new PathFigureCollection();
			pathFigureCollection.Add(pathFigure);

			PathGeometry pathGeometry = new PathGeometry();
			pathGeometry.Figures = pathFigureCollection;

			return pathGeometry;
		}
		#endregion

		#region ConvertBack()
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		#endregion

	}
	#endregion
	
}
