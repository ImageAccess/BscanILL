using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace BscanILL.UI.Converters
{
	#region class MaintenanceBrushConverter
	/*[ValueConversion(typeof(MaintenanceLocal), typeof(Brush))]
	class MaintenanceBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			MaintenanceLocal maintenance = (MaintenanceLocal)value;

			if (maintenance == null)
				return new SolidColorBrush(Colors.White);
			else if (maintenance.ExpirationDate < DateTime.Now)
				return new SolidColorBrush(Colors.Red);
			else
				return new SolidColorBrush(Colors.LightGreen);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}*/
	#endregion

	#region class MaintenanceButtonTextConverter
	/*[ValueConversion(typeof(MaintenanceLocal), typeof(string))]
	class MaintenanceButtonTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			MaintenanceLocal maintenance = (MaintenanceLocal)value;

			if (maintenance == null)
				return "Add";
			else
				return "Edit";
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}*/
	#endregion

	#region class StringToDateConverter
	[ValueConversion(typeof(string), typeof(DateTime))]
	class StringToDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DateTime date;

			if (DateTime.TryParse((string)value, out date))
				return date;
			else
				throw new Exception("Cant parse " + value + " to DateTime");
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return ((DateTime)value).ToString("yyyy-MM-dd");
		}
	}
	#endregion

	#region class IntDataConverter
	[ValueConversion(typeof(object), typeof(string))]
	public class IntDataConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			string formatString = parameter as string;

			if (formatString != null)
			{
				return string.Format(culture, formatString, value);
			}
			else
			{
				if (value is int)
					return ((int)value).ToString("#,#");
				else
					return value.ToString();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
	#endregion

	#region class BoolToCollapsedConverter
	class BoolToCollapsedConverter : IValueConverter
	{
		#region Constructors
		/// <summary>
		/// The default constructor
		/// </summary>
		public BoolToCollapsedConverter()
		{
		}
		#endregion

		#region IValueConverter Members
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((bool)value)
				return Visibility.Collapsed;
			else
				return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((Visibility)value == Visibility.Visible)
				return false;
			else
				return true;
		}
		#endregion
	}
	#endregion

	#region class BoolToVisibleConverter
	class BoolToVisibleConverter : IValueConverter
	{
		#region Constructors
		/// <summary>
		/// The default constructor
		/// </summary>
		public BoolToVisibleConverter()
		{
		}
		#endregion

		#region IValueConverter Members
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((bool)value)
				return Visibility.Visible;
			else
				return Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if ((Visibility)value == Visibility.Visible)
				return true;
			else
				return false;
		}
		#endregion
	}
	#endregion
}
